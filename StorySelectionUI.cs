using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorySelectionUI : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_15 = new(0.15f);
    public static StorySelectionUI Instance { get; private set; }
    private string _pendingOriginID = "";
    private Coroutine _typingCoroutine;
    public Button originButtonTemplate;
    public Transform originButtonContainer;
    public float buttonSpacing = 70f;

    [Header("Detail Panel")]
    public GameObject detailPanel;
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailText;
    public Button continueButton;

    [Header("Main Story Panel")]
    public GameObject mainStoryPanel;

    [Header("Legacy")]
    public Button originAButton;
    public TextMeshProUGUI mainStoryText;
    public Button originBButton;
    public Button originCButton;
    public GameObject storyAPanel;
    public GameObject storyBPanel;
    public GameObject storyCPanel;
    public TextMeshProUGUI storyAText;
    public TextMeshProUGUI storyBText;
    public TextMeshProUGUI storyCText;
    public Button continueAButton;
    public Button continueBButton;
    public Button continueCButton;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void Start()
    {
        if (detailPanel != null)
            detailPanel.SetActive(false);

        if (continueButton != null)
        {
            continueButton.onClick.RemoveListener(OnContinue);
            continueButton.onClick.AddListener(OnContinue);
        }

        BuildOriginButtons();
    }

    public void ShowMainPanel()
    {
        if (mainStoryPanel != null)
        {
            if (mainStoryPanel.transform.parent != null)
                mainStoryPanel.transform.parent.gameObject.SetActive(true);

            mainStoryPanel.SetActive(true);
        }

        if (detailPanel != null)
            detailPanel.SetActive(false);

        if (mainStoryText != null)
            StartTypewriter(mainStoryText, mainStoryText.text);
    }

    void BuildOriginButtons()
    {
        if (originButtonTemplate == null || originButtonContainer == null || OriginManager.Instance == null)
            return;

        originButtonTemplate.gameObject.SetActive(false);

        for (int i = originButtonContainer.childCount - 1; i >= 0; i--)
        {
            var child = originButtonContainer.GetChild(i);

            if (child.name.StartsWith("OriginBtn_"))
                Destroy(child.gameObject);
        }

        var origins = OriginManager.Instance.allOrigins;

        if (origins == null)
            return;

        var templateRect = originButtonTemplate.transform as RectTransform;
        bool hasLayoutGroup = originButtonContainer.GetComponent<LayoutGroup>() != null;
        int shown = 0;

        foreach (var origin in origins)
        {
            if (origin == null)
                continue;

            var btn = Instantiate(originButtonTemplate, originButtonContainer);
            btn.gameObject.name = "OriginBtn_" + origin.originID;
            btn.gameObject.SetActive(true);

            if (!hasLayoutGroup && templateRect != null && btn.transform is RectTransform rect)
                rect.anchoredPosition = templateRect.anchoredPosition + new Vector2(0f, -buttonSpacing * shown);

            var label = btn.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
                label.text = origin.displayName;

            string capturedID = origin.originID;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OpenPanel(capturedID));
            shown++;
        }
    }

    void OpenPanel(string originID)
    {
        _pendingOriginID = originID;

        var data = OriginManager.Instance != null ? OriginManager.Instance.GetOrigin(originID) : null;

        if (mainStoryPanel != null)
            mainStoryPanel.SetActive(false);

        if (detailPanel != null)
            detailPanel.SetActive(true);

        if (detailTitle != null && data != null)
            detailTitle.text = data.displayName;

        string summary = data != null ? data.summary : "";

        if (string.IsNullOrEmpty(summary))
            Debug.LogWarning($"[StorySelectionUI] '{originID}' has no summary.");

        if (detailText != null)
            StartTypewriter(detailText, summary);
    }

    void StartTypewriter(TextMeshProUGUI label, string text)
    {
        if (label == null || string.IsNullOrEmpty(text))
            return;

        if (_typingCoroutine != null)
        {
            StopCoroutine(_typingCoroutine);
            _typingCoroutine = null;
        }

        if (Typewriter.Instance != null && Typewriter.Instance.IsTyping)
            Typewriter.Instance.Complete();

        label.text = "";
        _typingCoroutine = StartCoroutine(TypeAfterDelay(label, text));
    }

    IEnumerator TypeAfterDelay(TextMeshProUGUI label, string text)
    {
        yield return _waitForSeconds0_15;

        if (Typewriter.Instance != null)
            Typewriter.Instance.StartTyping(label, text);
        else
            label.text = text;

        _typingCoroutine = null;
    }

    void OnContinue()
    {
        if (Typewriter.Instance != null && Typewriter.Instance.IsTyping)
            Typewriter.Instance.Complete();

        if (string.IsNullOrEmpty(_pendingOriginID))
        {
            Debug.LogWarning("[StorySelectionUI] OnContinue: no origin selected.");
            return;
        }

        if (OriginManager.Instance == null)
        {
            Debug.LogError("[StorySelectionUI] OriginManager.Instance is null.");
            return;
        }

        OriginManager.Instance.SelectOrigin(_pendingOriginID);

        if (mainStoryPanel != null && mainStoryPanel.transform.parent != null)
            mainStoryPanel.transform.parent.gameObject.SetActive(false);

        if (SceneEvent.Instance != null)
            SceneEvent.Instance.InitializeGame();
        else
            Debug.LogWarning("[StorySelectionUI] SceneEvent.Instance is null.");
    }
}
