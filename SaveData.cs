using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public List<string> itemIDs = new();
    public List<int> itemCounts = new();
    public string currentScene;
    public List<string> storyFlags = new();
    public List<StatType> statTypes = new();
    public List<int> statValues = new();
    public string playerName = "Player";
    public int playerLevel = 1;
    public int playerExperience = 0;
    public int playerExperienceToNext = 100;
    public int playerCurrency = 100;
    public List<string> shopStockIDs = new();
    public List<int> shopStockAmounts = new();
    public TimePhase currentTimePhase = TimePhase.Morning;
    public int currentDay = 1;
    public float phaseProgress = 0f;
    public List<EquippedItemSave> equippedItems = new();
    public List<CurrencyType> currencyTypes = new();
    public List<int> currencyAmounts = new();
    public List<string> completedScenarios = new();
    public string activeScenarioID = "";
    public int activeScenarioStep = 0;
    public List<QuestSaveData> activeQuests = new();
    public List<string> completedQuests = new();
    public List<string> trackedQuests = new();
    public int sceneProgress;
    public bool resumeDialogueOnLoad;
}

[Serializable]
public class EquippedItemSave
{
    public EquipmentSlot slot;
    public string itemID;
}

[Serializable]
public class QuestSaveData
{
    public string questID;
    public List<int> objectiveProgress = new();
    public List<bool> objectiveCompleted = new();
}
