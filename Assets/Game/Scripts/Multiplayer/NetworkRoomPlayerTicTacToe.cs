using UnityEngine;
using Mirror;

public class NetworkRoomPlayerTicTacToe : NetworkRoomPlayer
{
    // keep track of the players' name
    // ready state already exists in NetworkRoomPlayer.readyToBegin
    [SyncVar]
    public string PlayerName;

    // set PlayerName on the server
    [Command]
    public void CmdSetPlayerName(string name)
    {
        PlayerName = name;
    }

    // set PlayerName when the player joins
    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();
        CmdSetPlayerName(NameChangeManager.DisplayName);
    }
}
