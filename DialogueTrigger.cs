using UnityEngine;
using System;

public class DailyCurrencyReward : MonoBehaviour
{
    [Header("Daily Reward")]
    public CurrencyType rewardType = CurrencyType.Gold;
    public int baseRewardAmount = 100;
    public int streakBonus = 50;
    public int maxStreak = 30;

    private DateTime lastClaimDate;
    private int currentStreak = 0;

    void Start() => LoadLastClaimDate();

    public bool CanClaim()
    {
        if (lastClaimDate == DateTime.MinValue || DateTime.Now < lastClaimDate)
            return true;

        return (DateTime.Now - lastClaimDate).TotalHours >= 24;
    }

    public void ClaimDailyReward()
    {
        if (!CanClaim())
        {
            Debug.Log("Daily reward already claimed today!");
            return;
        }

        bool isFirstClaim = lastClaimDate == DateTime.MinValue;
        TimeSpan timeSinceClaim = isFirstClaim ? TimeSpan.Zero : DateTime.Now - lastClaimDate;
        bool maintainsStreak = !isFirstClaim && timeSinceClaim.TotalHours >= 24 && timeSinceClaim.TotalHours < 48;
        currentStreak = maintainsStreak ? Mathf.Min(currentStreak + 1, maxStreak) : 1;
        int totalReward = baseRewardAmount + (streakBonus * Mathf.Max(0, currentStreak - 1));

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
        Debug.Log($"Daily reward claimed! {totalReward} {rewardType} (Streak: {currentStreak}/{maxStreak})");
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
