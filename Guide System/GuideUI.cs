using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuideUI : MonoBehaviour
{
    public static GuideUI Instance;
    private GuideCategory _current = GuideCategory.Book;
    private readonly List<GameObject> _spawned = new();
    private readonly Dictionary<GuideEntry, UIButtonStyle> _entryStyles = new();
    private UITheme Theme => theme != null ? theme : booksTab != null && booksTab.TryGetComponent<UIButtonStyle>(out var s) ? s.theme : null;

    [Header("Window")]
    public GameObject panel;
    public Button closeButton;

    [Header("Tabs")]
    public Button booksTab;
    public Button charactersTab;
    public Button loreTab;

    [Header("List")]
    public Transform listContent;
    public TMP_FontAsset listFont;

    [Header("Detail")]
    public TextMeshProUGUI detailTitle;
    public TextMeshProUGUI detailBody;
    public Image detailIcon;

    [Header("Style")]
    public UITheme theme;
    public Color goldColor = new(0.784f, 0.659f, 0.431f);

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
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (booksTab != null)
        {
            booksTab.onClick.RemoveAllListeners();
            booksTab.onClick.AddListener(() => Show(GuideCategory.Book));
        }

        if (charactersTab != null)
        {
            charactersTab.onClick.RemoveAllListeners();
            charactersTab.onClick.AddListener(() => Show(GuideCategory.Character));
        }

        if (loreTab != null)
        {
            loreTab.onClick.RemoveAllListeners();
            loreTab.onClick.AddListener(() => Show(GuideCategory.Lore));
        }

        if (panel != null)
            panel.SetActive(false);
    }

    public void Open()
    {
        if (panel != null && panel.activeSelf)
        {
            Close();
            return;
        }

        if (SceneEvent.Instance != null)
            SceneEvent.Instance.HideAllPanels();

        UIPanelAnimator.Show(panel);
        Show(GuideCategory.Book);
    }

    public void Close() => UIPanelAnimator.Hide(panel);

    public void Show(GuideCategory category)
    {
        _current = category;
        MarkTab(category);
        ClearList();
        ClearDetail();
        var all = GuideManager.Instance != null ? GuideManager.Instance.GetEntries(category) : new List<GuideEntry>();
        var list = new List<GuideEntry>();

        foreach (var e in all)
            if (e.bookItem == null || HasItem(e.bookItem))
                list.Add(e);

        foreach (var e in list)
            SpawnListButton(e);

        if (list.Count > 0)
            ShowDetail(list[0]);
    }

    static bool HasItem(ItemData item) => InventoryManager.Instance != null && InventoryManager.Instance.GetQuantity(item) > 0;

    void SpawnListButton(GuideEntry e)
    {
        var go = new GameObject(e.title, typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        go.transform.SetParent(listContent, false);
        go.GetComponent<LayoutElement>().minHeight = 56;
        var lblGo = new GameObject("Label", typeof(RectTransform));
        lblGo.transform.SetParent(go.transform, false);
        var lbl = lblGo.AddComponent<TextMeshProUGUI>();

        if (listFont != null)
            lbl.font = listFont;

        lbl.text = e.DisplayTitle;
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.margin = new Vector4(18, 0, 8, 0);
        var lr = lbl.rectTransform;
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        var style = go.AddComponent<UIButtonStyle>();
        style.theme = Theme;
        style.kind = UIButtonStyle.Kind.Compact;
        style.labelFontSize = 26;
        style.resizeToMetrics = false;
        style.Apply();
        _entryStyles[e] = style;
        go.GetComponent<Button>().onClick.AddListener(() => Select(e));
        _spawned.Add(go);
    }

    void MarkTab(GuideCategory category)
    {
        SetPrimary(booksTab, category == GuideCategory.Book);
        SetPrimary(charactersTab, category == GuideCategory.Character);
        SetPrimary(loreTab, category == GuideCategory.Lore);
    }

    void MarkEntry(GuideEntry selected)
    {
        foreach (var kv in _entryStyles)
            if (kv.Value != null && kv.Value.primary != (kv.Key == selected))
            {
                kv.Value.primary = kv.Key == selected;
                kv.Value.Apply();
            }
    }

    static void SetPrimary(Button button, bool on)
    {
        if (button != null && button.TryGetComponent<UIButtonStyle>(out var style) && style.primary != on)
        {
            style.primary = on;
            style.Apply();
        }
    }

    void Select(GuideEntry e)
    {
        if (e.category == GuideCategory.Book && e.bookItem != null && ReadingPanel.Instance != null)
        {
            Close();
            ReadingPanel.Instance.Show(e.bookItem.DisplayName, e.bookItem.DisplayReadText, e.bookItem.icon);
            return;
        }

        ShowDetail(e);
    }

    void ShowDetail(GuideEntry e)
    {
        MarkEntry(e);

        if (detailTitle != null)
        {
            detailTitle.text = e.DisplayTitle;
            detailTitle.color = goldColor;
        }

        if (detailBody != null)
        {
            string body = e.DisplayBody;

            if (e.category == GuideCategory.Book)
                body += Loc.T("\n\n(Click to open and read.)", "\n\n(Açıp okumak için tıkla.)");
            else if (e.category == GuideCategory.Character && AffinityManager.Instance != null)
                body += $"\n\n{Loc.T("Affinity", "Yakınlık")}: {AffinityManager.Instance.HeartBar(e.title)}  {AffinityManager.Instance.Get(e.title)}/{AffinityManager.Instance.maxAffinity}  ({AffinityManager.Instance.Tier(e.title)})";

            detailBody.text = body;
        }

        if (detailIcon != null)
        {
            Sprite sprite = e.bookItem != null && e.bookItem.icon != null ? e.bookItem.icon : e.icon;
            detailIcon.sprite = sprite;
            detailIcon.enabled = sprite != null;
        }
    }

    void ClearList()
    {
        foreach (var g in _spawned)
            if (g != null)
                Destroy(g);

        _spawned.Clear();
        _entryStyles.Clear();
    }

    void ClearDetail()
    {
        if (detailTitle != null)
            detailTitle.text = "";

        if (detailBody != null)
            detailBody.text = "";

        if (detailIcon != null)
            detailIcon.enabled = false;
    }
}
