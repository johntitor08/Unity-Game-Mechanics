using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class QuestRewardUI : MonoBehaviour
{
    public static QuestRewardUI Instance { get; private set; }
    private Coroutine autoCloseCoroutine;

    [Header("Reward Panel")]
    public GameObject rewardPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI questNameText;
    public Transform rewardsContainer;
    public RewardItemUI rewardItemPrefab;
    public Button closeButton;

    [Header("Optional Rewards")]
    public Transform optionalRewardsContainer;
    public TextMeshProUGUI optionalRewardsLabel;

    [Header("ScrollRects")]
    public ScrollRect rewardsScrollRect;
    public ScrollRect optionalRewardsScrollRect;
    public float maxRewardsHeight = 300f;

    [Header("Animation")]
    public float displayDuration = 5f;

    [Header("Display")]
    [SerializeField] private string completionTitle = "Quest Complete!";

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ShowRewards(QuestData quest, ItemData selectedOptionalReward = null)
    {
        if (quest == null || rewardItemPrefab == null)
            return;

        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

        if (rewardPanel != null)
            rewardPanel.SetActive(true);

        if (questNameText != null)
            questNameText.text = quest.questName;

        if (titleText != null)
            titleText.text = completionTitle;

        ClearContainer(rewardsContainer);

        if (quest.experienceReward > 0)
            SpawnRewardItem(rewardsContainer, "Experience", quest.experienceReward.ToString(), null);

        if (quest.currencyRewards != null)
        {
            foreach (var reward in quest.currencyRewards)
            {
                var currencyInfo = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetCurrencyInfo(reward.type) : null;
                SpawnRewardItem(rewardsContainer, reward.type.ToString(), reward.amount.ToString(), currencyInfo?.icon);
            }
        }

        if (quest.itemRewards != null)
        {
            for (int i = 0; i < quest.itemRewards.Length; i++)
            {
                var item = quest.itemRewards[i];

                if (item == null)
                    continue;

                int qty = (quest.itemRewardQuantities != null && i < quest.itemRewardQuantities.Length) ? quest.itemRewardQuantities[i] : 1;
                SpawnRewardItem(rewardsContainer, item.itemName, $"x{qty}", item.icon);
            }
        }

        bool hasOptional = selectedOptionalReward != null || quest.optionalRewards != null && quest.optionalRewards.Length > 0;

        if (optionalRewardsLabel != null)
            optionalRewardsLabel.gameObject.SetActive(hasOptional);

        ClearContainer(optionalRewardsContainer);

        if (selectedOptionalReward != null)
        {
            SpawnRewardItem(optionalRewardsContainer, selectedOptionalReward.itemName, "x1", selectedOptionalReward.icon);
        }
        else if (quest.optionalRewards != null && optionalRewardsContainer != null)
        {
            foreach (var item in quest.optionalRewards)
                if (item != null)
                    SpawnRewardItem(optionalRewardsContainer, item.itemName, "x1", item.icon);
        }

        autoCloseCoroutine = StartCoroutine(AutoClose());
        StartCoroutine(UpdateScrollsNextFrame());
    }

    IEnumerator UpdateScrollsNextFrame()
    {
        yield return null;
        UpdateVerticalScrollRect(rewardsContainer, rewardsScrollRect, maxRewardsHeight);
        UpdateVerticalScrollRect(optionalRewardsContainer, optionalRewardsScrollRect, maxRewardsHeight);
    }

    void UpdateVerticalScrollRect(Transform contentParent, ScrollRect sr, float threshold)
    {
        if (sr == null || contentParent == null)
            return;

        float contentHeight = contentParent is RectTransform rt ? rt.rect.height : 0f;
        bool needsScroll = contentHeight > threshold;
        sr.horizontal = false;
        sr.vertical = needsScroll;
        sr.verticalScrollbar = needsScroll ? sr.verticalScrollbar : null;
        sr.verticalNormalizedPosition = 1f;
    }

    private void ClearContainer(Transform container)
    {
        if (container == null)
            return;

        foreach (Transform child in container)
            Destroy(child.gameObject);
    }

    private void SpawnRewardItem(Transform container, string label, string value, Sprite icon)
    {
        if (container == null)
            return;

        var rewardUI = Instantiate(rewardItemPrefab, container);
        rewardUI.Setup(label, value, icon);
    }

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(displayDuration);
        Close();
    }

    void Close()
    {
        if (autoCloseCoroutine != null)
        {
            StopCoroutine(autoCloseCoroutine);
            autoCloseCoroutine = null;
        }

        if (rewardPanel != null)
            rewardPanel.SetActive(false);
    }
}
