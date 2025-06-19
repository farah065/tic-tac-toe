using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    public NetworkPlayerBehaviour Player1;
    public NetworkPlayerBehaviour Player2;

    [SerializeField] private Button[] _cells;
    [SerializeField] private GameObject _announcement;

    [SyncVar] private int _roundNumber = 1;
    [SyncVar] private bool _isPlayer1Turn = true;
    [SyncVar] private string[] board = new string[9];
    private int _maxRounds = 3;

    private void Start()
    {
        AssignButtonListeners();

        if (isServer)
        {
            ResetBoardServer();
        }

        CmdAssignPlayers();
    }

    // leave the room
    public void LeaveGame()
    {
        NetworkRoomManagerTicTacToe _networkRoomManager = NetworkRoomManagerTicTacToe.Instance;
        NetworkPlayerBehaviour _networkPlayer = NetworkClient.localPlayer.GetComponent<NetworkPlayerBehaviour>();
        if (_networkPlayer != null)
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

    [Command (requiresAuthority = false)]
    private void CmdAssignPlayers()
    {
        RpcAssignPlayers();
    }

    [ClientRpc]
    private void RpcAssignPlayers()
    {
        StartCoroutine(AssignPlayers());
    }

    private IEnumerator AssignPlayers()
    {
        yield return new WaitUntil(() => FindObjectsByType<NetworkPlayerBehaviour>(FindObjectsSortMode.InstanceID).Length >= 2);
        NetworkPlayerBehaviour[] players = FindObjectsByType<NetworkPlayerBehaviour>(FindObjectsSortMode.InstanceID);

        if (players.Length == 2)
        {
            if (players[0].PlayerId == 0)
            {
                Player1 = players[0];
                Player2 = players[1];
            }
            else
            {
                Player1 = players[1];
                Player2 = players[0];
            }
        }
        else
        {
            Debug.LogError("There should be exactly two players in the scene.");
            yield break;
        }
    }

    private void AssignButtonListeners()
    {
        for (int i = 0; i < _cells.Length; i++)
        {
            int index = i;
            _cells[i].onClick.AddListener(() => OnCellClick(index));
        }
    }

    private void OnCellClick(int index)
    {
        CmdOnCellClick(index);
    }

    [Command(requiresAuthority = false)]
    private void CmdOnCellClick(int index, NetworkConnectionToClient sender = null)
    {
        if (board[index] != "") { return; }

        NetworkPlayerBehaviour player = sender.identity.GetComponent<NetworkPlayerBehaviour>();
        if (player != null) {
            if ((_isPlayer1Turn && player.PlayerId != 0) || (!_isPlayer1Turn && player.PlayerId != 1))
            {
                Debug.Log("Player " + player.PlayerId + " tried to play out of turn.");
                return;
            }
        }
        else
        {
            Debug.LogError("NetworkPlayerBehaviour is not assigned.");
            return;
        }

        string mark = _isPlayer1Turn ? "X" : "O";
        board[index] = mark;

        RpcUpdateCell(index, mark);

        if (CheckWin(mark))
        {
            if (_isPlayer1Turn)
            {
                Player1.PlayerScore = Player1.PlayerScore + 1;
            }
            else
            {
                Player2.PlayerScore = Player2.PlayerScore + 1;
            }

            if (_roundNumber >= _maxRounds)
            {
                string text = $"Game over, it's a tie!\n{Player1.PlayerName}: {Player1.PlayerScore}, {Player2.PlayerName}: {Player2.PlayerScore}";
                if (Player1.PlayerScore > Player2.PlayerScore)
                {
                    text = $"Game over, {Player1.PlayerName} wins!\n{Player1.PlayerName}: {Player1.PlayerScore}, {Player2.PlayerName}: {Player2.PlayerScore}";
                }
                else if (Player2.PlayerScore > Player1.PlayerScore)
                {
                    text = $"Game over, {Player2.PlayerName} wins!\n{Player1.PlayerName}: {Player1.PlayerScore}, {Player2.PlayerName}: {Player2.PlayerScore}";
                }

                RpcShowAnnouncement(text, 5f);

                _roundNumber = 1;
                Player1.PlayerScore = 0;
                Player2.PlayerScore = 0;
                ResetBoardServer();
                RpcClearBoardUI();
            }
            else
            {
                _roundNumber++;
                ResetBoardServer();
                RpcClearBoardUI();

                string message = _isPlayer1Turn ? $"{Player1.PlayerName} wins this round!" : $"{Player2.PlayerName} wins this round!";
                RpcShowAnnouncement(message, 3f);
            }
        }
        else if (CheckForTie())
        {
            _roundNumber++;
            ResetBoardServer();
            RpcClearBoardUI();

            RpcShowAnnouncement("It's a tie! Starting next round...", 3f);
        }
        else
        {
            _isPlayer1Turn = !_isPlayer1Turn;
        }
    }

    [ClientRpc]
    private void RpcShowAnnouncement(string message, float duration)
    {
        _announcement.SetActive(true);
        TMP_Text announcementText = _announcement.GetComponentInChildren<TMP_Text>();
        announcementText.text = message;
        StartCoroutine(HideAnnouncementAfterDelay(duration));
    }

    private IEnumerator HideAnnouncementAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _announcement.SetActive(false);
    }

    [ClientRpc]
    private void RpcUpdateCell(int index, string mark)
    {
        if (index < 0 || index >= _cells.Length) { return; }
        TMP_Text text = _cells[index].GetComponentInChildren<TMP_Text>();
        text.text = mark;
    }

    [Server]
    private bool CheckWin(string mark)
    {
        // row win
        for (int r = 0; r < 3; r++)
        {
            int start = r * 3;
            if (board[start] == mark && board[start + 1] == mark && board[start + 2] == mark)
                return true;
        }

        // column win
        for (int c = 0; c < 3; c++)
        {
            if (board[c] == mark && board[c + 3] == mark && board[c + 6] == mark)
                return true;
        }

        // diagonal win
        if ((board[0] == mark && board[4] == mark && board[8] == mark) ||
            (board[2] == mark && board[4] == mark && board[6] == mark))
            return true;

        return false;
    }

    [Server]
    private bool CheckForTie()
    {
        foreach (string cell in board)
        {
            if (cell == "") { return false; }
        }
        return true;
    }

    [Server]
    private void ResetBoardServer()
    {
        for (int i = 0; i < 9; i++)
        {
            board[i] = "";
        }
    }

    [ClientRpc]
    private void RpcClearBoardUI()
    {
        foreach (Button cell in _cells)
        {
            cell.GetComponentInChildren<TMP_Text>().text = "";
        }
    }
}
