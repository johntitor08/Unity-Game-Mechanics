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
    public static bool IsLoading { get; private set; }

    public static bool HasSaveFile() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public static void SaveGame()
    {
        if (IsLoading)
            return;

        SaveData data = new();

        if (ProfileManager.Instance != null)
        {
            var p = ProfileManager.Instance.profile;
            data.playerName = p.playerName;
            data.playerLevel = p.level;
            data.playerExperience = p.experience;
            data.playerExperienceToNext = p.experienceToNextLevel;
        }

        data.currentScene = SceneManager.GetActiveScene().name;
        data.storyFlags.AddRange(StoryFlags.GetAll());

        if (SceneEvent.Instance != null)
            data.sceneProgress = (int)SceneEvent.Instance.Progress;

        if (TimePhaseManager.Instance != null)
        {
            data.currentTimePhase = TimePhaseManager.Instance.currentPhase;
            data.phaseProgress = TimePhaseManager.Instance.GetPhaseProgress();
        }

        if (TimeUI.Instance != null)
            data.currentDay = TimeUI.Instance.GetCurrentDay();

        if (InventoryManager.Instance != null)
        {
            data.inventoryKeys.Clear();
            data.inventoryCounts.Clear();

            foreach (var kv in InventoryManager.Instance.GetRawStock())
            {
                data.inventoryKeys.Add(kv.Key);
                data.inventoryCounts.Add(kv.Value);
            }
        }

        if (EquipmentManager.Instance != null)
        {
            data.equippedItems.Clear();

            foreach (var kvp in EquipmentManager.Instance.GetAllEquipped())
                data.equippedItems.Add(new EquippedItemSave
                {
                    slot = kvp.Key,
                    itemID = kvp.Value.baseData.itemID,
                    upgradeLevel = kvp.Value.upgradeLevel
                });
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

        if (ShopManager.Instance != null)
        {
            data.shopStockIDs.Clear(); data.shopStockAmounts.Clear();

            foreach (var s in ShopManager.Instance.GetStockDataForSave())
            {
                data.shopStockIDs.Add(s.itemID);
                data.shopStockAmounts.Add(s.amount);
            }
        }

        if (ScenarioManager.Instance != null)
        {
            data.completedScenarios.Clear();
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
                QuestSaveData q = new()
                {
                    questID = quest.questID
                };

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

        if (PlayerStats.Instance != null)
        {
            data.statTypes.Clear(); data.statValues.Clear();

            foreach (var s in PlayerStats.Instance.stats)
            {
                data.statTypes.Add(s.type);
                data.statValues.Add(s.currentValue);
            }
        }

        File.WriteAllText(SavePath, JsonUtility.ToJson(data, true));
    }

    public static void LoadGame()
    {
        if (!HasSaveFile())
            return;

        CachedData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        IsLoading = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(CachedData.currentScene);

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.StartCoroutine(UpdateUIAfterLoad());
    }

    static System.Collections.IEnumerator UpdateUIAfterLoad()
    {
        yield return null;

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (CachedData != null && SceneEvent.Instance != null)
            SceneEvent.Instance.StartCoroutine(DelayedApply(CachedData));

        CachedData = null;
    }

    static System.Collections.IEnumerator DelayedApply(SaveData data)
    {
        yield return null;
        ApplyLoadedData(data);
    }

    public static void ApplyLoadedData(SaveData data)
    {
        if (data == null)
            return;

        IsLoading = true;

        if (ProfileManager.Instance != null)
            ProfileManager.Instance.ApplyLoadedProfile(new PlayerProfile
            {
                playerName = data.playerName,
                level = data.playerLevel,
                experience = data.playerExperience,
                experienceToNextLevel = data.playerExperienceToNext,
                profileIconID = "default"
            });

        StoryFlags.Load(data.storyFlags);

        if (SceneEvent.Instance != null)
        {
            SceneEvent.Instance.UnsubscribeDialogue();
            SceneEvent.Instance.SubscribeDialogue();
            SceneEvent.Instance.ApplySceneProgress((SceneProgress)data.sceneProgress);
        }

        if (TimePhaseManager.Instance != null)
        {
            TimePhaseManager.Instance.SetPhase(data.currentTimePhase);
            TimePhaseManager.Instance.SetPhaseProgress(data.phaseProgress);
        }

        if (TimeUI.Instance != null)
            TimeUI.Instance.SetDay(data.currentDay);

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.Clear();

            if (data.inventoryKeys != null && data.inventoryKeys.Count > 0)
            {
                for (int i = 0; i < data.inventoryKeys.Count; i++)
                {
                    string key = data.inventoryKeys[i];
                    int sep = key.LastIndexOf(':');

                    if (sep < 0)
                        continue;

                    string id = key[..sep];

                    if (!int.TryParse(key[(sep + 1)..], out int lvl))
                        continue;

                    if (ItemDatabase.Instance == null)
                        return;

                    var itemData = ItemDatabase.Instance.GetByID(id);

                    if (itemData == null)
                        continue;

                    int qty = data.inventoryCounts[i];

                    if (itemData is EquipmentData eq)
                        InventoryManager.Instance.AddUpgradedItem(eq, lvl, qty);
                    else
                        InventoryManager.Instance.AddItem(itemData, qty);
                }
            }
        }

        if (EquipmentManager.Instance != null)
        {
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
                EquipmentManager.Instance.Unequip(slot, returnToInventory: false, save: false);

            foreach (var saved in data.equippedItems)
            {
                if (ItemDatabase.Instance == null)
                    return;

                var eq = ItemDatabase.Instance.GetByID(saved.itemID) as EquipmentData;

                if (eq != null)
                    EquipmentManager.Instance.Equip(new EquipmentInstance(eq, saved.upgradeLevel));
            }
        }

        if (CurrencyManager.Instance != null)
            for (int i = 0; i < data.currencyTypes.Count; i++)
                CurrencyManager.Instance.Set(data.currencyTypes[i], data.currencyAmounts[i]);

        if (ShopManager.Instance != null)
            ShopManager.Instance.ApplyLoadedStock(data.shopStockIDs, data.shopStockAmounts);

        if (ScenarioManager.Instance != null)
            ScenarioManager.Instance.SetCompletedScenarios(new HashSet<string>(data.completedScenarios));

        if (QuestManager.Instance != null)
        {
            foreach (var qSave in data.activeQuests)
            {
                var quest = QuestManager.Instance.allQuests.FirstOrDefault(q => q.questID == qSave.questID);

                if (quest == null)
                    continue;

                QuestManager.Instance.StartQuest(quest);

                for (int i = 0; i < quest.objectives.Length && i < qSave.objectiveProgress.Count; i++)
                {
                    quest.objectives[i].currentProgress = qSave.objectiveProgress[i];
                    quest.objectives[i].isCompleted = qSave.objectiveCompleted[i];
                }
            }

            if (QuestTrackerUI.Instance != null)
                foreach (var id in data.trackedQuests)
                    QuestTrackerUI.Instance.TrackQuest(id);
        }

        if (PlayerStats.Instance != null)
            for (int i = 0; i < data.statTypes.Count; i++)
                PlayerStats.Instance.Set(data.statTypes[i], data.statValues[i], true);

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();

        int sceneDialogueIndex = data.sceneProgress;

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartCoroutine(ClearAndStartDialogue(sceneDialogueIndex));
        else if (SceneEvent.Instance != null)
            SceneEvent.Instance.StartCoroutine(ClearAndStartDialogue(sceneDialogueIndex));
        else
            IsLoading = false;
    }

    static System.Collections.IEnumerator ClearAndStartDialogue(int sceneDialogueIndex)
    {
        IsLoading = false;
        yield return null;

        if (SceneEvent.Instance != null)
            yield return SceneEvent.Instance.StartCoroutine(SceneEvent.Instance.StartDialogueAfterLoad(sceneDialogueIndex));
    }
}
