using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class PanelHeader : MonoBehaviour
{
    [Header("Title")]
    public string titleEn = "Panel";
    public string titleTr = "Panel";

    [Header("Refs")]
    public TMP_Text titleLabel;
    public Button closeButton;

    [Header("Close target")]
    public GameObject closeTarget;

    public UnityEvent onClose = new();

    void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
        RefreshTitle();
    }

    void OnDisable()
    {
        LanguageManager.OnLanguageChanged -= OnLanguageChanged;
    }

    void OnLanguageChanged(GameLanguage lang) => RefreshTitle();

    void Start()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveListener(Close);
            closeButton.onClick.AddListener(Close);
        }
    }

    public void RefreshTitle()
    {
        if (titleLabel != null)
            titleLabel.text = Loc.T(titleEn, titleTr);
    }

    public void Close()
    {
        if (closeTarget != null)
            UIPanelAnimator.Hide(closeTarget);

        onClose.Invoke();
    }
}
