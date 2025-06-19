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
    [SerializeField] private List<TMP_Text> _waitingText;
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

        // if im not the host, disable the start button
        if (!isServer)
        {
            _startButton.gameObject.SetActive(false);
        }
    }

    //[Command(requiresAuthority=false)]
    //public void CmdSetWaitingTextVisibility()
    //{
    //    int fullSlots = _networkRoomManager.roomSlots.Count;
    //    RpcSetWaitingTextVisibility(fullSlots);
    //}

    //[ClientRpc]
    //public void RpcSetWaitingTextVisibility(int fullSlots)
    //{
        
    //    for (int i = 0; i < _waitingText.Count; i++)
    //    {
    //        if (i + 1 > fullSlots)
    //        {
    //            _waitingText[i].gameObject.SetActive(true);
    //        }
    //        else
    //        {
    //            _waitingText[i].gameObject.SetActive(false);
    //        }
    //    }
    //}

    public void OnReadyButtonClicked()
    {
        _networkRoomPlayer.CmdChangeReadyState(!_networkRoomPlayer.readyToBegin);
        CmdMakeStartButtonInteractable();
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
