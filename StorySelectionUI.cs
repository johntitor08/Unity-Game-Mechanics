using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class StorySelectionUI : MonoBehaviour
{
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

    private string _pendingOriginID = "";

    void Start()
    {
        SetPanels(true, false, false, false);

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

        FillTextsFromOriginData();
    }

    void OpenPanel(string originID)
    {
        _pendingOriginID = originID;
        bool a = originID == "bound_archivist";
        bool b = originID == "foreign_echo";
        bool c = originID == "sinned_guardian";
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
            Debug.LogError("[StorySelectionUI] OriginManager.Instance is null — make sure it exists in the scene.");
            return;
        }

        OriginManager.Instance.SelectOrigin(_pendingOriginID);
        mainStoryPanel.transform.parent.gameObject.SetActive(false);
    }

    void FillTextsFromOriginData()
    {
        if (OriginManager.Instance == null)
            return;

        TryFillText(storyAText, "bound_archivist");
        TryFillText(storyBText, "foreign_echo");
        TryFillText(storyCText, "sinned_guardian");
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
