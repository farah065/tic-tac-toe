using UnityEngine;
using TMPro;
using Mirror;
using System.Collections.Generic;
using UnityEngine.UI;
using System.Collections;
using Mirror.Discovery;

public class MenuManager : MonoBehaviour
{
    private NetworkRoomManagerTicTacToe _networkManager;
    private NetworkDiscoveryTicTacToe _networkDiscovery;
    private GameObject _IPInputField;
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _IPConnectionPanel;
    [SerializeField] private Transform scrollViewContent;
    [SerializeField] private GameObject serverButtonPrefab;

    Vector2 scrollViewPos = Vector2.zero;
    readonly Dictionary<long, (DiscoveryResponse response, float lastSeenTime)> discoveredServers = new();

    private void Awake()
    {
        _networkManager = FindFirstObjectByType<NetworkRoomManagerTicTacToe>();
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManagerTicTacToe not found in the scene.");
        }
        else
        {
            _networkDiscovery = FindFirstObjectByType<NetworkDiscoveryTicTacToe>();
            if (_networkDiscovery == null)
            {
                Debug.LogError("NetworkDiscoveryTicTacToe not found in the scene.");
            }
        }

        if (_mainMenuPanel == null)
        {
            Debug.LogError("Main Menu Panel is not assigned in the inspector.");
        }

        if (_IPConnectionPanel == null)
        {
            Debug.LogError("IP Connection Panel is not assigned in the inspector.");
        }
        else
        {
            _IPInputField = _IPConnectionPanel.transform.GetComponentInChildren<TMP_InputField>().gameObject;
            if (_IPInputField == null)
            {
                Debug.LogError("IP Input Field not found in the IP Connection Panel.");
            }
        }
    }

    private void Start()
    {
        _IPConnectionPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }

    public void Host()
    {
        discoveredServers.Clear();
        _networkManager.StartHost();
        _networkDiscovery.AdvertiseServer();
    }

    public void DisplayJoinScreen()
    {
        _mainMenuPanel.SetActive(false);
        _IPConnectionPanel.SetActive(true);
        FindServer();
        StartCoroutine(UpdateDiscoveryGUI());
    }

    public void Quit()
    {
        Application.Quit();
    }

    public void JoinIP()
    {
        // set the network address to the IP address entered by the player
        _networkManager.networkAddress = _IPInputField.GetComponent<TMP_InputField>().text;
        _networkManager.StartClient();
    }

    public void DrawDiscoveredServersGUI()
    {
        if (scrollViewContent == null)
        {
            Debug.LogError("Scroll View Content is not assigned in the inspector.");
            return;
        }
        // Clear the previous content
        foreach (Transform child in scrollViewContent)
        {
            Destroy(child.gameObject);
        }
        // Create a button for each discovered server
        foreach ((DiscoveryResponse, float) info in discoveredServers.Values)
        {
            DiscoveryResponse serverInfo = info.Item1;
            // instantiate the button prefab
            GameObject buttonObject = Instantiate(serverButtonPrefab, scrollViewContent, false);
            TMP_Text text = buttonObject.gameObject.GetComponentInChildren<TMP_Text>();
            text.text = serverInfo.HostPlayerName + " (" + serverInfo.CurrentPlayers + "/2)";

            // Add a listener to the button to connect to the server
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(() => Connect(serverInfo));
        }
    }

    private void Connect(DiscoveryResponse info)
    {
        _networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(info.Uri);
    }

    public void FindServer()
    {
        discoveredServers.Clear();
        _networkDiscovery.StartDiscovery();
    }

    public void OnDiscoveredServer(DiscoveryResponse info)
    {
        float currentTime = Time.time;
        discoveredServers[info.ServerId] = (info, currentTime);

        if (NetworkManager.singleton == null)
            return;

        if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
            DrawDiscoveredServersGUI();
    }

    public void RemoveExpiredServers()
    {
        float currentTime = Time.time;
        float timeout = 1.1f;

        List<long> expiredKeys = new();
        foreach (var kvp in discoveredServers)
        {
            Debug.Log("Current time: " + currentTime + ", kvp.Value.lastSeenTime: " + kvp.Value.lastSeenTime);
            if (currentTime - kvp.Value.lastSeenTime > timeout)
            {
                expiredKeys.Add(kvp.Key);
            }
        }
        foreach (long key in expiredKeys)
        {
            discoveredServers.Remove(key);
        }
    }

    public IEnumerator UpdateDiscoveryGUI()
    {
        yield return new WaitForSeconds(1.0f);
        RemoveExpiredServers();
        DrawDiscoveredServersGUI();
        StartCoroutine(UpdateDiscoveryGUI());
    }

    public void BackToMenu()
    {
        _networkDiscovery.StopDiscovery();
        StopCoroutine(UpdateDiscoveryGUI());
        _IPConnectionPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }
}
