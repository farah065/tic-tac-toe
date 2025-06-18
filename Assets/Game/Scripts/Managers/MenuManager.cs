using UnityEngine;
using TMPro;
using Mirror;

public class MenuManager : MonoBehaviour
{
    private NetworkRoomManagerTicTacToe _networkManager;
    private GameObject _IPInputField;
    [SerializeField] private GameObject _mainMenuPanel;
    [SerializeField] private GameObject _IPConnectionPanel;

    private void Awake()
    {
        _networkManager = FindFirstObjectByType<NetworkRoomManagerTicTacToe>();
        if (_networkManager == null)
        {
            Debug.LogError("NetworkManagerTicTacToe not found in the scene.");
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
        _networkManager.StartHost();
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

    public void Join()
    {
        // set the network address to the IP address entered by the player
        _networkManager.networkAddress = _IPInputField.GetComponent<TMP_InputField>().text;
        _networkManager.StartClient();
    }

    public void BackToMenu()
    {
        _IPConnectionPanel.SetActive(false);
        _mainMenuPanel.SetActive(true);
    }
}
