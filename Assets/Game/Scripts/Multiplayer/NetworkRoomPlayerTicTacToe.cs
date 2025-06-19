using UnityEngine;
using Mirror;
using TMPro;

public class NetworkRoomPlayerTicTacToe : NetworkRoomPlayer
{
    // keep track of the player's name
    // ready state already exists in NetworkRoomPlayer.readyToBegin
    [SyncVar(hook = nameof(PlayerNameChanged))]
    public string PlayerName;
    [SerializeField] public TMP_Text playerNameText;
    [SerializeField] private TMP_Text playerReadyText;

    // set PlayerName on the server
    [Command]
    public void CmdSetPlayerName(string name)
    {
        RpcSetPlayerName(name);
    }

    [ClientRpc]
    public void RpcSetPlayerName(string name)
    {
        PlayerName = name;
        playerNameText.text = name;
    }

    // set PlayerName when the player joins
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdSetPlayerName(NameChangeManager.DisplayName);
    }

    public void PlayerNameChanged(string oldPlayerName, string newPlayerName)
    {
        playerNameText.text = newPlayerName;
    }

    public override void ReadyStateChanged(bool oldReadyState, bool newReadyState)
    {
        playerReadyText.text = newReadyState ? "Ready" : "";
    }

    public override void OnClientExitRoom()
    {
        // update waiting text
        base.OnClientExitRoom();
    }

    [Command(requiresAuthority = false)]
    public void CmdSetInactive()
    {
        RpcSetInactive();
    }

    [ClientRpc]
    public void RpcSetInactive()
    {
        Debug.Log("Setting player inactive: " + PlayerName);
        gameObject.SetActive(false);
    }
}
