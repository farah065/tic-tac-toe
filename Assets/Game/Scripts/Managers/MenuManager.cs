using UnityEngine;
using TMPro;
using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using UnityEngine.UI;

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
    readonly Dictionary<long, ServerResponse> discoveredServers = new Dictionary<long, ServerResponse>();

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

    void OnGUI()
    {
        
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
        foreach (ServerResponse info in discoveredServers.Values)
        {
            ServerResponse serverInfo = info;
            // instantiate the button prefab
            GameObject buttonObject = Instantiate(serverButtonPrefab, scrollViewContent, false);
            TMP_Text text = buttonObject.gameObject.GetComponentInChildren<TMP_Text>();
            text.text = info.EndPoint.Address.ToString();

            // Add a listener to the button to connect to the server
            Button button = buttonObject.GetComponent<Button>();
            button.onClick.AddListener(() => Connect(serverInfo));
        }
    }

    private void Connect(ServerResponse info)
    {
        _networkDiscovery.StopDiscovery();
        NetworkManager.singleton.StartClient(info.uri);
    }

    public void FindServer()
    {
        discoveredServers.Clear();
        _networkDiscovery.StartDiscovery();
    }

    public void OnDiscoveredServer(ServerResponse info)
    {
        // Note that you can check the versioning to decide if you can connect to the server or not using this method
        discoveredServers[info.serverId] = info;

        if (NetworkManager.singleton == null)
            return;

        if (!NetworkClient.isConnected && !NetworkServer.active && !NetworkClient.active)
            DrawDiscoveredServersGUI();
    }

    public void BackToMenu()
    {
        _IPConnectionPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }
}
