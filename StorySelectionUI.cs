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
    private string _summaryA;
    private string _summaryB;
    private string _summaryC;

    [Header("Origin Select Buttons")]
    public Button originAButton;
    public Button originBButton;
    public Button originCButton;

    [Header("Story Panels")]
    public GameObject mainStoryPanel;
    public GameObject storyAPanel;
    public GameObject storyBPanel;
    public GameObject storyCPanel;

    [Header("Story Texts")]
    public TextMeshProUGUI mainStoryText;
    public TextMeshProUGUI storyAText;
    public TextMeshProUGUI storyBText;
    public TextMeshProUGUI storyCText;

    [Header("Continue Buttons")]
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
        SetPanels(false, false, false, false);

        if (originAButton != null)
            originAButton.onClick.AddListener(() => OpenPanel("bound_archivist"));

        if (originBButton != null)
            originBButton.onClick.AddListener(() => OpenPanel("foreign_echo"));

        if (originCButton != null)
            originCButton.onClick.AddListener(() => OpenPanel("sinned_guardian"));

        if (continueAButton != null)
            continueAButton.onClick.AddListener(OnContinue);

        if (continueBButton != null)
            continueBButton.onClick.AddListener(OnContinue);

        if (continueCButton != null)
            continueCButton.onClick.AddListener(OnContinue);

        if (storyAText != null)
            storyAText.text = "";

        if (storyBText != null)
            storyBText.text = "";

        if (storyCText != null)
            storyCText.text = "";

        StartCoroutine(CacheAfterFrame());
    }

    IEnumerator CacheAfterFrame()
    {
        yield return null;
        CacheOriginSummaries();
    }

    public void ShowMainPanel()
    {
        mainStoryPanel.transform.parent.gameObject.SetActive(true);
        SetPanels(true, false, false, false);

        if (mainStoryText != null)
        {
            string content = mainStoryText.text;
            StartTypewriter(mainStoryText, content);
        }
    }

    void OpenPanel(string originID)
    {
        if (string.IsNullOrEmpty(_summaryA) && string.IsNullOrEmpty(_summaryB) && string.IsNullOrEmpty(_summaryC))
            CacheOriginSummaries();

        _pendingOriginID = originID;
        bool a = originID == "bound_archivist";
        bool b = originID == "foreign_echo";
        bool c = originID == "sinned_guardian";
        SetPanels(false, a, b, c);
        TextMeshProUGUI targetLabel = a ? storyAText : b ? storyBText : storyCText;
        string summary = a ? _summaryA : b ? _summaryB : _summaryC;

        if (string.IsNullOrEmpty(summary))
        {
            Debug.LogWarning($"[StorySelectionUI] '{originID}' summary boþ.");
            return;
        }

        StartTypewriter(targetLabel, summary);
    }

    void SetPanels(bool main, bool a, bool b, bool c)
    {
        if (mainStoryPanel != null)
            mainStoryPanel.SetActive(main);

        if (storyAPanel != null)
            storyAPanel.SetActive(a);

        if (storyBPanel != null)
            storyBPanel.SetActive(b);

        if (storyCPanel != null)
            storyCPanel.SetActive(c);
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
            Typewriter.Instance.Complete(label);

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
        {
            Typewriter.Instance.Complete(GetActivePanelText());
        }

        if (string.IsNullOrEmpty(_pendingOriginID))
        {
            Debug.LogWarning("[StorySelectionUI] OnContinue: origin seçilmedi.");
            return;
        }

        if (OriginManager.Instance == null)
        {
            Debug.LogError("[StorySelectionUI] OriginManager.Instance null.");
            return;
        }

        OriginManager.Instance.SelectOrigin(_pendingOriginID);
        mainStoryPanel.transform.parent.gameObject.SetActive(false);

        if (SceneEvent.Instance != null)
            SceneEvent.Instance.InitializeGame();
        else
            Debug.LogWarning("[StorySelectionUI] SceneEvent.Instance null.");
    }

    void CacheOriginSummaries()
    {
        if (OriginManager.Instance == null)
        {
            Debug.LogWarning("[StorySelectionUI] OriginManager null.");
            return;
        }

        _summaryA = GetSummary("bound_archivist");
        _summaryB = GetSummary("foreign_echo");
        _summaryC = GetSummary("sinned_guardian");
    }

    string GetSummary(string originID)
    {
        var data = OriginManager.Instance.GetOrigin(originID);
        return (data != null && !string.IsNullOrEmpty(data.summary)) ? data.summary : "";
    }

    TextMeshProUGUI GetActivePanelText()
    {
        return _pendingOriginID switch
        {
            "bound_archivist" => storyAText,
            "foreign_echo" => storyBText,
            "sinned_guardian" => storyCText,
            _ => mainStoryText
        };
    }
}
