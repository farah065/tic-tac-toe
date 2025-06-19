using Mirror;
using UnityEngine;
using TMPro;

public class NetworkPlayerBehaviour : NetworkBehaviour
{
    [SyncVar(hook = nameof(PlayerNameChanged))]
    public string PlayerName;
    [SyncVar(hook = nameof(PlayerScoreChanged))]
    public int PlayerScore;
    [SyncVar] public int PlayerId;

    [SerializeField] private TMP_Text _playerNameText;
    [SerializeField] private TMP_Text _playerScoreText;

    public void PlayerNameChanged(string oldPlayerName, string newPlayerName)
    {
        _playerNameText.text = newPlayerName;
    }

    public void PlayerScoreChanged(int oldPlayerScore, int newPlayerScore)
    {
        _playerScoreText.text = "Score: " + newPlayerScore;
    }

    private void OnEnable()
    {
        Canvas canvas = gameObject.GetComponentInChildren<Canvas>();
        if (canvas != null)
        {
            Camera camera = FindFirstObjectByType<Camera>();
            if (camera != null)
            {
                canvas.worldCamera = camera;
            }
            else
            {
                Debug.LogError("Camera not found in the scene.");
            }
        }
        else
        {
            Debug.LogError("Canvas not found in the children of NetworkPlayerBehaviour.");
        }
    }
}
