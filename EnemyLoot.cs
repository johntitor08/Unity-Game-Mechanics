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

    [Header("Luck Modifiers")]
    [Tooltip("How much each point of luck increases drop chance (0.005 = 0.5% per luck point)")]
    public float luckDropChanceBonus = 0.005f;

    [Header("Debug")]
    public bool showDropLog = true;

    private static readonly Dictionary<Rarity, string> RarityColors = new()
    {
        {
            Rarity.Common, "#CCCCCC"
        },
        {
            Rarity.Rare, "#3399FF"
        },
        {
            Rarity.Epic, "#CC33FF"
        },
        {
            Rarity.Legendary, "#FF9933"
        },
        {
            Rarity.Godly, "#CC2200"
        }
    };

    private void OnValidate()
    {
        if (possibleLoot != null && lootChances != null && possibleLoot.Length != lootChances.Length)
        {
            Debug.LogWarning($"[{gameObject.name}] Loot arrays length mismatch! possibleLoot: {possibleLoot.Length}, lootChances: {lootChances.Length}");
        }

        if (minGold > maxGold)
        {
            Debug.LogWarning($"[{gameObject.name}] minGold ({minGold}) is greater than maxGold ({maxGold})");
        }
    }

    public void DropLoot(EnemyData data = null)
    {
        ItemData[] loot = data != null ? data.possibleLoot : possibleLoot;
        float[] chances = data != null ? data.lootChances : lootChances;
        EquipmentLootTable table = data != null ? data.equipmentLootTable : equipmentLootTable;
        int playerLuck = GetPlayerLuck();
        DropEquipment(table, playerLuck);
        DropRegularLoot(loot, chances, playerLuck);
        DropCurrency();
    }

    private void DropEquipment(EquipmentLootTable table, int playerLuck)
    {
        if (table == null)
            return;

        List<EquipmentData> droppedEquipment = table.RollLoot(playerLuck);

        if (droppedEquipment == null || droppedEquipment.Count == 0)
            return;

        foreach (var equipment in droppedEquipment)
        {
            if (equipment == null)
                continue;

            AddItemToInventory(equipment, 1);
            string rarityColor = GetRarityColorHex(equipment.rarity);
            LogDrop($"<color={rarityColor}>{equipment.itemName}</color> (Equipment)");
        }
    }

    private void DropRegularLoot(ItemData[] possibleLoot, float[] lootChances, int playerLuck)
    {
        if (possibleLoot == null || lootChances == null)
            return;

        int itemCount = Mathf.Min(possibleLoot.Length, lootChances.Length);

        for (int i = 0; i < itemCount; i++)
        {
            ItemData item = possibleLoot[i];

            if (item == null)
                continue;

            float dropChance = CalculateDropChance(lootChances[i], playerLuck);

            if (Random.value <= dropChance)
            {
                AddItemToInventory(item, 1);
                LogDrop($"<color=yellow>{item.itemName}</color>");
            }
        }
    }

    private int GetPlayerLuck()
    {
        return PlayerStats.Instance != null ? PlayerStats.Instance.Get(StatType.Luck) : 0;
    }

    private float CalculateDropChance(float baseChance, int playerLuck)
    {
        if (baseChance <= 0f)
            return 0f;

        float luckBonus = playerLuck * luckDropChanceBonus;
        return Mathf.Clamp01(baseChance + luckBonus);
    }

    private void DropCurrency()
    {
        if (ProfileManager.Instance == null)
            return;

        int gold = Random.Range(minGold, maxGold + 1);
        ProfileManager.Instance.AddCurrency(gold);
        LogDrop($"<color=#FFD700>{gold} Gold</color>");
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
        if (!showDropLog)
            return;

        if (CombatUI.Instance != null)
        {
            CombatUI.Instance.AddLogMessage($"Obtained: {message}");
        }
        else if (Debug.isDebugBuild)
        {
            Debug.Log($"[{gameObject.name}] Drop: {message}");
        }
    }

    private string GetRarityColorHex(Rarity rarity)
    {
        return RarityColors.TryGetValue(rarity, out string color) ? color : "#FFFFFF";
    }

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
