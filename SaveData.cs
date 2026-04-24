using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public string playerName = "Player";
    public int playerLevel = 1;
    public int playerExperience = 0;
    public int playerExperienceToNext = 100;
    public string currentScene;
    public int sceneProgress;
    public List<string> storyFlags = new();
    public TimePhase currentTimePhase = TimePhase.Morning;
    public int currentDay = 1;
    public float phaseProgress = 0f;
    public bool resumeDialogueOnLoad;
    public List<string> inventoryKeys = new();
    public List<int> inventoryCounts = new();
    public List<EquippedItemSave> equippedItems = new();
    public List<CurrencyType> currencyTypes = new();
    public List<int> currencyAmounts = new();
    public List<string> shopStockIDs = new();
    public List<int> shopStockAmounts = new();
    public List<StatType> statTypes = new();
    public List<int> statValues = new();
    public List<string> activeQuestIDs = new();
    public List<QuestRuntimeState> activeQuests = new();
    public List<string> completedQuests = new();
    public List<string> trackedQuests = new();
    public string activeScenarioID = "";
    public int activeScenarioStep = 0;
    public List<string> completedScenarios = new();
}

[Serializable]
public class EquippedItemSave
{
    public EquipmentSlot slot;
    public string itemID;
    public int upgradeLevel;
}
