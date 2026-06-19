[System.Serializable]
public class StatModifier
{
    public StatType statType;
    public int amount;
    public bool isPercentage;
    public bool isTemporary;
    public float duration;

    public StatModifier(StatType type, int value, bool percentage = false)
    {
        statType = type;
        amount = value;
        isPercentage = percentage;
    }
}
