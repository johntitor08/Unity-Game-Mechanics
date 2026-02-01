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

    [Header("Luck Modifiers")]
    [Tooltip("How much each point of luck increases drop chance (0.005 = 0.5% per luck point)")]
    public float luckDropChanceBonus = 0.005f;

    [Header("Debug")]
    public bool showDropLog = true;

    // Cache for color strings to avoid repeated switch statements
    private static readonly Dictionary<EquipmentRarity, string> RarityColors = new()
    {
        { EquipmentRarity.Common, "#CCCCCC" },
        { EquipmentRarity.Uncommon, "#33FF33" },
        { EquipmentRarity.Rare, "#3399FF" },
        { EquipmentRarity.Epic, "#CC33FF" },
        { EquipmentRarity.Legendary, "#FF9933" }
    };

    private void OnValidate()
    {
        // Ensure arrays match in length
        if (possibleLoot != null && lootChances != null && possibleLoot.Length != lootChances.Length)
        {
            Debug.LogWarning($"[{gameObject.name}] Loot arrays length mismatch! possibleLoot: {possibleLoot.Length}, lootChances: {lootChances.Length}");
        }

        // Validate gold range
        if (minGold > maxGold)
        {
            Debug.LogWarning($"[{gameObject.name}] minGold ({minGold}) is greater than maxGold ({maxGold})");
        }
    }

    public void DropLoot()
    {
        int playerLuck = GetPlayerLuck();
        DropEquipment(playerLuck);
        DropRegularLoot(playerLuck);
        DropCurrencyAndExp();
    }

    private int GetPlayerLuck()
    {
        return PlayerStats.Instance != null
            ? PlayerStats.Instance.Get(StatType.Luck)
            : 0;
    }

    private void DropEquipment(int playerLuck)
    {
        if (equipmentLootTable == null) return;
        List<EquipmentData> droppedEquipment = equipmentLootTable.RollLoot(playerLuck);
        if (droppedEquipment == null || droppedEquipment.Count == 0) return;

        foreach (var equipment in droppedEquipment)
        {
            if (equipment == null) continue;
            AddItemToInventory(equipment, 1);
            string rarityColor = GetRarityColorHex(equipment.equipmentRarity);
            LogDrop($"<color={rarityColor}>{equipment.itemName}</color> (Equipment)");
        }
    }

    private void DropRegularLoot(int playerLuck)
    {
        if (possibleLoot == null || lootChances == null) return;
        int itemCount = Mathf.Min(possibleLoot.Length, lootChances.Length);

        for (int i = 0; i < itemCount; i++)
        {
            ItemData item = possibleLoot[i];
            if (item == null) continue;
            float dropChance = CalculateDropChance(lootChances[i], playerLuck);

            if (Random.value <= dropChance)
            {
                AddItemToInventory(item, 1);
                LogDrop($"<color=yellow>{item.itemName}</color>");
            }
        }
    }

    private float CalculateDropChance(float baseChance, int playerLuck)
    {
        if (baseChance <= 0f) return 0f;
        float luckBonus = playerLuck * luckDropChanceBonus;
        return Mathf.Clamp01(baseChance + luckBonus);
    }

    private void DropCurrencyAndExp()
    {
        if (ProfileManager.Instance == null) return;
        int gold = Random.Range(minGold, maxGold + 1);
        ProfileManager.Instance.AddCurrency(gold);
        ProfileManager.Instance.AddExperience(experienceReward);
        LogDrop($"<color=#FFD700>{gold} Gold</color>");
        LogDrop($"<color=#00FF00>{experienceReward} EXP</color>");
    }

    private void AddItemToInventory(ItemData item, int quantity)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.AddItem(item, quantity);
        }
        else
        {
            Debug.LogWarning($"[{gameObject.name}] InventoryManager.Instance is null! Cannot add {item.itemName}");
        }
    }

    private void LogDrop(string message)
    {
        if (!showDropLog) return;

        if (CombatUI.Instance != null)
        {
            CombatUI.Instance.AddLogMessage($"Obtained: {message}");
        }
        else if (Debug.isDebugBuild)
        {
            Debug.Log($"[{gameObject.name}] Drop: {message}");
        }
    }

    private string GetRarityColorHex(EquipmentRarity rarity)
    {
        return RarityColors.TryGetValue(rarity, out string color)
            ? color
            : "#FFFFFF";
    }

    // Optional: Method to preview total drop chances with current luck
    [ContextMenu("Preview Drop Chances")]
    private void PreviewDropChances()
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("Preview only works in Play mode when PlayerStats is available");
            return;
        }

        int playerLuck = GetPlayerLuck();
        Debug.Log($"=== Drop Chances Preview (Luck: {playerLuck}) ===");

        if (possibleLoot != null && lootChances != null)
        {
            for (int i = 0; i < Mathf.Min(possibleLoot.Length, lootChances.Length); i++)
            {
                if (possibleLoot[i] != null)
                {
                    float chance = CalculateDropChance(lootChances[i], playerLuck);
                    Debug.Log($"{possibleLoot[i].itemName}: {chance * 100f:F2}%");
                }
            }
        }
    }
}
