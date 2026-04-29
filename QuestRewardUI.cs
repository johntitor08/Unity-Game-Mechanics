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
        if (quest == null)
        {
            Debug.LogWarning("QuestRewardUI.ShowRewards called with null quest.");
            return;
        }

        if (rewardItemPrefab == null)
        {
            Debug.LogError("QuestRewardUI: rewardItemPrefab is not assigned.");
            return;
        }

        if (autoCloseCoroutine != null)
            StopCoroutine(autoCloseCoroutine);

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
                int qty = (quest.itemRewardQuantities != null && i < quest.itemRewardQuantities.Length) ? quest.itemRewardQuantities[i] : 1;
                SpawnRewardItem(rewardsContainer, quest.itemRewards[i].itemName, $"x{qty}", quest.itemRewards[i].icon);
            }
        }

        bool hasOptional = quest.optionalRewards != null && quest.optionalRewards.Length > 0;

        if (optionalRewardsLabel != null)
            optionalRewardsLabel.gameObject.SetActive(hasOptional);

        ClearContainer(optionalRewardsContainer);

        if (hasOptional && optionalRewardsContainer != null)
        {
            foreach (var item in quest.optionalRewards)
                SpawnRewardItem(optionalRewardsContainer, item.itemName, "x1", item.icon);
        }

        autoCloseCoroutine = StartCoroutine(AutoClose());
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

        rewardPanel.SetActive(false);
    }
}
