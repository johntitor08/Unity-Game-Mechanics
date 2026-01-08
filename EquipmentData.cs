using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Scriptable Objects/EquipmentData")]
public class EquipmentData : ItemData
{
    [Header("Equipment Properties")]
    public EquipmentSlot slot;

    [Header("Combat Bonuses")]
    public int damageBonus = 0;
    public int defenseBonus = 0;

    [Header("Stat Bonuses")]
    public StatType primaryStat;
    public int primaryStatBonus = 0;
    public StatType secondaryStat;
    public int secondaryStatBonus = 0;

    [Header("Requirements")]
    public int requiredLevel = 1;
    public StatType requiredStat;
    public int requiredStatValue = 0;

    [Header("Rarity")]
    public EquipmentRarity rarity = EquipmentRarity.Common;

    [Header("Set Bonus")]
    public string setName = "";
    public int setID = 0;

    public string GetStatsDescription()
    {
        string desc = "";

        if (damageBonus > 0)
            desc += $"Damage: +{damageBonus}\n";

        if (defenseBonus > 0)
            desc += $"Defense: +{defenseBonus}\n";

        if (primaryStatBonus > 0)
            desc += $"{primaryStat}: +{primaryStatBonus}\n";

        if (secondaryStatBonus > 0)
            desc += $"{secondaryStat}: +{secondaryStatBonus}\n";

        return desc;
    }

    public Color GetRarityColor()
    {
        return rarity switch
        {
            EquipmentRarity.Common => new Color(0.8f, 0.8f, 0.8f), // Gray
            EquipmentRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f), // Green
            EquipmentRarity.Rare => new Color(0.2f, 0.5f, 1f), // Blue
            EquipmentRarity.Epic => new Color(0.8f, 0.2f, 0.8f), // Purple
            EquipmentRarity.Legendary => new Color(1f, 0.6f, 0f), // Orange
            _ => Color.white
        };
    }
}

public enum EquipmentRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
