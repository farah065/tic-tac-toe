using TMPro;
using UnityEngine;

public class UnderlineButtonText : MonoBehaviour
{
    // underline button text on hover
    public void OnEnterUnderlineButtonText()
    {
        TMP_Text buttonText = gameObject.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.fontStyle = FontStyles.Underline;
        }
    }

    // remove underline from button text on exit
    public void OnExitUnunderlineButtonText()
    {
        TMP_Text buttonText = gameObject.GetComponentInChildren<TMP_Text>();
        if (buttonText != null)
        {
            buttonText.fontStyle = FontStyles.Normal;
        }
    }
}
