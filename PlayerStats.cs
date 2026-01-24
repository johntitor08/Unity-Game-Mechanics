using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : StatsBase
{
    public static PlayerStats Instance;

    [Header("Regeneration")]
    public bool enableHealthRegen = true;
    public float healthRegenRate = 1f;
    public float healthRegenDelay = 5f;

    public bool enableEnergyRegen = true;
    public float energyRegenRate = 2f;
    public float energyRegenDelay = 2f;

    private float lastHealthDamageTime;
    private float lastEnergyUseTime;

    [Header("Combat")]
    public int baseDamage = 10;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Attack Status Effects")]
    public StatusEffectData burnEffect;
    public StatusEffectData poisonEffect;

    public event Action OnHealthChanged;
    public event Action OnEnergyChanged;

    protected override void Awake()
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

        if (stats == null || stats.Count == 0)
        {
            stats = new List<Stat>
            {
                new(StatType.Health, 100, 0, 999),
                new(StatType.MaxHealth, 100, 1, 999),
                new(StatType.Energy, 100, 0, 999),
                new(StatType.MaxEnergy, 100, 1, 999),
                new(StatType.Strength, 10, 0, 999),
                new(StatType.Intelligence, 10, 0, 999),
                new(StatType.Charisma, 10, 0, 999),
                new(StatType.Defense, 5, 0, 999),
                new(StatType.Speed, 10, 0, 999),
                new(StatType.Luck, 5, 0, 999)
            };
        }

        base.Awake();
        Set(StatType.Health, Get(StatType.MaxHealth), false);
        Set(StatType.Energy, Get(StatType.MaxEnergy), false);
    }

    void Update()
    {
        HandleRegeneration();
    }

    void HandleRegeneration()
    {
        if (statusEffectManager != null && !statusEffectManager.CanAct())
            return;

        if (enableHealthRegen && Time.time - lastHealthDamageTime >= healthRegenDelay)
        {
            int currentHealth = statDict[StatType.Health].currentValue;
            int maxHealth = Get(StatType.MaxHealth);

            if (currentHealth < maxHealth && currentHealth > 0)
            {
                float regenAmount = healthRegenRate * Time.deltaTime;
                Modify(StatType.Health, Mathf.CeilToInt(regenAmount), false);
            }
        }

        if (enableEnergyRegen && Time.time - lastEnergyUseTime >= energyRegenDelay)
        {
            int currentEnergy = statDict[StatType.Energy].currentValue;
            int maxEnergy = Get(StatType.MaxEnergy);

            if (currentEnergy < maxEnergy)
            {
                float regenAmount = energyRegenRate * Time.deltaTime;
                Modify(StatType.Energy, Mathf.CeilToInt(regenAmount), false);
            }
        }
    }

    public override void Modify(StatType type, int amount, bool save = true)
    {
        if (amount < 0)
        {
            if (type == StatType.Health)
                lastHealthDamageTime = Time.time;
            else if (type == StatType.Energy)
                lastEnergyUseTime = Time.time;
        }

        base.Modify(type, amount, save);

        if (type == StatType.Health)
            OnHealthChanged?.Invoke();
        else if (type == StatType.Energy)
            OnEnergyChanged?.Invoke();
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

    public void FullRestore()
    {
        Set(StatType.Health, Get(StatType.MaxHealth), false);
        Set(StatType.Energy, Get(StatType.MaxEnergy), false);
        SaveStats();
    }

    public float GetHealthPercentage()
    {
        int maxHealth = Get(StatType.MaxHealth);
        if (maxHealth <= 0) return 0f;
        return (float)statDict[StatType.Health].currentValue / maxHealth;
    }

    public float GetEnergyPercentage()
    {
        int maxEnergy = Get(StatType.MaxEnergy);
        if (maxEnergy <= 0) return 0f;
        return (float)statDict[StatType.Energy].currentValue / maxEnergy;
    }

    public bool HasEnoughEnergy(int amount)
    {
        if (!statDict.TryGetValue(StatType.Energy, out var energy))
            return false;

        return energy.currentValue >= amount;
    }

    public bool IsAlive()
    {
        return statDict[StatType.Health].currentValue > 0;
    }

    public Dictionary<StatType, int> GetAllStats()
    {
        Dictionary<StatType, int> result = new();

        foreach (var stat in stats)
        {
            result[stat.type] = Get(stat.type);
        }

        return result;
    }

    protected override void SaveStats()
    {
        SaveSystem.SaveGame();
    }

    protected override void OnDie()
    {
        Debug.Log("Player died!");
    }

    public void Attack(EnemyStats target)
    {
        if (target == null) return;
        if (Time.time - lastAttackTime < attackCooldown) return;
        int damage = baseDamage + Get(StatType.Strength);
        target.Modify(StatType.Health, -damage);
        float rand = UnityEngine.Random.value;

        if (target.TryGetComponent<StatusEffectManager>(out var effectManager))
        {
            if (burnEffect != null && rand < 0.2f)
                effectManager.ApplyEffect(burnEffect);
            else if (poisonEffect != null && rand < 0.3f)
                effectManager.ApplyEffect(poisonEffect);
        }

        lastAttackTime = Time.time;
    }

}
