using System;
using UnityEngine;

[Serializable]
public class EquipmentInstance
{
    public EquipmentData baseData;
    public int upgradeLevel = 0;

    public EquipmentInstance(EquipmentData data, int upgrade = 0)
    {
        baseData = data;
        upgradeLevel = upgrade;
    }

    public int GetDamageBonus() => baseData.damageBonus + upgradeLevel;

    public int GetDefenseBonus() => baseData.defenseBonus + upgradeLevel;

    public int GetPrimaryBonus() => baseData.primaryStatBonus + upgradeLevel;

    public int GetSecondaryBonus() => baseData.secondaryStatBonus + upgradeLevel;

    public bool CanUpgrade() => upgradeLevel < baseData.maxUpgradeLevel;

    public string GetDisplayName() => upgradeLevel > 0 ? $"{baseData.itemName} +{upgradeLevel}" : baseData.itemName;

    public string GetStatsDescription()
    {
        string desc = "";

        if (GetDamageBonus() > 0)
            desc += $"Damage: +{GetDamageBonus()}\n";

        if (GetDefenseBonus() > 0)
            desc += $"Defense: +{GetDefenseBonus()}\n";

        if (GetPrimaryBonus() > 0)
            desc += $"{baseData.primaryStat}: +{GetPrimaryBonus()}\n";

        if (GetSecondaryBonus() > 0)
            desc += $"{baseData.secondaryStat}: +{GetSecondaryBonus()}\n";

        if (upgradeLevel > 0)
            desc += $"<color=#FFD700>Upgrade: +{upgradeLevel}</color>";

        return desc;
    }

    public EquipmentInstance Clone() => new(baseData, upgradeLevel);
}
