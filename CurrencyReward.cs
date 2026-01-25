using System;
using System.Collections.Generic;

[Serializable]
public class CurrencyReward
{
    public CurrencyType type;
    public int amount;

    public void Grant(bool showNotification = true)
    {
        if (CurrencyManager.Instance == null) return;
        if (amount <= 0) return;
        CurrencyManager.Instance.Add(type, amount, showNotification);
    }
}

[Serializable]
public class MultiCurrencyReward
{
    public CurrencyReward[] rewards;

    public void GrantAll(bool showNotification = true)
    {
        if (rewards == null || rewards.Length == 0) return;
        if (CurrencyManager.Instance == null) return;
        Dictionary<CurrencyType, int> rewardDict = new();

        foreach (var reward in rewards)
        {
            if (reward == null || reward.amount <= 0) continue;

            if (rewardDict.ContainsKey(reward.type))
                rewardDict[reward.type] += reward.amount;
            else
                rewardDict.Add(reward.type, reward.amount);
        }

        if (rewardDict.Count > 0)
            CurrencyManager.Instance.AddMultiple(rewardDict, showNotification);
    }
}
