using UnityEngine;

[System.Serializable]
public class CurrencyReward
{
    public CurrencyType type;
    public int amount;

    public void Grant()
    {
        if (CurrencyManager.Instance != null)
        {
            CurrencyManager.Instance.Add(type, amount);
        }
    }
}

// For quest/achievement rewards
[System.Serializable]
public class MultiCurrencyReward
{
    public CurrencyReward[] rewards;

    public void GrantAll()
    {
        foreach (var reward in rewards)
        {
            reward.Grant();
        }
    }
}
