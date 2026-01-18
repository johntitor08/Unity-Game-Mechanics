using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class QuestRewardUI : MonoBehaviour
{
    public static QuestRewardUI Instance;

    [Header("Reward Panel")]
    public GameObject rewardPanel;
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI questNameText;
    public Transform rewardsContainer;
    public RewardItemUI rewardItemPrefab;
    public Button closeButton;

    [Header("Animation")]
    public float displayDuration = 5f;

    void Awake()
    {
        Instance = this;
        rewardPanel.SetActive(false);
    }

    void Start()
    {
        if (closeButton != null)
            closeButton.onClick.AddListener(Close);
    }

    public void ShowRewards(QuestData quest)
    {
        rewardPanel.SetActive(true);

        if (questNameText != null)
            questNameText.text = quest.questName;

        if (titleText != null)
            titleText.text = "Quest Complete!";

        // Clear old rewards
        foreach (Transform child in rewardsContainer)
            Destroy(child.gameObject);

        // Show experience
        if (quest.experienceReward > 0)
        {
            var rewardUI = Instantiate(rewardItemPrefab, rewardsContainer);
            rewardUI.Setup("Experience", quest.experienceReward.ToString(), null);
        }

        // Show currency
        if (quest.currencyRewards != null)
        {
            foreach (var reward in quest.currencyRewards)
            {
                var rewardUI = Instantiate(rewardItemPrefab, rewardsContainer);
                var currencyInfo = CurrencyManager.Instance.GetCurrencyInfo(reward.type);
                rewardUI.Setup(reward.type.ToString(), reward.amount.ToString(), currencyInfo?.icon);
            }
        }

        // Show items
        if (quest.itemRewards != null)
        {
            foreach (var item in quest.itemRewards)
            {
                var rewardUI = Instantiate(rewardItemPrefab, rewardsContainer);
                rewardUI.Setup(item.itemName, "x1", item.icon);
            }
        }

        // Auto-close after duration
        StartCoroutine(AutoClose());
    }

    IEnumerator AutoClose()
    {
        yield return new WaitForSeconds(displayDuration);
        Close();
    }

    void Close()
    {
        rewardPanel.SetActive(false);
    }
}
