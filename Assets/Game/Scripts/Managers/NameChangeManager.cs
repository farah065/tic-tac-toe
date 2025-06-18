using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class NameChangeManager : MonoBehaviour
{
    public static string DisplayName { get; private set; }
    private const string _playerPrefsNameKey = "PlayerName";
    [SerializeField] private GameObject _nameEditPanel;
    [SerializeField] private TMP_Text _nameText;
    [SerializeField] private GameObject _nameEditButton;

    private void Awake()
    {
        if (_nameEditPanel == null)
        {
            Debug.LogError("Name Edit Panel is not assigned in the inspector.");
        }
        if (_nameText == null)
        {
            Debug.LogError("Name Text is not assigned in the inspector.");
        }
        if (_nameEditButton == null)
        {
            Debug.LogError("Name Edit Button is not assigned in the inspector.");
        }
    }

    private void Start()
    {
        _nameEditPanel.SetActive(false);

        // ensure the player name is set in PlayerPrefs
        if (!PlayerPrefs.HasKey(_playerPrefsNameKey))
        {
            PlayerPrefs.SetString(_playerPrefsNameKey, "Player");
        }

        // set the name in the UI and attributes
        DisplayName = PlayerPrefs.GetString(_playerPrefsNameKey);
        _nameText.text = PlayerPrefs.GetString(_playerPrefsNameKey);

        // set the default text for the input field
        TMP_InputField nameInputField = _nameEditPanel.GetComponentInChildren<TMP_InputField>();
        if (nameInputField != null)
        {
            nameInputField.text = PlayerPrefs.GetString(_playerPrefsNameKey);
        }
    }

    // show the name edit UI
    public void OpenNameEditPanel()
    {
        _nameEditButton.SetActive(false);
        _nameText.gameObject.SetActive(false);
        _nameEditPanel.SetActive(true);
    }

    // called on pressing save in the name edit panel
    public void ChangeName()
    {
        TMP_InputField nameInputField = _nameEditPanel.GetComponentInChildren<TMP_InputField>();
        if (nameInputField != null)
        {
            string newName = nameInputField.text;
            if (!string.IsNullOrEmpty(newName))
            {
                // change the display name in the UI
                _nameText.text = newName;
                // change the name in the player attributes (this is where we fetch it from later)
                DisplayName = newName;
                // save the name in PlayerPrefs for next session
                PlayerPrefs.SetString(_playerPrefsNameKey, newName);
            }
        }

        // after changing the name, close the edit panel
        CloseNameEditPanel();
    }

    // hide the name edit UI and go back to the normal name display
    private void CloseNameEditPanel()
    {
        _nameEditButton.SetActive(true);
        _nameText.gameObject.SetActive(true);
        _nameEditPanel.SetActive(false);
    }
}
