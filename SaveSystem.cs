using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    static string Path => Application.persistentDataPath + "/save.json";

    public static void SaveGame()
    {
        SaveData data = new();

        foreach (var pair in InventoryManager.Instance.GetItems())
        {
            data.itemIDs.Add(pair.Key);
            data.itemCounts.Add(pair.Value);
        }

        data.currentScene = SceneManager.GetActiveScene().name;
        data.storyFlags.AddRange(StoryFlags.flags);

        if (TimePhaseManager.Instance != null)
        {
            data.currentTimePhase = TimePhaseManager.Instance.currentPhase;
            data.phaseTimer = TimePhaseManager.Instance.GetPhaseProgress() * TimePhaseManager.Instance.phaseDuration;
        }

        if (TimeUI.Instance != null)
        {
            data.currentDay = TimeUI.Instance.GetCurrentDay();
        }

        if (PlayerStats.Instance != null)
        {
            data.statTypes.Clear();
            data.statValues.Clear();

            foreach (var stat in PlayerStats.Instance.stats)
            {
                data.statTypes.Add(stat.type);
                data.statValues.Add(stat.currentValue);
            }
        }

        if (ProfileManager.Instance != null)
        {
            data.playerName = ProfileManager.Instance.profile.playerName;
            data.playerLevel = ProfileManager.Instance.profile.level;
            data.playerExperience = ProfileManager.Instance.profile.experience;
            data.playerExperienceToNext = ProfileManager.Instance.profile.experienceToNextLevel;
            data.playerCurrency = ProfileManager.Instance.profile.currency;
        }

        if (ShopManager.Instance != null)
        {
            foreach (var item in ShopManager.Instance.shopItems)
            {
                if (!item.unlimitedStock)
                {
                    int stock = ShopManager.Instance.GetStock(item.item.itemID);
                    data.shopStockIDs.Add(item.item.itemID);
                    data.shopStockAmounts.Add(stock);
                }
            }
        }

        if (EquipmentManager.Instance != null)
        {
            data.equippedItems.Clear();

            var equipped = EquipmentManager.Instance.GetAllEquipped();
            foreach (var kvp in equipped)
            {
                data.equippedItems.Add(new EquippedItemSave
                {
                    slot = kvp.Key,
                    itemID = kvp.Value.itemID
                });
            }
        }

        if (CurrencyManager.Instance != null)
        {
            data.currencyTypes.Clear();
            data.currencyAmounts.Clear();

            var currencies = CurrencyManager.Instance.GetAllCurrencies();
            foreach (var kvp in currencies)
            {
                data.currencyTypes.Add(kvp.Key);
                data.currencyAmounts.Add(kvp.Value);
            }
        }

        if (ScenarioManager.Instance != null)
        {
            data.completedScenarios.AddRange(ScenarioManager.Instance.GetCompletedScenarios());

            if (ScenarioManager.Instance.IsScenarioActive())
            {
                data.activeScenarioID = ScenarioManager.Instance.GetCurrentScenario().scenarioID;
                data.activeScenarioStep = ScenarioManager.Instance.GetCurrentStepIndex();
            }
        }

        File.WriteAllText(Path, JsonUtility.ToJson(data, true));
    }

    public static void LoadGame()
    {
        if (!File.Exists(Path)) return;

        SaveData data = JsonUtility.FromJson<SaveData>(File.ReadAllText(Path));

        InventoryManager.Instance.Clear();

        for (int i = 0; i < data.itemIDs.Count; i++)
        {
            ItemData item = ItemDatabase.Instance.GetByID(data.itemIDs[i]);
            InventoryManager.Instance.AddItem(item, data.itemCounts[i]);
        }

        StoryFlags.flags = new(data.storyFlags);

        if (TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.SetPhase(data.currentTimePhase);
            TimePhaseManager.Instance.SetPhaseTimer(data.phaseTimer);
            TimePhaseManager.Instance.SetIsFirstMorning(false); // Prevent day increment on load
        }

        if (TimeUI.Instance != null)
        {
            TimeUI.Instance.SetDay(data.currentDay);
        }

        if (PlayerStats.Instance != null)
        {
            for (int i = 0; i < data.statTypes.Count; i++)
            {
                PlayerStats.Instance.Set(data.statTypes[i], data.statValues[i], false);
            }
        }

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.profile.playerName = data.playerName;
            ProfileManager.Instance.profile.level = data.playerLevel;
            ProfileManager.Instance.profile.experience = data.playerExperience;
            ProfileManager.Instance.profile.experienceToNextLevel = data.playerExperienceToNext;
            ProfileManager.Instance.profile.currency = data.playerCurrency;
        }

        if (EquipmentManager.Instance != null)
        {
            // First unequip all
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                EquipmentManager.Instance.Unequip(slot, false);
            }

            // Then equip saved items
            foreach (var saved in data.equippedItems)
            {
                var equipment = ItemDatabase.Instance.GetByID(saved.itemID) as EquipmentData;
                if (equipment != null)
                {
                    EquipmentManager.Instance.Equip(equipment);
                }
            }
        }

        if (CurrencyManager.Instance != null)
        {
            for (int i = 0; i < data.currencyTypes.Count; i++)
            {
                CurrencyManager.Instance.Set(data.currencyTypes[i], data.currencyAmounts[i]);
            }
        }

        if (ScenarioManager.Instance != null)
        {
            ScenarioManager.Instance.SetCompletedScenarios(new HashSet<string>(data.completedScenarios));

            // Note: Restoring active scenarios requires additional logic
            // to find and resume the correct scenario
        }

        if (!string.IsNullOrEmpty(data.currentScene))
            SceneManager.LoadScene(data.currentScene);
    }
}
