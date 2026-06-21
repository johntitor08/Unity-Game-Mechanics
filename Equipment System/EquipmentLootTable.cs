using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "EquipmentLootTable", menuName = "Equipment/Equipment Loot Table")]
public class EquipmentLootTable : ScriptableObject
{
    [System.Serializable]
    public class EquipmentDrop
    {
        public EquipmentData equipment;
        [Range(0f, 1f)]
        public float baseDropChance = 0.5f;
        [Tooltip("Bu drop i�in minimum luck de�eri")]
        public int minLuckRequired = 0;
    }

    [Header("Equipment Pool")]
    public List<EquipmentDrop> possibleEquipment = new();

    [Header("Rarity Drop Chances")]
    [Range(0f, 1f)] public float commonChance = 0.50f;
    [Range(0f, 1f)] public float rareChance = 0.15f;
    [Range(0f, 1f)] public float epicChance = 0.04f;
    [Range(0f, 1f)] public float legendaryChance = 0.01f;

    [Header("Drop Settings")]
    public int minDrops = 0;
    public int maxDrops = 2;
    [Range(0f, 1f)]
    public float equipmentDropChance = 0.3f;

    [Header("Luck Bonus")]
    [Tooltip("Her luck puan� i�in ekstra �ans (%)")]
    public float luckBonusPerPoint = 0.5f;

    public List<EquipmentData> RollLoot(int playerLuck)
    {
        List<EquipmentData> droppedItems = new();
        float totalDropChance = equipmentDropChance + (playerLuck * luckBonusPerPoint / 100f);

        if (Random.value > totalDropChance)
        {
            return droppedItems;
        }

        int dropCount = Random.Range(minDrops, maxDrops + 1);
        int attempts = 0;
        int maxAttempts = dropCount * 4;

        while (droppedItems.Count < dropCount && attempts < maxAttempts)
        {
            attempts++;
            EquipmentData item = RollSingleItem(playerLuck);

            if (item != null && !droppedItems.Contains(item))
            {
                droppedItems.Add(item);
            }
        }

        return droppedItems;
    }

    EquipmentData RollSingleItem(int playerLuck)
    {
        Rarity targetRarity = RollRarity(playerLuck);
        var eligibleItems = possibleEquipment.Where(e => e.equipment != null && e.equipment.rarity == targetRarity && playerLuck >= e.minLuckRequired).ToList();

        if (eligibleItems.Count == 0)
        {
            eligibleItems = possibleEquipment.Where(e => e.equipment != null && playerLuck >= e.minLuckRequired).ToList();

            if (eligibleItems.Count == 0)
                return null;
        }

        float totalWeight = eligibleItems.Sum(e => e.baseDropChance);
        float roll = Random.value * totalWeight;
        float current = 0f;

        foreach (var drop in eligibleItems)
        {
            current += drop.baseDropChance;

            if (roll <= current)
            {
                return drop.equipment;
            }
        }

        return eligibleItems[Random.Range(0, eligibleItems.Count)].equipment;
    }

    Rarity RollRarity(int playerLuck)
    {
        float luckBonus = Mathf.Min(playerLuck * luckBonusPerPoint / 100f, 0.05f);
        float roll = Random.value;
        float cumulative = 0f;
        cumulative += legendaryChance + luckBonus;

        if (roll <= cumulative)
            return Rarity.Legendary;

        cumulative += epicChance + luckBonus * 0.5f;

        if (roll <= cumulative)
            return Rarity.Epic;

        cumulative += rareChance + luckBonus * 0.3f;

        if (roll <= cumulative)
            return Rarity.Rare;

        return Rarity.Common;
    }
}
