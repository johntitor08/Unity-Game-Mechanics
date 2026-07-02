using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ReadingPanel : MonoBehaviour
{
    public static ReadingPanel Instance;
    private int currentPage = 1;
    private int pageCount = 1;

    [Header("UI")]
    public GameObject panel;
    public TextMeshProUGUI title;
    public TextMeshProUGUI body;
    public Image icon;
    public Button closeButton;

    [Header("Pagination")]
    public Button prevButton;
    public Button nextButton;
    public TextMeshProUGUI pageLabel;

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
        if (prevButton != null)
        {
            prevButton.onClick.RemoveAllListeners();
            prevButton.onClick.AddListener(PrevPage);
        }

        if (nextButton != null)
        {
            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(NextPage);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Close);
        }

        if (panel != null)
            panel.SetActive(false);
    }

    public void Show(string itemName, string content, Sprite itemIcon = null)
    {
        if (title != null)
            title.text = itemName;

        if (icon != null)
        {
            icon.sprite = itemIcon;
            icon.enabled = itemIcon != null;
        }

        if (body != null)
        {
            body.overflowMode = TextOverflowModes.Page;
            body.text = content ?? "";
        }

        UIPanelAnimator.Show(panel);
        StartCoroutine(InitPages());
    }

    IEnumerator InitPages()
    {
        yield return null;
        RecomputePages();
        GoToPage(1);
    }

    void RecomputePages()
    {
        if (body == null)
        {
            pageCount = 1;
            return;
        }

        body.ForceMeshUpdate();
        pageCount = Mathf.Max(1, body.textInfo.pageCount);
    }

    public void NextPage() => GoToPage(currentPage + 1);

    public void PrevPage() => GoToPage(currentPage - 1);

    void GoToPage(int page)
    {
        if (body == null)
            return;

        if (pageCount <= 1)
            RecomputePages();

        currentPage = Mathf.Clamp(page, 1, pageCount);
        body.pageToDisplay = currentPage;
        body.ForceMeshUpdate();
        UpdateNav();
    }

    void UpdateNav()
    {
        if (pageLabel != null)
            pageLabel.text = $"{currentPage} / {pageCount}";

        if (prevButton != null)
            prevButton.interactable = currentPage > 1;

        if (nextButton != null)
            nextButton.interactable = currentPage < pageCount;
    }

    public void Close() => UIPanelAnimator.Hide(panel);
}
