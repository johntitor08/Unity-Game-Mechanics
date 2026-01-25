using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    static string Path => Application.persistentDataPath + "/save.json";

    public static bool HasSaveFile()
    {
        return System.IO.File.Exists(Path);
    }

    public static void DeleteSave()
    {
        if (System.IO.File.Exists(Path))
        {
            System.IO.File.Delete(Path);
            Debug.Log("Save file deleted");
        }
    }

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

            foreach (var currency in currencies)
                {
                data.currencyTypes.Add(currency.Key);
                data.currencyAmounts.Add(currency.Value);
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

        if (QuestManager.Instance != null)
        {
            data.activeQuests.Clear();

            foreach (var quest in QuestManager.Instance.GetActiveQuests())
            {
                var questSave = new QuestSaveData { questID = quest.questID };

                foreach (var obj in quest.objectives)
                {
                    questSave.objectiveProgress.Add(obj.currentProgress);
                    questSave.objectiveCompleted.Add(obj.isCompleted);
                }

                data.activeQuests.Add(questSave);
            }

            data.completedQuests.Clear();

            foreach (var quest in QuestManager.Instance.GetCompletedQuests())
            {
                data.completedQuests.Add(quest.questID);
            }

            if (QuestTrackerUI.Instance != null)
            {
                data.trackedQuests = QuestTrackerUI.Instance.GetTrackedQuests();
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
            TimePhaseManager.Instance.SetIsFirstMorning(false);
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
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
            {
                EquipmentManager.Instance.Unequip(slot, false);
            }

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
        }

        if (QuestManager.Instance != null)
        {
            foreach (var questSave in data.activeQuests)
            {
                var quest = QuestManager.Instance.allQuests
                    .FirstOrDefault(q => q.questID == questSave.questID);

                if (quest != null)
                {
                    for (int i = 0; i < quest.objectives.Length && i < questSave.objectiveProgress.Count; i++)
                    {
                        quest.objectives[i].currentProgress = questSave.objectiveProgress[i];
                        quest.objectives[i].isCompleted = questSave.objectiveCompleted[i];
                    }

                    QuestManager.Instance.StartQuest(quest);
                }
            }

            if (QuestTrackerUI.Instance != null)
            {
                foreach (var questID in data.trackedQuests)
                {
                    QuestTrackerUI.Instance.TrackQuest(questID);
                }
            }
        }

        if (!string.IsNullOrEmpty(data.currentScene))
            SceneManager.LoadScene(data.currentScene);
    }
}
