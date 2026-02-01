using UnityEngine;
using System;

[Serializable]
public class PlayerProfile
{
    public string playerName = "Player";
    public int level = 1;
    public int experience = 0;
    public int experienceToNextLevel = 100;
    public int currency = 100;
    public string profileIconID = "default";
}

public class ProfileManager : MonoBehaviour
{
    public static ProfileManager Instance;
    public PlayerProfile profile;
    public event Action<PlayerProfile> OnProfileChanged;
    public event Action<PlayerProfile> OnLevelUp;
    public event Action<PlayerProfile> OnCurrencyChanged;

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
        if (string.IsNullOrWhiteSpace(name)) return;
        profile.playerName = name;
        OnProfileChanged?.Invoke(profile);
        SaveSystem.SaveGame();
    }

    public void AddExperience(int amount)
    {
        if (amount <= 0) return;
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
        profile.currency += amount;
        OnCurrencyChanged?.Invoke(profile);
        OnProfileChanged?.Invoke(profile);
        SaveSystem.SaveGame();
    }

    public bool SpendCurrency(int amount)
    {
        if (profile.currency < amount) return false;
        profile.currency -= amount;
        OnCurrencyChanged?.Invoke(profile);
        OnProfileChanged?.Invoke(profile);
        SaveSystem.SaveGame();
        return true;
    }

    public void ApplyLoadedProfile(PlayerProfile saved)
    {
        if (saved == null) return;
        profile.playerName = saved.playerName;
        profile.level = saved.level;
        profile.experience = saved.experience;
        profile.experienceToNextLevel = saved.experienceToNextLevel;
        profile.currency = saved.currency;
        profile.profileIconID = saved.profileIconID;
        OnCurrencyChanged?.Invoke(profile);
        OnProfileChanged?.Invoke(profile);
    }
}
