using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(StatusEffectManager))]
public abstract class StatsBase : MonoBehaviour, IStatOwner
{
    [System.Serializable]
    public class Stat
    {
        public StatType type;
        public int baseValue;
        public int currentValue;
        public int minValue;
        public int maxValue;

        public Stat(StatType type, int baseValue, int min = 0, int max = 999)
        {
            this.type = type;
            this.baseValue = baseValue;
            this.currentValue = baseValue;
            this.minValue = min;
            this.maxValue = max;
        }
    }

    [Header("Stats")]
    public List<Stat> stats = new();

    protected readonly Dictionary<StatType, Stat> statDict = new();

    public event Action<StatType, int, int> OnStatChanged;
    public event Action OnDeath;

    protected StatusEffectManager statusEffectManager;

    protected virtual void Awake()
    {
        statusEffectManager = GetComponent<StatusEffectManager>();
        InitializeStats();
    }

    protected void InitializeStats()
    {
        statDict.Clear();

        foreach (var stat in stats)
        {
            if (stat.currentValue <= 0)
            {
                stat.currentValue = stat.baseValue;
            }

            statDict[stat.type] = stat;
        }
    }

    public int Get(StatType type)
    {
        int baseValue = statDict[type].currentValue;
        return ApplyEffectModifiers(type, baseValue);
    }

    public void Set(StatType type, int value, bool save = true)
    {
        if (!statDict.TryGetValue(type, out var stat))
        {
            Debug.LogWarning($"Stat type {type} not found!");
            return;
        }

        int oldValue = stat.currentValue;
        stat.currentValue = Mathf.Clamp(value, stat.minValue, stat.maxValue);

        if (oldValue != stat.currentValue)
        {
            OnStatChanged?.Invoke(type, oldValue, stat.currentValue);

            if (type == StatType.Health && stat.currentValue <= 0)
            {
                OnDeath?.Invoke();
                OnDie();
            }

            if (save)
                SaveStats();
        }
    }

    public virtual void Modify(StatType type, int amount, bool save = true)
    {
        if (!statDict.TryGetValue(type, out var stat))
            return;

        int oldValue = stat.currentValue;

        // Apply status effect damage modifiers
        if (amount < 0 && type == StatType.Health && statusEffectManager != null)
        {
            float reduction = statusEffectManager.GetDamageReduction();
            amount = Mathf.RoundToInt(amount * (1f - reduction));
        }

        int maxValue = GetMaxValue(type, stat);
        stat.currentValue = Mathf.Clamp(stat.currentValue + amount, stat.minValue, maxValue);

        if (oldValue == stat.currentValue)
            return;

        OnStatChanged?.Invoke(type, oldValue, stat.currentValue);

        if (type == StatType.Health && stat.currentValue <= 0)
        {
            OnDeath?.Invoke();
            OnDie();
        }

        if (save)
            SaveStats();
    }

    protected virtual int GetMaxValue(StatType type, Stat stat)
    {
        // For Health and Energy, use their max stat values
        if (type == StatType.Health)
            return Get(StatType.MaxHealth);
        else if (type == StatType.Energy)
            return Get(StatType.MaxEnergy);

        return stat.maxValue;
    }

    protected virtual int ApplyEffectModifiers(StatType type, int baseValue)
    {
        int value = baseValue;

        if (statusEffectManager == null)
            return value;

        foreach (var effect in statusEffectManager.activeEffects)
        {
            if (effect.data.statModifiers == null)
                continue;

            foreach (var mod in effect.data.statModifiers)
            {
                if (mod.statType != type)
                    continue;

                int amount = mod.amount * effect.currentStacks;

                value += mod.isPercentage
                    ? Mathf.RoundToInt(baseValue * amount / 100f)
                    : amount;
            }
        }

        return value;
    }

    protected virtual void SaveStats()
    {
        // Override in derived classes if needed
    }

    protected abstract void OnDie();

    public void ResetToBase(StatType type)
    {
        if (statDict.TryGetValue(type, out var stat))
        {
            Set(type, stat.baseValue);
        }
    }

    public void ResetAllToBase()
    {
        foreach (var stat in stats)
        {
            Set(stat.type, stat.baseValue, false);
        }

        SaveStats();
    }
}
