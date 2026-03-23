using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class PlayerProfile
{
    public string playerName = "Player";
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    public string profileIconID = "default";
    public List<string> unlockedIconIDs = new() { "default" };
    public int statPoints = 0;
}

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;
    public PlayerProfile profile;
    public event Action<PlayerProfile> OnProfileChanged;
    public event Action<PlayerProfile> OnLevelUp;
    public event Action<PlayerProfile> OnCurrencyChanged;
    public static event Action OnReady;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        profile ??= new PlayerProfile();
        OnReady?.Invoke();
    }

    public void CreateNewProfile(string playerName = "Player")
    {
        profile = new PlayerProfile
        {
            playerName = playerName
        };

        OnProfileChanged?.Invoke(profile);
        SaveSystem.SaveGame();
    }

    public void SetPlayerName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return;

        profile.playerName = name;
        OnProfileChanged?.Invoke(profile);
        SaveSystem.SaveGame();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0)
            return;

        profile.experience += amount;
        bool leveledUp = false;

        while (profile.experience >= profile.experienceToNextLevel)
        {
            LevelUp();
            leveledUp = true;
        }

        OnProfileChanged?.Invoke(profile);

        if (amount > 0 || leveledUp)
            SaveSystem.SaveGame();
    }

    void LevelUp()
    {
        profile.experience -= profile.experienceToNextLevel;
        profile.level++;
        profile.experienceToNextLevel = Mathf.RoundToInt(profile.experienceToNextLevel * 1.5f);
        profile.statPoints += 3;

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.Modify(StatType.Health, 10);
            PlayerStats.Instance.Modify(StatType.Energy, 5);
            PlayerStats.Instance.Modify(StatType.Strength, 2);
            PlayerStats.Instance.Modify(StatType.Intelligence, 2);
        }

        OnLevelUp?.Invoke(profile);
    }

    public void AddCurrency(int amount)
    {
        CurrencyManager.Instance.Add(CurrencyType.Gold, amount);
        OnProfileChanged?.Invoke(profile);
    }

    public bool SpendCurrency(int amount)
    {
        bool success = CurrencyManager.Instance.Spend(CurrencyType.Gold, amount);

        if (success)
            OnProfileChanged?.Invoke(profile);

        return success;
    }

    public void ApplyLoadedProfile(PlayerProfile saved)
    {
        profile.playerName = saved.playerName;
        profile.level = saved.level;
        profile.experience = saved.experience;
        profile.experienceToNextLevel = saved.experienceToNextLevel;
        profile.profileIconID = saved.profileIconID;
        profile.statPoints = saved.statPoints;
        profile.unlockedIconIDs = saved.unlockedIconIDs;
        OnCurrencyChanged?.Invoke(profile);
        OnProfileChanged?.Invoke(profile);
    }

    public bool PurchaseIcon(string iconID, int cost)
    {
        if (profile.unlockedIconIDs.Contains(iconID))
            return false;

        if (!CurrencyManager.Instance.Spend(CurrencyType.Gold, cost))
            return false;

        profile.unlockedIconIDs.Add(iconID);
        OnProfileChanged?.Invoke(profile);
        SaveSystem.SaveGame();
        return true;
    }

    public void UnlockIcon(string iconID)
    {
        if (profile.unlockedIconIDs.Contains(iconID))
            return;

        profile.unlockedIconIDs.Add(iconID);
        OnProfileChanged?.Invoke(profile);
        SaveSystem.SaveGame();
    }

    public bool SelectIcon(string iconID)
    {
        if (!profile.unlockedIconIDs.Contains(iconID))
            return false;

        profile.profileIconID = iconID;
        OnProfileChanged?.Invoke(profile);
        SaveSystem.SaveGame();
        return true;
    }

    public bool IsIconUnlocked(string iconID) => profile.unlockedIconIDs.Contains(iconID);
}
