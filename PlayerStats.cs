using UnityEngine;
using System.Collections.Generic;
using System;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats Instance;

    [System.Serializable]
    public class Stat
    {
        public StatType type;
        public int baseValue = 10;
        public int currentValue;
        public int minValue = 0;
        public int maxValue = 9999;

        public Stat(StatType type, int baseValue, int minValue = 0, int maxValue = 9999)
        {
            this.type = type;
            this.baseValue = baseValue;
            this.currentValue = baseValue;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
    }

    [Header("Initial Stats")]
    public List<Stat> stats = new List<Stat>
    {
        new Stat(StatType.Health, 100, 0, 999),
        new Stat(StatType.MaxHealth, 100, 1, 999),
        new Stat(StatType.Energy, 100, 0, 999),
        new Stat(StatType.MaxEnergy, 100, 1, 999),
        new Stat(StatType.Strength, 10, 0, 999),
        new Stat(StatType.Intelligence, 10, 0, 999),
        new Stat(StatType.Charisma, 10, 0, 999),
        new Stat(StatType.Defense, 5, 0, 999),
        new Stat(StatType.Speed, 10, 0, 999),
        new Stat(StatType.Luck, 5, 0, 999)
    };

    [Header("Regeneration")]
    public bool enableHealthRegen = true;
    public float healthRegenRate = 1f; // HP per second
    public float healthRegenDelay = 5f; // Delay after taking damage

    public bool enableEnergyRegen = true;
    public float energyRegenRate = 2f; // Energy per second
    public float energyRegenDelay = 2f; // Delay after using energy

    private Dictionary<StatType, Stat> statDict = new Dictionary<StatType, Stat>();
    private float lastHealthDamageTime;
    private float lastEnergyUseTime;

    public event Action<StatType, int, int> OnStatChanged; // (statType, oldValue, newValue)
    public event Action OnHealthChanged;
    public event Action OnEnergyChanged;
    public event Action OnDeath;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        InitializeStats();
    }

    void Start()
    {
        // Set current health and energy to max
        Set(StatType.Health, Get(StatType.MaxHealth));
        Set(StatType.Energy, Get(StatType.MaxEnergy));
    }

    void Update()
    {
        HandleRegeneration();
    }

    void InitializeStats()
    {
        statDict.Clear();

        foreach (var stat in stats)
        {
            stat.currentValue = stat.baseValue;
            statDict[stat.type] = stat;
        }
    }

    void HandleRegeneration()
    {
        // Health regeneration
        if (enableHealthRegen)
        {
            if (Time.time - lastHealthDamageTime >= healthRegenDelay)
            {
                int currentHealth = Get(StatType.Health);
                int maxHealth = Get(StatType.MaxHealth);

                if (currentHealth < maxHealth)
                {
                    float regenAmount = healthRegenRate * Time.deltaTime;
                    Modify(StatType.Health, Mathf.CeilToInt(regenAmount), false); // Don't save every frame
                }
            }
        }

        // Energy regeneration
        if (enableEnergyRegen)
        {
            if (Time.time - lastEnergyUseTime >= energyRegenDelay)
            {
                int currentEnergy = Get(StatType.Energy);
                int maxEnergy = Get(StatType.MaxEnergy);

                if (currentEnergy < maxEnergy)
                {
                    float regenAmount = energyRegenRate * Time.deltaTime;
                    Modify(StatType.Energy, Mathf.CeilToInt(regenAmount), false); // Don't save every frame
                }
            }
        }
    }

    public int Get(StatType type)
    {
        if (!statDict.ContainsKey(type))
        {
            Debug.LogWarning($"Stat type {type} not found! Returning 0.");
            return 0;
        }

        return statDict[type].currentValue;
    }

    public void Set(StatType type, int value, bool save = true)
    {
        if (!statDict.ContainsKey(type))
        {
            Debug.LogWarning($"Stat type {type} not found!");
            return;
        }

        Stat stat = statDict[type];
        int oldValue = stat.currentValue;
        stat.currentValue = Mathf.Clamp(value, stat.minValue, stat.maxValue);

        if (oldValue != stat.currentValue)
        {
            OnStatChanged?.Invoke(type, oldValue, stat.currentValue);

            // Trigger specific events
            if (type == StatType.Health)
            {
                OnHealthChanged?.Invoke();

                if (stat.currentValue <= 0)
                {
                    OnDeath?.Invoke();
                }
            }
            else if (type == StatType.Energy)
            {
                OnEnergyChanged?.Invoke();
            }

            if (save)
                SaveSystem.SaveGame();
        }
    }

    public void Modify(StatType type, int amount, bool save = true)
    {
        if (!statDict.ContainsKey(type))
        {
            Debug.LogWarning($"Stat type {type} not found!");
            return;
        }

        Stat stat = statDict[type];
        int oldValue = stat.currentValue;
        stat.currentValue = Mathf.Clamp(stat.currentValue + amount, stat.minValue, stat.maxValue);

        // Track damage/use times for regeneration
        if (amount < 0)
        {
            if (type == StatType.Health)
                lastHealthDamageTime = Time.time;
            else if (type == StatType.Energy)
                lastEnergyUseTime = Time.time;
        }

        if (oldValue != stat.currentValue)
        {
            OnStatChanged?.Invoke(type, oldValue, stat.currentValue);

            // Trigger specific events
            if (type == StatType.Health)
            {
                OnHealthChanged?.Invoke();

                if (stat.currentValue <= 0)
                {
                    OnDeath?.Invoke();
                }
            }
            else if (type == StatType.Energy)
            {
                OnEnergyChanged?.Invoke();
            }

            if (save)
                SaveSystem.SaveGame();
        }
    }

    public void ModifyMax(StatType type, int amount)
    {
        StatType maxType = type switch
        {
            StatType.Health => StatType.MaxHealth,
            StatType.Energy => StatType.MaxEnergy,
            _ => type
        };

        Modify(maxType, amount);
    }

    public void ResetToBase(StatType type)
    {
        if (!statDict.ContainsKey(type))
        {
            Debug.LogWarning($"Stat type {type} not found!");
            return;
        }

        Set(type, statDict[type].baseValue);
    }

    public void ResetAllToBase()
    {
        foreach (var stat in stats)
        {
            Set(stat.type, stat.baseValue, false);
        }
        SaveSystem.SaveGame();
    }

    public void FullRestore()
    {
        Set(StatType.Health, Get(StatType.MaxHealth), false);
        Set(StatType.Energy, Get(StatType.MaxEnergy), false);
        SaveSystem.SaveGame();
    }

    public float GetHealthPercentage()
    {
        int maxHealth = Get(StatType.MaxHealth);
        if (maxHealth <= 0) return 0f;
        return (float)Get(StatType.Health) / maxHealth;
    }

    public float GetEnergyPercentage()
    {
        int maxEnergy = Get(StatType.MaxEnergy);
        if (maxEnergy <= 0) return 0f;
        return (float)Get(StatType.Energy) / maxEnergy;
    }

    public bool HasEnoughEnergy(int amount)
    {
        return Get(StatType.Energy) >= amount;
    }

    public bool IsAlive()
    {
        return Get(StatType.Health) > 0;
    }

    public Dictionary<StatType, int> GetAllStats()
    {
        Dictionary<StatType, int> result = new Dictionary<StatType, int>();
        foreach (var stat in stats)
        {
            result[stat.type] = stat.currentValue;
        }
        return result;
    }
}
