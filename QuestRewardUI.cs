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

    [Header("Animation")]
    public float displayDuration = 5f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
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

    public void ShowRewards(QuestData quest)
    {
        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

        rewardPanel.SetActive(true);

        if (questNameText != null)
            questNameText.text = quest.questName;

        if (titleText != null)
            titleText.text = "Quest Complete!";

        foreach (Transform child in rewardsContainer)
            Destroy(child.gameObject);

        if (quest.experienceReward > 0)
        {
            var rewardUI = Instantiate(rewardItemPrefab, rewardsContainer);
            rewardUI.Setup("Experience", quest.experienceReward.ToString(), null);
        }

        if (quest.currencyRewards != null)
        {
            foreach (var reward in quest.currencyRewards)
            {
                var rewardUI = Instantiate(rewardItemPrefab, rewardsContainer);
                var currencyInfo = CurrencyManager.Instance != null ? CurrencyManager.Instance.GetCurrencyInfo(reward.type) : null;
                rewardUI.Setup(reward.type.ToString(), reward.amount.ToString(), currencyInfo?.icon);
            }
        }

        if (quest.itemRewards != null)
        {
            for (int i = 0; i < quest.itemRewards.Length; i++)
            {
                int qty = (quest.itemRewardQuantities != null && i < quest.itemRewardQuantities.Length) ? quest.itemRewardQuantities[i] : 1;
                var rewardUI = Instantiate(rewardItemPrefab, rewardsContainer);
                rewardUI.Setup(quest.itemRewards[i].itemName, $"x{qty}", quest.itemRewards[i].icon);
            }
        }

        bool hasOptional = quest.optionalRewards != null && quest.optionalRewards.Length > 0;

        if (optionalRewardsLabel != null)
            optionalRewardsLabel.gameObject.SetActive(hasOptional);

        if (optionalRewardsContainer != null)
        {
            foreach (Transform child in optionalRewardsContainer)
                Destroy(child.gameObject);

            if (hasOptional)
            {
                foreach (var item in quest.optionalRewards)
                {
                    var rewardUI = Instantiate(rewardItemPrefab, optionalRewardsContainer);
                    rewardUI.Setup(item.itemName, "x1", item.icon);
                }
            }
        }

        autoCloseCoroutine = StartCoroutine(AutoClose());
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

        rewardPanel.SetActive(false);
    }
}
