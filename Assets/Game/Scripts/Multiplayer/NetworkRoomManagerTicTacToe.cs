using UnityEngine;
using Mirror;

public class NetworkRoomManagerTicTacToe : NetworkRoomManager
{
    private static NetworkRoomManagerTicTacToe instance;
    public static NetworkRoomManagerTicTacToe Instance {
        get
        {
            if (instance != null) { return instance; }
            return instance = NetworkManager.singleton as NetworkRoomManagerTicTacToe;
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        NetworkRoomPlayerTicTacToe room = roomPlayer.GetComponent<NetworkRoomPlayerTicTacToe>();
        NetworkPlayerBehaviour player = gamePlayer.GetComponent<NetworkPlayerBehaviour>();

        player.PlayerId = room.index;
        Debug.Log("Player ID assigned: " + room.index);

        base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);

        return true;
    }

    public override void OnRoomClientEnter()
    {
        base.OnRoomClientEnter();

        //NetworkRoomPlayerTicTacToe roomPlayer = NetworkClient.localPlayer.GetComponent<NetworkRoomPlayerTicTacToe>();
        //if (roomPlayer.index == 0)
        //{
        //    roomPlayer.transform.position = new Vector3(-3, 0, 0);
        //}
        //else
        //{
        //    roomPlayer.transform.position = new Vector3(3, 0, 0);
        //}
    }

    // prevent the default behavior of starting the game when all players are ready
    public override void OnRoomServerPlayersReady()
    {

    }
}
