using UnityEngine;
using Mirror;
using TMPro;

public class NetworkRoomPlayerTicTacToe : NetworkRoomPlayer
{
    // keep track of the player's name
    // ready state already exists in NetworkRoomPlayer.readyToBegin
    [SyncVar(hook = nameof(PlayerNameChanged))]
    public string PlayerName;

    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _playerReadyText;

    // set PlayerName on the server
    [Command]
    public void CmdSetPlayerName(string name)
    {
        RpcSetPlayerName(name);
    }

    [Command(requiresAuthority = false)]
    public void CmdSetInactive()
    {
        RpcSetInactive();
    }

    // set PlayerName when the player joins
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdSetPlayerName(NameChangeManager.DisplayName);
    }

    public void PlayerNameChanged(string oldPlayerName, string newPlayerName)
    {
        _playerNameText.text = newPlayerName;
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        _playerReadyText.text = newReadyState ? "Ready" : "";
    }

    public override void OnClientExitRoom()
    {
        // update waiting text
        base.OnClientExitRoom();
    }

    [ClientRpc]
    private void RpcSetPlayerName(string name)
    {
        PlayerName = name;
        _playerNameText.text = name;
    }

    [ClientRpc]
    private void RpcSetInactive()
    {
        gameObject.SetActive(false);
    }
}
