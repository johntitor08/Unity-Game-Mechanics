using UnityEngine;
using System;

[System.Serializable]
public class DailyCurrencyReward : MonoBehaviour
{
    [Header("Daily Reward")]
    public CurrencyType rewardType = CurrencyType.Gold;
    public int baseRewardAmount = 100;
    public int streakBonus = 50;

    private DateTime lastClaimDate;
    private int currentStreak = 0;

    void Start() => LoadLastClaimDate();

    public bool CanClaim() => lastClaimDate == DateTime.MinValue || (DateTime.Now - lastClaimDate).TotalHours >= 24;

    public void ClaimDailyReward()
    {
        if (!CanClaim())
        {
            Debug.Log("Daily reward already claimed today!");
            return;
        }

        TimeSpan timeSinceClaim = DateTime.Now - lastClaimDate;
        currentStreak = (timeSinceClaim.TotalHours >= 24 && timeSinceClaim.TotalHours < 48) ? currentStreak + 1 : 1;
        int totalReward = baseRewardAmount + (streakBonus * (currentStreak - 1));

        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddMultiple(
                new System.Collections.Generic.Dictionary<CurrencyType, int>
                {
                    { rewardType, totalReward }
                }
            );
        }

        lastClaimDate = DateTime.Now;
        SaveLastClaimDate();
        Debug.Log($"Daily reward claimed! {totalReward} {rewardType} (Streak: {currentStreak})");
    }

    void SaveLastClaimDate()
    {
        PlayerPrefs.SetString("LastDailyRewardClaim", lastClaimDate.ToString());
        PlayerPrefs.SetInt("DailyRewardStreak", currentStreak);
        PlayerPrefs.Save();
    }

    void LoadLastClaimDate()
    {
        string saved = PlayerPrefs.GetString("LastDailyRewardClaim", "");

        if (!string.IsNullOrEmpty(saved))
            DateTime.TryParse(saved, out lastClaimDate);

        currentStreak = PlayerPrefs.GetInt("DailyRewardStreak", 0);
    }

    public int GetCurrentStreak() => currentStreak;

    public TimeSpan GetTimeUntilNextReward() => CanClaim() ? TimeSpan.Zero : TimeSpan.FromHours(24) - (DateTime.Now - lastClaimDate);
}
