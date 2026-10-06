using System.Collections.Generic;
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

    public string GetDisplayName() => upgradeLevel > 0 ? $"{baseData.DisplayName} +{upgradeLevel}" : baseData.DisplayName;

    public List<(StatType stat, int value)> GetStatTotals()
    {
        var totals = new List<(StatType stat, int value)>();

        void Add(StatType stat, int value)
        {
            if (value <= 0)
                return;

            for (int i = 0; i < totals.Count; i++)
            {
                if (totals[i].stat == stat)
                {
                    totals[i] = (stat, totals[i].value + value);
                    return;
                }
            }

            totals.Add((stat, value));
        }

        Add(StatType.Damage, GetDamageBonus());
        Add(StatType.Defense, GetDefenseBonus());
        Add(baseData.primaryStat, GetPrimaryBonus());
        Add(baseData.secondaryStat, GetSecondaryBonus());
        return totals;
    }

    public int GetStatTotal(StatType stat)
    {
        foreach (var (s, v) in GetStatTotals())
            if (s == stat)
                return v;

        return 0;
    }

    public string GetStatsDescription()
    {
        string desc = "";

        foreach (var (stat, value) in GetStatTotals())
            desc += $"{stat.Display()}: +{value}\n";

        if (upgradeLevel > 0)
            desc += $"<color={UIPalette.Hex(UIPalette.Gold)}>{Loc.T("Upgrade", "Geliştirme")}: +{upgradeLevel}</color>";

        return desc;
    }

    public EquipmentInstance Clone() => new(baseData, upgradeLevel);
}
