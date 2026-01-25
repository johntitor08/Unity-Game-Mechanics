using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : StatsBase
{
    public static PlayerStats Instance;
    public event Action OnHealthChanged;
    public event Action OnEnergyChanged;
    public static event Action OnReady;
    private float healthRemainder;
    private float energyRemainder;
    private float lastHealthDamageTime;
    private float lastEnergyUseTime;

    [Header("Regeneration")]
    public bool enableHealthRegen = true;
    public float healthRegenRate = 1f;
    public float healthRegenDelay = 5f;
    public bool enableEnergyRegen = true;
    public float energyRegenRate = 2f;
    public float energyRegenDelay = 2f;

    [Header("Combat")]
    public int baseDamage = 10;
    public float attackCooldown = 1f;
    private float lastAttackTime;

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
        OnReady?.Invoke();
    }

    void Update()
    {
        HandleRegeneration();
    }

    void HandleRegeneration()
    {
        if (enableHealthRegen && Time.time - lastHealthDamageTime >= healthRegenDelay)
        {
            int hp = Get(StatType.Health);
            int maxHp = Get(StatType.MaxHealth);

            if (hp > 0 && hp < maxHp)
            {
                healthRemainder += healthRegenRate * Time.deltaTime;

                if (healthRemainder >= 1f)
                {
                    int regen = Mathf.FloorToInt(healthRemainder);
                    Modify(StatType.Health, regen, false);
                    healthRemainder -= regen;
                }
            }
        }

        if (enableEnergyRegen && Time.time - lastEnergyUseTime >= energyRegenDelay)
        {
            int energy = Get(StatType.Energy);
            int maxEnergy = Get(StatType.MaxEnergy);

            if (energy < maxEnergy)
            {
                energyRemainder += energyRegenRate * Time.deltaTime;

                if (energyRemainder >= 1f)
                {
                    int regen = Mathf.FloorToInt(energyRemainder);
                    Modify(StatType.Energy, regen, false);
                    energyRemainder -= regen;
                }
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
        lastAttackTime = Time.time;
    }

    public bool HasEnoughEnergy(int amount)
    {
        return Get(StatType.Energy) >= amount;
    }

    public void FullRestore()
    {
        Set(StatType.Health, Get(StatType.MaxHealth), false);
        Set(StatType.Energy, Get(StatType.MaxEnergy), false);
        OnHealthChanged?.Invoke();
        OnEnergyChanged?.Invoke();
        SaveStats();
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

    public bool IsAlive() => Get(StatType.Health) > 0;

    public float GetHealthPercentage() =>
        Get(StatType.MaxHealth) <= 0 ? 0f :
        (float)Get(StatType.Health) / Get(StatType.MaxHealth);

    public float GetEnergyPercentage() =>
        Get(StatType.MaxEnergy) <= 0 ? 0f :
        (float)Get(StatType.Energy) / Get(StatType.MaxEnergy);

    protected override void SaveStats()
    {
        SaveSystem.SaveGame();
    }
}
