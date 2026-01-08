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

    public PlayerProfile profile = new();

    public event Action OnProfileChanged;
    public event Action OnLevelUp;
    public event Action OnCurrencyChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public void AddExperience(int amount)
    {
        profile.experience += amount;

        while (profile.experience >= profile.experienceToNextLevel)
        {
            LevelUp();
        }

        OnProfileChanged?.Invoke();
        SaveSystem.SaveGame();
    }

    void LevelUp()
    {
        profile.experience -= profile.experienceToNextLevel;
        profile.level++;
        profile.experienceToNextLevel = Mathf.RoundToInt(profile.experienceToNextLevel * 1.5f);

        // Increase all stats on level up
        PlayerStats.Instance.Modify(StatType.Health, 10);
        PlayerStats.Instance.Modify(StatType.Energy, 5);
        PlayerStats.Instance.Modify(StatType.Strength, 2);
        PlayerStats.Instance.Modify(StatType.Intelligence, 2);

        OnLevelUp?.Invoke();
        OnProfileChanged?.Invoke();
    }

    public bool SpendCurrency(int amount)
    {
        if (profile.currency < amount) return false;

        profile.currency -= amount;
        OnCurrencyChanged?.Invoke();
        OnProfileChanged?.Invoke();
        SaveSystem.SaveGame();
        return true;
    }

    public void AddCurrency(int amount)
    {
        profile.currency += amount;
        OnCurrencyChanged?.Invoke();
        OnProfileChanged?.Invoke();
        SaveSystem.SaveGame();
    }

    public void SetPlayerName(string name)
    {
        profile.playerName = name;
        OnProfileChanged?.Invoke();
        SaveSystem.SaveGame();
    }
}
