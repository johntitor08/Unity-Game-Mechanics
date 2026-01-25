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
        [Tooltip("Bu drop için minimum luck deðeri")]
        public int minLuckRequired = 0;
    }

    [Header("Equipment Pool")]
    public List<EquipmentDrop> possibleEquipment = new();

    [Header("Rarity Drop Chances")]
    [Range(0f, 1f)] public float commonChance = 0.50f;
    [Range(0f, 1f)] public float uncommonChance = 0.30f;
    [Range(0f, 1f)] public float rareChance = 0.15f;
    [Range(0f, 1f)] public float epicChance = 0.04f;
    [Range(0f, 1f)] public float legendaryChance = 0.01f;

    [Header("Drop Settings")]
    public int minDrops = 0;
    public int maxDrops = 2;
    [Range(0f, 1f)]
    public float equipmentDropChance = 0.3f;

    [Header("Luck Bonus")]
    [Tooltip("Her luck puaný için ekstra þans (%)")]
    public float luckBonusPerPoint = 0.5f;

    public List<EquipmentData> RollLoot(int playerLuck)
    {
        List<EquipmentData> droppedItems = new();

        // Önce ekipman düþecek mi kontrol et
        float totalDropChance = equipmentDropChance + (playerLuck * luckBonusPerPoint / 100f);

        if (Random.value > totalDropChance)
        {
            return droppedItems; // Ekipman düþmedi
        }

        // Kaç tane düþecek
        int dropCount = Random.Range(minDrops, maxDrops + 1);

        for (int i = 0; i < dropCount; i++)
        {
            EquipmentData item = RollSingleItem(playerLuck);

            if (item != null)
            {
                droppedItems.Add(item);
            }
        }

        return droppedItems;
    }

    EquipmentData RollSingleItem(int playerLuck)
    {
        // Önce rarity belirle
        EquipmentRarity targetRarity = RollRarity(playerLuck);

        // O rarity'deki itemleri filtrele
        var eligibleItems = possibleEquipment
            .Where(e => e.equipment != null &&
                   e.equipment.equipmentRarity == targetRarity &&
                   playerLuck >= e.minLuckRequired)
            .ToList();

        if (eligibleItems.Count == 0)
        {
            // Bu rarity'de item yok, rastgele rarity dene
            eligibleItems = possibleEquipment
                .Where(e => e.equipment != null && playerLuck >= e.minLuckRequired)
                .ToList();
        }

        if (eligibleItems.Count == 0)
            return null;

        // Aðýrlýklý rastgele seçim
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

        // Fallback
        return eligibleItems[Random.Range(0, eligibleItems.Count)].equipment;
    }

    EquipmentRarity RollRarity(int playerLuck)
    {
        float luckBonus = playerLuck * luckBonusPerPoint / 100f;
        float roll = Random.value;
        float cumulative = 0f;
        cumulative += legendaryChance + luckBonus;
        if (roll <= cumulative) return EquipmentRarity.Legendary;
        cumulative += epicChance + luckBonus * 0.5f;
        if (roll <= cumulative) return EquipmentRarity.Epic;
        cumulative += rareChance + luckBonus * 0.3f;
        if (roll <= cumulative) return EquipmentRarity.Rare;
        cumulative += uncommonChance;
        if (roll <= cumulative) return EquipmentRarity.Uncommon;
        return EquipmentRarity.Common;
    }
}
