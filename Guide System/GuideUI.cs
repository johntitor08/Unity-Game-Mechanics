using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GuideUI : MonoBehaviour
{
    public static GuideUI Instance;
    GuideCategory _current = GuideCategory.Book;
    readonly List<GameObject> _spawned = new();

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
    public Color buttonColor = new(0.478f, 0.333f, 0.188f);
    public Color labelColor = new(0.937f, 0.886f, 0.761f);
    public Color goldColor = new(0.878f, 0.733f, 0.353f);

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
        UIPanelAnimator.Show(panel);
        Show(GuideCategory.Book);
    }

    public void Close() => UIPanelAnimator.Hide(panel);

    public void Show(GuideCategory category)
    {
        _current = category;
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
        go.GetComponent<Image>().color = buttonColor;
        go.GetComponent<LayoutElement>().minHeight = 56;
        var lblGo = new GameObject("Label", typeof(RectTransform));
        lblGo.transform.SetParent(go.transform, false);
        var lbl = lblGo.AddComponent<TextMeshProUGUI>();

        if (listFont != null)
            lbl.font = listFont;

        lbl.text = e.title;
        lbl.color = labelColor;
        lbl.fontSize = 26;
        lbl.alignment = TextAlignmentOptions.Left;
        lbl.margin = new Vector4(18, 0, 8, 0);
        var lr = lbl.rectTransform;
        lr.anchorMin = Vector2.zero; lr.anchorMax = Vector2.one; lr.offsetMin = Vector2.zero; lr.offsetMax = Vector2.zero;
        go.GetComponent<Button>().onClick.AddListener(() => Select(e));
        _spawned.Add(go);
    }

    void Select(GuideEntry e)
    {
        if (e.category == GuideCategory.Book && e.bookItem != null && ReadingPanel.Instance != null)
        {
            Close();
            ReadingPanel.Instance.Show(e.bookItem.itemName, e.bookItem.readText, e.bookItem.icon);
            return;
        }

        ShowDetail(e);
    }

    void ShowDetail(GuideEntry e)
    {
        if (detailTitle != null)
        {
            detailTitle.text = e.title;
            detailTitle.color = goldColor;
        }

        if (detailBody != null)
        {
            string body = e.body;

            if (e.category == GuideCategory.Book)
                body += "\n\n(Click to open and read.)";
            else if (e.category == GuideCategory.Character && AffinityManager.Instance != null)
                body += $"\n\nAffinity: {AffinityManager.Instance.HeartBar(e.title)}  {AffinityManager.Instance.Get(e.title)}/{AffinityManager.Instance.maxAffinity}  ({AffinityManager.Instance.Tier(e.title)})";

            detailBody.text = body;
        }

        if (detailIcon != null)
        {
            detailIcon.sprite = e.icon;
            detailIcon.enabled = e.icon != null;
        }
    }

    void ClearList()
    {
        foreach (var g in _spawned)
            if (g != null)
                Destroy(g);

        _spawned.Clear();
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
