using System.Collections.Generic;
using UnityEngine;

public class EnemyLoot : MonoBehaviour
{
    [Header("Loot Tables")]
    public EquipmentLootTable equipmentLootTable;

    [Header("Regular Loot")]
    public ItemData[] possibleLoot;
    [Range(0f, 1f)]
    public float[] lootChances;

    [Header("Currency")]
    public int minGold = 10;
    public int maxGold = 50;
    public int experienceReward = 50;

    [Header("Debug")]
    public bool showDropLog = true;

    public void DropLoot()
    {
        int playerLuck = PlayerStats.Instance != null
            ? PlayerStats.Instance.Get(StatType.Luck)
            : 0;

        // Ekipman düþür
        if (equipmentLootTable != null)
        {
            List<EquipmentData> droppedEquipment = equipmentLootTable.RollLoot(playerLuck);

            foreach (var equipment in droppedEquipment)
            {
                if (equipment != null && InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(equipment, 1);
                    string rarityColor = GetRarityColorHex(equipment.equipmentRarity);
                    LogDrop($"<color={rarityColor}>{equipment.itemName}</color> (Equipment)");
                }
            }
        }

        DropRegularLoot(playerLuck);
        DropCurrencyAndExp();
    }

    void DropRegularLoot(int playerLuck)
    {
        if (possibleLoot == null || lootChances == null) return;

        for (int i = 0; i < possibleLoot.Length && i < lootChances.Length; i++)
        {
            if (possibleLoot[i] == null) continue;
            float chance = lootChances[i];

            // Luck bonus
            if (chance > 0)
            {
                chance = Mathf.Clamp01(chance + playerLuck * 0.005f);
            }

            if (Random.value <= chance)
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.AddItem(possibleLoot[i], 1);
                }

                LogDrop($"<color=yellow>{possibleLoot[i].itemName}</color>");
            }
        }
    }

    void DropCurrencyAndExp()
    {
        if (ProfileManager.Instance != null)
        {
            int gold = Random.Range(minGold, maxGold + 1);
            ProfileManager.Instance.AddCurrency(gold);
            ProfileManager.Instance.AddExperience(experienceReward);
            LogDrop($"<color=#FFD700>{gold} Gold</color>");
            LogDrop($"<color=#00FF00>{experienceReward} EXP</color>");
        }
    }

    void LogDrop(string message)
    {
        if (!showDropLog) return;

        if (CombatUI.Instance != null)
        {
            CombatUI.Instance.AddLogMessage($"Obtained: {message}");
        }
    }

    string GetRarityColorHex(EquipmentRarity rarity)
    {
        return rarity switch
        {
            EquipmentRarity.Common => "#CCCCCC",
            EquipmentRarity.Uncommon => "#33FF33",
            EquipmentRarity.Rare => "#3399FF",
            EquipmentRarity.Epic => "#CC33FF",
            EquipmentRarity.Legendary => "#FF9933",
            _ => "#FFFFFF"
        };
    }
}
