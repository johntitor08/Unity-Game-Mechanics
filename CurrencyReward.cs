using System.Collections.Generic;

[System.Serializable]
public class CurrencyReward
{
    public CurrencyType type;
    public int amount;

    public void Grant(bool showNotification = true)
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.Add(type, amount, showNotification);
        }
    }
}

[System.Serializable]
public class MultiCurrencyReward
{
    public CurrencyReward[] rewards;

    public void GrantAll(bool showNotification = true)
    {
        if (rewards == null || rewards.Length == 0) return;
        var rewardDict = new Dictionary<CurrencyType, int>();

        // Aggregate rewards into a dictionary
        foreach (var reward in rewards)
        {
            if (reward.amount <= 0) continue;

            if (rewardDict.ContainsKey(reward.type))
                rewardDict[reward.type] += reward.amount;
            else
                rewardDict[reward.type] = reward.amount;
        }

        // Give all rewards at once
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.AddMultiple(rewardDict, showNotification);
        }
    }
}
