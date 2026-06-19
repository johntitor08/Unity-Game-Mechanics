using TMPro;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class Message : MonoBehaviour
{
    public TMP_Text messageText;

    private void Start()
    {
        GetComponent<RectTransform>().SetAsFirstSibling();

    }

    public void SetMessageText(string text)
    {
        messageText.text = text;

    }

}