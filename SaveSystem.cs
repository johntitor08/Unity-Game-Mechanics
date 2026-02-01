using System.Collections.Generic;
using System.IO;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    public static SaveData CachedData;
    
    public static bool HasSaveFile() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public static void SaveGame()
    {
        SaveData data = new();

        if (InventoryManager.Instance != null)
        {
            data.itemIDs.Clear();
            data.itemCounts.Clear();

            foreach (var pair in InventoryManager.Instance.GetItems())
            {
                data.itemIDs.Add(pair.Key);
                data.itemCounts.Add(pair.Value);
            }
        }

        data.currentScene = SceneManager.GetActiveScene().name;
        data.storyFlags.Clear();
        data.storyFlags.AddRange(StoryFlags.flags);

        if (SceneEvent.Instance != null)
            data.sceneProgress = (int)SceneEvent.Instance.Progress;

        if (TimePhaseManager.Instance != null)
        {
            data.currentTimePhase = TimePhaseManager.Instance.currentPhase;
            data.phaseProgress = TimePhaseManager.Instance.GetPhaseProgress();
        }

        if (TimeUI.Instance != null)
            data.currentDay = TimeUI.Instance.GetCurrentDay();

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
            var p = ProfileManager.Instance.profile;
            data.playerName = p.playerName;
            data.playerLevel = p.level;
            data.playerExperience = p.experience;
            data.playerExperienceToNext = p.experienceToNextLevel;
            data.playerCurrency = p.currency;
        }

        if (ShopManager.Instance != null)
        {
            data.shopStockIDs.Clear();
            data.shopStockAmounts.Clear();

            foreach (var s in ShopManager.Instance.GetStockDataForSave())
            {
                data.shopStockIDs.Add(s.itemID);
                data.shopStockAmounts.Add(s.amount);
            }
        }

        if (EquipmentManager.Instance != null)
        {
            data.equippedItems.Clear();

            foreach (var kvp in EquipmentManager.Instance.GetAllEquipped())
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

            foreach (var c in CurrencyManager.Instance.GetAllCurrencies())
            {
                data.currencyTypes.Add(c.Key);
                data.currencyAmounts.Add(c.Value);
            }
        }

        if (ScenarioManager.Instance != null)
        {
            data.completedScenarios.Clear();
            data.completedScenarios.AddRange(
                ScenarioManager.Instance.GetCompletedScenarios()
            );

            if (ScenarioManager.Instance.IsScenarioActive())
            {
                data.activeScenarioID =
                    ScenarioManager.Instance.GetCurrentScenario().scenarioID;
                data.activeScenarioStep =
                    ScenarioManager.Instance.GetCurrentStepIndex();
            }
        }

        if (QuestManager.Instance != null)
        {
            data.activeQuests.Clear();

            foreach (var quest in QuestManager.Instance.GetActiveQuests())
            {
                QuestSaveData q = new() { questID = quest.questID };

                foreach (var obj in quest.objectives)
                {
                    q.objectiveProgress.Add(obj.currentProgress);
                    q.objectiveCompleted.Add(obj.isCompleted);
                }

                data.activeQuests.Add(q);
            }

            data.completedQuests.Clear();
            data.completedQuests.AddRange(QuestManager.Instance.GetCompletedQuests());

            if (QuestTrackerUI.Instance != null)
                data.trackedQuests = QuestTrackerUI.Instance.GetTrackedQuests();
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public static void LoadGame()
    {
        if (!HasSaveFile()) return;
        CachedData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(CachedData.currentScene);

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.StartCoroutine(UpdateUIAfterLoad());
        }
    }
    
    private static System.Collections.IEnumerator UpdateUIAfterLoad()
    {
        yield return null;

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }
    }

    private static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (CachedData != null)
        {
            ApplyLoadedData(CachedData);
            CachedData = null;
        }

        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    public static void ApplyLoadedData(SaveData data)
    {
        if (data == null) return;

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Clear();
            for (int i = 0; i < data.itemIDs.Count; i++)
            {
                ItemData item = ItemDatabase.Instance.GetByID(data.itemIDs[i]);
                if (item != null)
                    InventoryManager.Instance.AddItem(item, data.itemCounts[i]);
            }
        }

        StoryFlags.flags = new HashSet<string>(data.storyFlags);

        if (SceneEvent.Instance != null)
            SceneEvent.Instance.ApplySceneProgress((SceneProgress)data.sceneProgress);

        if (TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.SetPhase(data.currentTimePhase);
            TimePhaseManager.Instance.SetPhaseProgress(data.phaseProgress);
            TimePhaseManager.Instance.SetIsFirstMorning(false);
        }

        if (TimeUI.Instance != null)
            TimeUI.Instance.SetDay(data.currentDay);

        if (PlayerStats.Instance != null)
        {
            for (int i = 0; i < data.statTypes.Count; i++)
            {
                PlayerStats.Instance.Set(
                    data.statTypes[i],
                    data.statValues[i],
                    true
                );
            }
        }

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.ApplyLoadedProfile(new PlayerProfile
            {
                playerName = data.playerName,
                level = data.playerLevel,
                experience = data.playerExperience,
                experienceToNextLevel = data.playerExperienceToNext,
                currency = data.playerCurrency,
                profileIconID = "default"
            });
        }

        if (ShopManager.Instance != null)
            ShopManager.Instance.ApplyLoadedStock(data.shopStockIDs, data.shopStockAmounts);

        if (EquipmentManager.Instance != null)
        {
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
                EquipmentManager.Instance.Unequip(slot, false);

            foreach (var saved in data.equippedItems)
            {
                EquipmentData eq = ItemDatabase.Instance.GetByID(saved.itemID) as EquipmentData;
                if (eq != null) EquipmentManager.Instance.Equip(eq);
            }
        }

        if (CurrencyManager.Instance != null)
        {
            for (int i = 0; i < data.currencyTypes.Count; i++)
                CurrencyManager.Instance.Set(data.currencyTypes[i], data.currencyAmounts[i]);
        }

        if (ScenarioManager.Instance != null)
            ScenarioManager.Instance.SetCompletedScenarios(new HashSet<string>(data.completedScenarios));

        if (QuestManager.Instance != null)
        {
            foreach (var qSave in data.activeQuests)
            {
                QuestData quest = QuestManager.Instance.allQuests.FirstOrDefault(q => q.questID == qSave.questID);
                if (quest == null) continue;

                QuestManager.Instance.StartQuest(quest);
                for (int i = 0; i < quest.objectives.Length && i < qSave.objectiveProgress.Count; i++)
                {
                    quest.objectives[i].currentProgress = qSave.objectiveProgress[i];
                    quest.objectives[i].isCompleted = qSave.objectiveCompleted[i];
                }
            }

            if (QuestTrackerUI.Instance != null)
            {
                foreach (var id in data.trackedQuests)
                    QuestTrackerUI.Instance.TrackQuest(id);
            }
        }

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public static SaveData LoadShopStockData()
    {
        if (!HasSaveFile()) return null;
        string json = File.ReadAllText(SavePath);
        return JsonUtility.FromJson<SaveData>(json);
    }
}
