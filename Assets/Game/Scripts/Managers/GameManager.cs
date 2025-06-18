using Mirror;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class GameManager : NetworkBehaviour
{
    [SyncVar] public int RoundNumber = 1;
    [SyncVar] public int ScorePlayer1 = 0;
    [SyncVar] public int ScorePlayer2 = 0;
    [SyncVar] public bool IsPlayer1Turn = true;

    [SyncVar] private string[] board = new string[9];

    private int _maxRounds = 3;

    [SerializeField] private Button[] Cells;

    [SerializeField] private GameObject _announcement;

    private void Start()
    {
        AssignButtonListeners();

        if (isServer)
        {
            ResetBoardServer();
        }
    }

    private void AssignButtonListeners()
    {
        for (int i = 0; i < Cells.Length; i++)
        {
            int index = i;
            Cells[i].onClick.AddListener(() => OnCellClick(index));
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
            if ((IsPlayer1Turn && player.PlayerId != 0) || (!IsPlayer1Turn && player.PlayerId != 1))
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

        string mark = IsPlayer1Turn ? "X" : "O";
        board[index] = mark;

        RpcUpdateCell(index, mark);

        if (CheckWin(mark))
        {
            if (IsPlayer1Turn)
            {
                ScorePlayer1++;
            }
            else
            {
                ScorePlayer2++;
            }

            if (RoundNumber >= _maxRounds)
            {
                string text = $"Game over, it's a tie!\nPlayer 1: {ScorePlayer1}, Player 2: {ScorePlayer2}";
                if (ScorePlayer1 > ScorePlayer2)
                {
                    text = $"Game over, player 1 wins!\nPlayer 1: {ScorePlayer1}, Player 2: {ScorePlayer2}";
                }
                else if (ScorePlayer2 > ScorePlayer1)
                {
                    text = $"Game over, player 2 wins!\nPlayer 1: {ScorePlayer1}, Player 2: {ScorePlayer2}";
                }

                ShowAnnouncement(text, 5f);

                RoundNumber = 1;
                ScorePlayer1 = 0;
                ScorePlayer2 = 0;
                ResetBoardServer();
                RpcClearBoardUI();
            }
            else
            {
                RoundNumber++;
                ResetBoardServer();
                RpcClearBoardUI();

                string message = IsPlayer1Turn ? "Player 1 wins this round!" : "Player 2 wins this round!";
                ShowAnnouncement(message, 3f);
            }
        }
        else if (CheckForTie())
        {
            RoundNumber++;
            ResetBoardServer();
            RpcClearBoardUI();

            ShowAnnouncement("It's a tie! Starting next round...", 3f);
        }
        else
        {
            IsPlayer1Turn = !IsPlayer1Turn;
        }
    }

    [ClientRpc]
    private void ShowAnnouncement(string message, float duration)
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
        if (index < 0 || index >= Cells.Length) { return; }
        TMP_Text text = Cells[index].GetComponentInChildren<TMP_Text>();
        text.text = mark;
    }

    [ClientRpc]
    private void RpcClearBoardUI()
    {
        foreach (Button cell in Cells)
        {
            cell.GetComponentInChildren<TMP_Text>().text = "";
        }
    }

    [Server]
    private void ResetBoardServer()
    {
        for (int i = 0; i < 9; i++)
        {
            board[i] = "";
        }
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
}
