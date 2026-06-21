using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SaveSystem
{
    public static SaveData CachedData;
    public static bool IsLoading { get; private set; }
    public static bool SavingEnabled = false;
    private static int activeSlot = 0;
    private static string SavePath => GetSavePath(activeSlot);

    private static string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slotIndex}.json");
    }

    public static void SetActiveSlot(int slotIndex)
    {
        activeSlot = Mathf.Max(0, slotIndex);
    }

    public static bool HasSaveFile(int slotIndex)
    {
        return File.Exists(GetSavePath(slotIndex));
    }

    public static SaveData PeekSlot(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (!File.Exists(path))
            return null;

        try
        {
            return JsonUtility.FromJson<SaveData>(File.ReadAllText(path));
        }
        catch
        {
            return null;
        }
    }

    public static void DeleteSave(int slotIndex)
    {
        string path = GetSavePath(slotIndex);

        if (File.Exists(path))
            File.Delete(path);
    }

    public static void SaveGame(int slotIndex)
    {
        SetActiveSlot(slotIndex);
        SaveGame(isManual: true);
    }

    public static void LoadGame(int slotIndex)
    {
        SetActiveSlot(slotIndex);
        LoadGame();
    }

    public static bool HasSaveFile() => File.Exists(SavePath);

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public static void SaveGame(bool isManual = false)
    {
        if (!SavingEnabled || IsLoading || (!isManual && SettingsManager.Instance != null && !SettingsManager.Instance.IsAutosaveEnabled()))
            return;

        SaveData data = new()
        {
            savedAt = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm")
        };

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

        if (OriginManager.Instance != null)
            data.originID = OriginManager.Instance.GetSaveID();

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
            data.shopStockIDs.Clear();
            data.shopStockAmounts.Clear();

            foreach (var s in ShopManager.Instance.GetStockDataForSave())
            {
                data.shopStockIDs.Add(s.itemID);
                data.shopStockAmounts.Add(s.amount);
            }
        }

        if (ScenarioManager.Instance != null)
        {
            var scenarioSave = ScenarioManager.Instance.GetSaveData();
            data.completedScenarios = scenarioSave.completedScenarioIDs;

            if (ScenarioManager.Instance.IsScenarioActive())
            {
                data.activeScenarioID = ScenarioManager.Instance.GetCurrentScenario().scenarioID;
                data.activeScenarioStep = ScenarioManager.Instance.GetCurrentStepIndex();
            }
            else
            {
                data.activeScenarioID = "";
                data.activeScenarioStep = 0;
            }
        }

        if (QuestManager.Instance != null)
        {
            var questSave = QuestManager.Instance.GetSaveData();
            data.activeQuestIDs = questSave.activeQuestIDs;
            data.activeQuests = questSave.runtimeStates;
            data.completedQuests = questSave.completedQuestIDs;
            data.trackedQuests = questSave.trackedQuestIDs;
            data.questTimerKeys = questSave.questTimerKeys;
            data.questTimerValues = questSave.questTimerValues;
            data.selectedOptionalRewardQuestIDs = questSave.selectedOptionalRewardQuestIDs;
            data.selectedOptionalRewardItemIDs = questSave.selectedOptionalRewardItemIDs;
        }

        if (PlayerStats.Instance != null)
        {
            data.statTypes.Clear();
            data.statValues.Clear();

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

        SavingEnabled = true;
        CachedData = JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
        IsLoading = true;
        SceneManager.sceneLoaded += OnSceneLoaded;
        SceneManager.LoadScene(CachedData.currentScene);
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

        StoryFlags.Reset();
        StoryFlags.Load(data.storyFlags);

        if (OriginManager.Instance != null && !string.IsNullOrEmpty(data.originID))
            OriginManager.Instance.LoadFromSaveID(data.originID);

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
                if (ItemDatabase.Instance == null)
                {
                    Debug.LogWarning("[SaveSystem] ItemDatabase.Instance null, inventory yüklenemedi.");
                }
                else
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
        }

        if (EquipmentManager.Instance != null)
        {
            foreach (EquipmentSlot slot in System.Enum.GetValues(typeof(EquipmentSlot)))
                EquipmentManager.Instance.Unequip(slot, returnToInventory: false, save: false);

            if (ItemDatabase.Instance == null)
            {
                Debug.LogWarning("[SaveSystem] ItemDatabase.Instance null, equipment yüklenemedi.");
            }
            else
            {
                foreach (var saved in data.equippedItems)
                {
                    var eq = ItemDatabase.Instance.GetByID(saved.itemID) as EquipmentData;

                    if (eq != null)
                        EquipmentManager.Instance.Equip(new EquipmentInstance(eq, saved.upgradeLevel));
                }
            }
        }

        if (CurrencyManager.Instance != null)
            for (int i = 0; i < data.currencyTypes.Count; i++)
                CurrencyManager.Instance.Set(data.currencyTypes[i], data.currencyAmounts[i]);

        if (ShopManager.Instance != null)
            ShopManager.Instance.ApplyLoadedStock(data.shopStockIDs, data.shopStockAmounts);

        if (ScenarioManager.Instance != null)
        {
            ScenarioManager.Instance.LoadSaveData(new ScenarioSaveData
            {
                completedScenarioIDs = data.completedScenarios
            });
        }

        if (QuestManager.Instance != null)
        {
            var activeQuestIDs = data.activeQuestIDs ?? new();

            if (activeQuestIDs.Count == 0 && data.activeQuests != null && data.activeQuests.Count > 0)
            {
                var completedSet = new System.Collections.Generic.HashSet<string>(data.completedQuests ?? new());

                foreach (var state in data.activeQuests)
                    if (state != null && !string.IsNullOrEmpty(state.questID) && !completedSet.Contains(state.questID))
                        activeQuestIDs.Add(state.questID);
            }

            var questSave = new QuestSaveData
            {
                activeQuestIDs = activeQuestIDs,
                runtimeStates = data.activeQuests,
                completedQuestIDs = data.completedQuests,
                trackedQuestIDs = data.trackedQuests,
                questTimerKeys = data.questTimerKeys ?? new(),
                questTimerValues = data.questTimerValues ?? new(),
                selectedOptionalRewardQuestIDs = data.selectedOptionalRewardQuestIDs ?? new(),
                selectedOptionalRewardItemIDs = data.selectedOptionalRewardItemIDs ?? new()
            };

            if (questSave.runtimeStates != null)
                foreach (var state in questSave.runtimeStates)
                    state.RebuildLookup();

            QuestManager.Instance.LoadSaveData(questSave);
        }

        if (PlayerStats.Instance != null)
            for (int i = 0; i < data.statTypes.Count; i++)
                PlayerStats.Instance.Set(data.statTypes[i], data.statValues[i], true);

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();

        bool hasActiveScenario = ScenarioManager.Instance != null && !string.IsNullOrEmpty(data.activeScenarioID) && ScenarioManager.Instance.GetScenarioByID(data.activeScenarioID) != null;

        if (hasActiveScenario)
        {
            IsLoading = false;
            ScenarioManager.Instance.StartCoroutine(ResumeScenarioAfterLoad(data.activeScenarioID, data.activeScenarioStep));
            return;
        }

        int sceneDialogueIndex = data.sceneProgress;

        try
        {
            if (DialogueManager.Instance != null)
                DialogueManager.Instance.StartCoroutine(ClearAndStartDialogue(sceneDialogueIndex));
            else if (SceneEvent.Instance != null)
                SceneEvent.Instance.StartCoroutine(ClearAndStartDialogue(sceneDialogueIndex));
            else
                IsLoading = false;
        }
        catch
        {
            IsLoading = false;
            throw;
        }
    }

    static System.Collections.IEnumerator ClearAndStartDialogue(int sceneDialogueIndex)
    {
        IsLoading = false;
        yield return null;

        if (SceneEvent.Instance != null)
            yield return SceneEvent.Instance.StartCoroutine(SceneEvent.Instance.StartDialogueAfterLoad(sceneDialogueIndex));
    }

    static System.Collections.IEnumerator ResumeScenarioAfterLoad(string scenarioID, int stepIndex)
    {
        yield return null;

        if (ScenarioManager.Instance != null)
            ScenarioManager.Instance.ResumeScenario(scenarioID, stepIndex);
    }
}
