using UnityEngine;
using System;

public class DailyCurrencyReward : MonoBehaviour
{
    [Header("Daily Reward")]
    public CurrencyType rewardType = CurrencyType.Gold;
    public int baseRewardAmount = 100;
    public int streakBonus = 50; // Bonus per consecutive day

    private DateTime lastClaimDate;
    private int currentStreak = 0;

    void Start()
    {
        LoadLastClaimDate();
    }

    public bool CanClaim()
    {
        if (lastClaimDate == DateTime.MinValue)
            return true;

        TimeSpan timeSinceClaim = DateTime.Now - lastClaimDate;
        return timeSinceClaim.TotalHours >= 24;
    }

    public void ClaimDailyReward()
    {
        if (!CanClaim())
        {
            Debug.Log("Daily reward already claimed today!");
            return;
        }

        // Check if streak continues
        TimeSpan timeSinceClaim = DateTime.Now - lastClaimDate;
        if (timeSinceClaim.TotalHours >= 24 && timeSinceClaim.TotalHours < 48)
        {
            currentStreak++;
        }
        else
        {
            currentStreak = 1; // Reset streak
        }

        // Calculate reward
        int totalReward = baseRewardAmount + (streakBonus * (currentStreak - 1));

        // Grant reward
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.Add(rewardType, totalReward);
        }

        // Update last claim
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
        {
            DateTime.TryParse(saved, out lastClaimDate);
        }

        currentStreak = PlayerPrefs.GetInt("DailyRewardStreak", 0);
    }

    public int GetCurrentStreak() => currentStreak;
    public TimeSpan GetTimeUntilNextReward()
    {
        if (CanClaim()) return TimeSpan.Zero;

        TimeSpan timeSinceClaim = DateTime.Now - lastClaimDate;
        return TimeSpan.FromHours(24) - timeSinceClaim;
    }
}
