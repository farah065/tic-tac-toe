using UnityEngine;
using Mirror;
using System.Collections;
using UnityEngine.SceneManagement;

public class NetworkRoomManagerTicTacToe : NetworkRoomManager
{
    public static NetworkRoomManagerTicTacToe Instance {
        get
        {
            if (instance != null) { return instance; }
            return instance = NetworkManager.singleton as NetworkRoomManagerTicTacToe;
        }
    }
    private static NetworkRoomManagerTicTacToe instance;

    public IEnumerator SetUpRoomPlayers(NetworkConnectionToClient conn)
    {
        yield return new WaitUntil(() => conn.identity != null && conn.identity.GetComponent<NetworkRoomPlayerTicTacToe>() != null);
        NetworkRoomPlayerTicTacToe roomPlayer = conn.identity.GetComponent<NetworkRoomPlayerTicTacToe>();
        if (roomPlayer.index == 0)
        {
            roomPlayer.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(-320, -132, 0);
        }
        else
        {
            roomPlayer.transform.GetChild(0).GetChild(0).transform.localPosition = new Vector3(320, -132, 0);
        }

        LobbyUIManager lobbyUIManager = FindFirstObjectByType<LobbyUIManager>();
        if (lobbyUIManager != null)
        {
            lobbyUIManager.RpcSetWaitingTextVisibility(roomSlots.Count);
        }
        else
        {
            Debug.LogError("LobbyUIManager not found in the scene.");
        }
    }

    public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnectionToClient conn, GameObject roomPlayer, GameObject gamePlayer)
    {
        NetworkRoomPlayerTicTacToe room = roomPlayer.GetComponent<NetworkRoomPlayerTicTacToe>();
        NetworkPlayerBehaviour player = gamePlayer.GetComponent<NetworkPlayerBehaviour>();

        player.PlayerId = room.index;
        player.PlayerName = room.PlayerName;

        base.OnRoomServerSceneLoadedForPlayer(conn, roomPlayer, gamePlayer);

        return true;
    }

    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        StartCoroutine(SetUpRoomPlayers(conn));
    }

    public override void OnRoomClientExit()
    {
        // if active scene is the lobby
        if (SceneManager.GetActiveScene().name == "Lobby")
        {
            LobbyUIManager lobbyUIManager = FindFirstObjectByType<LobbyUIManager>();
            if (lobbyUIManager != null)
            {
                lobbyUIManager.RpcSetWaitingTextVisibility(roomSlots.Count);
            }
            else
            {
                Debug.LogError("LobbyUIManager not found in the scene.");
            }
        }
        base.OnRoomClientExit();
    }

    public override GameObject OnRoomServerCreateGamePlayer(NetworkConnectionToClient conn, GameObject roomPlayer)
    {
        GameObject gamePlayer = base.OnRoomServerCreateGamePlayer(conn, roomPlayer);
        roomPlayer.GetComponent<NetworkRoomPlayerTicTacToe>()?.CmdSetInactive();
        return gamePlayer;
    }

    // prevent the default behavior of starting the game when all players are ready
    public override void OnRoomServerPlayersReady()
    {

    }
}
