using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorySelectionUI : MonoBehaviour
{
    public static StorySelectionUI Instance { get; private set; }
    private string _pendingOriginID = "";

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
        SetPanels(true, false, false, false);

        if (originAButton != null)
            originAButton.onClick.AddListener(() => OpenPanel("archivist"));

        if (originBButton != null)
            originBButton.onClick.AddListener(() => OpenPanel("echo"));

        if (originCButton != null)
            originCButton.onClick.AddListener(() => OpenPanel("guardian"));

        if (continueAButton != null)
            continueAButton.onClick.AddListener(OnContinue);

        if (continueBButton != null)
            continueBButton.onClick.AddListener(OnContinue);

        if (continueCButton != null)
            continueCButton.onClick.AddListener(OnContinue);

        FillTextsFromOriginData();
    }

    void OpenPanel(string originID)
    {
        _pendingOriginID = originID;
        bool a = originID == "archivist";
        bool b = originID == "echo";
        bool c = originID == "guardian";
        SetPanels(!a && !b && !c, a, b, c);
    }

    void SetPanels(bool a, bool b, bool c, bool d)
    {
        if (mainStoryPanel != null)
            mainStoryPanel.SetActive(a);

        if (storyAPanel != null)
            storyAPanel.SetActive(b);

        if (storyBPanel != null)
            storyBPanel.SetActive(c);

        if (storyCPanel != null)
            storyCPanel.SetActive(d);
    }

    void OnContinue()
    {
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
        mainStoryPanel.transform.parent.gameObject.SetActive(false);

        if (SceneEvent.Instance != null)
            SceneEvent.Instance.InitializeGame();
    }

    void FillTextsFromOriginData()
    {
        if (OriginManager.Instance == null)
            return;

        TryFillText(storyAText, "archivist");
        TryFillText(storyBText, "echo");
        TryFillText(storyCText, "guardian");
    }

    void TryFillText(TextMeshProUGUI label, string originID)
    {
        if (label == null)
            return;

        var data = OriginManager.Instance.GetOrigin(originID);

        if (data != null && !string.IsNullOrEmpty(data.summary))
            label.text = data.summary;
    }
}
