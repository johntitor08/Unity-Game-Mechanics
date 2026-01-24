using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

    [System.Serializable]
    public class StatBuff
    {
        public enum StatTypeBuff { HealthRegen, EnergyRegen, Strength, Speed }
        public StatTypeBuff type;
        public float multiplier = 1f;
        public float duration = 5f;
        [HideInInspector] public float endTime;
    }

    private List<StatBuff> activeBuffs = new();
    private readonly Dictionary<StatBuff, BuffUI> activeBuffUI = new();

    private float healthRemainder = 0f;
    private float energyRemainder = 0f;

    [Header("Buff UI")]
    public Transform buffUIParent;
    public GameObject buffUIPrefab;

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
        HandleBuffs();
        HandleRegeneration();
    }

    public void AddBuff(StatBuff buff, Sprite icon = null, string buffName = "Buff")
    {
        buff.endTime = Time.time + buff.duration;
        activeBuffs.Add(buff);

        if (buffUIParent != null && buffUIPrefab != null && icon != null)
        {
            GameObject uiObj = Instantiate(buffUIPrefab, buffUIParent);
            BuffUI buffUI = uiObj.GetComponent<BuffUI>();
            buffUI.Setup(icon, buffName, buff.duration);

            if (!activeBuffUI.ContainsKey(buff))
                activeBuffUI[buff] = buffUI;
        }
    }

    private void HandleBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeBuffs[i];
            float timeLeft = buff.endTime - Time.time;

            if (activeBuffUI.TryGetValue(buff, out var buffUI))
                buffUI.UpdateTimer(timeLeft);

            if (timeLeft <= 0f)
            {
                if (buffUI != null)
                    Destroy(buffUI.gameObject);

                activeBuffUI.Remove(buff);
                activeBuffs.RemoveAt(i);
            }
        }
    }

    private float GetBuffedMultiplier(StatBuff.StatTypeBuff type)
    {
        float result = 1f;

        foreach (var buff in activeBuffs)
        {
            if (buff.type == type)
                result *= buff.multiplier;
        }

        return result;
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
                float regenAmount = healthRegenRate * GetBuffedMultiplier(StatBuff.StatTypeBuff.HealthRegen) * Time.deltaTime;
                healthRemainder += regenAmount;

                if (healthRemainder >= 1f)
                {
                    int intRegen = Mathf.FloorToInt(healthRemainder);
                    Modify(StatType.Health, intRegen, false);
                    healthRemainder -= intRegen;
                }
            }
        }

        if (enableEnergyRegen && Time.time - lastEnergyUseTime >= energyRegenDelay)
        {
            int currentEnergy = statDict[StatType.Energy].currentValue;
            int maxEnergy = Get(StatType.MaxEnergy);

            if (currentEnergy < maxEnergy)
            {
                float regenAmount = energyRegenRate * GetBuffedMultiplier(StatBuff.StatTypeBuff.EnergyRegen) * Time.deltaTime;
                energyRemainder += regenAmount;

                if (energyRemainder >= 1f)
                {
                    int intRegen = Mathf.FloorToInt(energyRemainder);
                    Modify(StatType.Energy, intRegen, false);
                    energyRemainder -= intRegen;
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

public class BuffUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI timerText;
    public GameObject tooltip;
    public TextMeshProUGUI tooltipText;
    private string buffName;
    private float duration;

    public void Setup(Sprite buffIcon, string name, float duration)
    {
        icon.sprite = buffIcon;
        buffName = name;
        this.duration = duration;
        timerText.text = duration.ToString("F1") + "s";

        if (tooltip != null)
        {
            tooltip.SetActive(false);
            tooltipText.text = $"{buffName}\nDuration: {duration:F1}s";
        }
    }

    public void UpdateTimer(float timeLeft)
    {
        timerText.text = Mathf.Max(0f, timeLeft).ToString("F1") + "s";
        
        if (tooltip != null)
        {
            tooltipText.text = $"{buffName}\nDuration: {Mathf.Max(0f, timeLeft):F1}s";
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetActive(false);
    }
}
