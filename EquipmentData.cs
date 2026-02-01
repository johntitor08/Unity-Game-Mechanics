using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentData", menuName = "Equipment/EquipmentData")]
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

    [Header("Equipment Rarity Mapping")]
    public EquipmentRarity equipmentRarity = EquipmentRarity.Common;

    [Header("Set Bonus")]
    public string setName = "";
    public int setID = 0;

    [Header("Set Data")]
    public EquipmentSetData setData;

    // Constructor to ensure itemType is always Equipment
    void OnEnable()
    {
        itemType = ItemType.Equipment;
        stackable = false; // Equipment should not stack
        maxStack = 1;
    }

    public override bool IsEquipment()
    {
        return true;
    }

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
}

public enum EquipmentRarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
