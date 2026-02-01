using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ConfirmationDialog : MonoBehaviour
{
    public static ConfirmationDialog Instance;
    private Action onConfirm;
    private Action onCancel;

    [Header("UI Elements")]
    public GameObject confirmationPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public Button confirmButton;
    public Button cancelButton;
    public TextMeshProUGUI confirmButtonText;
    public TextMeshProUGUI cancelButtonText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    void Start()
    {
        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirmClicked);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancelClicked);
    }

    public void Show(string title, string message, Action confirmAction, Action cancelAction = null)
    {
        if (titleText != null)
            titleText.text = title;

        if (messageText != null)
            messageText.text = message;

        onConfirm = confirmAction;
        onCancel = cancelAction;

        if (confirmationPanel != null)
            confirmationPanel.SetActive(true);
    }

    void OnConfirmClicked()
    {
        Hide();
        onConfirm?.Invoke();
    }

    void OnCancelClicked()
    {
        Hide();
        onCancel?.Invoke();
    }

    void Hide()
    {
        if (confirmationPanel != null)
            confirmationPanel.SetActive(false);
    }

    public void SetButtonText(string confirm = "Yes", string cancel = "No")
    {
        if (confirmButtonText != null)
            confirmButtonText.text = confirm;

        if (cancelButtonText != null)
            cancelButtonText.text = cancel;
    }
}
