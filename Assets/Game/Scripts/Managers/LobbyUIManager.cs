using UnityEngine;
using System.Collections.Generic;
using Mirror;
using System.Collections;
using TMPro;
using UnityEngine.UI;

public class LobbyUIManager : NetworkBehaviour
{
    private NetworkRoomPlayerTicTacToe _networkRoomPlayer;
    private NetworkRoomManagerTicTacToe _networkRoomManager;
    [SerializeField] private List<TMP_Text> _playerNames;
    [SerializeField] private List<TMP_Text> _playerReadyStates;
    [SerializeField] private Button _startButton;

    private IEnumerator Start()
    {
        // wait until the local player has been loaded in
        yield return new WaitUntil(() => NetworkClient.localPlayer != null);

        NetworkIdentity localPlayerIdentity = NetworkClient.localPlayer.GetComponent<NetworkIdentity>();
        if (localPlayerIdentity != null)
        {
            _networkRoomPlayer = localPlayerIdentity.GetComponent<NetworkRoomPlayerTicTacToe>();
        }

        if (_networkRoomPlayer == null)
        {
            Debug.LogError("NetworkRoomPlayerTicTacToe not found on local player.");
        }

        _networkRoomManager = NetworkRoomManagerTicTacToe.Instance;
        if (_networkRoomManager == null)
        {
            Debug.LogError("NetworkRoomManagerTicTacToe instance not found.");
        }

        UpdateLobbyUI();

        // if im not the host, disable the start button
        if (!isServer)
        {
            _startButton.gameObject.SetActive(false);
        }
    }

    [Command(requiresAuthority = false)]
    public void UpdateLobbyUI()
    {
        RpcUpdateLobbyUI();
    }

    [ClientRpc]
    public void RpcUpdateLobbyUI()
    {
        StartCoroutine(UpdateLobbyUICoroutine());
    }

    private IEnumerator UpdateLobbyUICoroutine()
    {
        yield return new WaitUntil(() => _networkRoomManager != null && _networkRoomManager.roomSlots.Count > 0);
        int index = 0;
        foreach (NetworkRoomPlayerTicTacToe player in _networkRoomManager.roomSlots)
        {
            if (index < _playerNames.Count && index < _playerReadyStates.Count)
            {
                _playerNames[index].text = player.PlayerName;
                _playerReadyStates[index].text = player.readyToBegin ? "READY" : "";
            }
            index++;
        }
    }

    public void OnReadyButtonClicked()
    {
        StartCoroutine(OnReadyButtonClickedCoroutine());
        CmdMakeStartButtonInteractable();
    }

    public IEnumerator OnReadyButtonClickedCoroutine()
    {
        if (_networkRoomPlayer != null)
        {
            _networkRoomPlayer.CmdChangeReadyState(!_networkRoomPlayer.readyToBegin);
            yield return new WaitForSeconds(0.1f);
            UpdateLobbyUI();
        }
        else
        {
            Debug.LogError("NetworkRoomPlayerTicTacToe is not assigned.");
        }
    }

    [Command(requiresAuthority=false)]
    public void CmdMakeStartButtonInteractable()
    {
        bool isInteractable = _networkRoomManager.roomSlots.Count > 1 && _networkRoomManager.allPlayersReady;
        RpcMakeStartButtonInteractable(isInteractable);
    }

    [ClientRpc]
    public void RpcMakeStartButtonInteractable(bool isInteractable)
    {
        if (_startButton != null)
        {
            _startButton.interactable = isInteractable;
        }
        else
        {
            Debug.LogError("Start Button is not assigned in the inspector.");
        }
    }

    // go to the game scene
    public void StartGame()
    {
        if (_networkRoomManager != null && _networkRoomManager.allPlayersReady)
        {
            _networkRoomManager.ServerChangeScene(_networkRoomManager.GameplayScene);
        }
        else
        {
            Debug.LogWarning("Cannot start game: not all players are ready or room manager is not set.");
        }
    }

    // leave the room
    public void LeaveRoom()
    {
        if (_networkRoomPlayer != null)
        {
            if (isServer)
            {
                _networkRoomManager.StopHost();
            }
            else
            {
                _networkRoomManager.StopClient();
            }
        }
        else
        {
            Debug.LogError("NetworkRoomPlayerTicTacToe is not assigned.");
        }
    }
}
