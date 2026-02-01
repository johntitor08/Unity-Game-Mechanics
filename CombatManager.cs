using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance { get; private set; }
    public event Action<string> OnCombatLog;
    public event Action OnCombatStarted;
    public event Action OnCombatEnded;
    public event Action OnCombatStateChanged;
    public event Action<bool> OnTurnChanged;

    [Header("Combat State")]
    public bool inCombat;
    public EnemyData currentEnemy;
    public EnemyStats enemyStats;
    private bool combatResolved;
    private bool isPlayerTurn;
    private bool waitingForInput;
    private int playerDefenseBonus;
    private int enemyDefenseBonus;

    [Header("Actions")]
    public List<CombatAction> defaultActions = new();
    private readonly List<CombatAction> unlockedActions = new();

    [Header("Turn Settings")]
    [Tooltip("Delay between player action and enemy turn")]
    public float turnDelay = 1f;
    [Tooltip("Delay before enemy executes their action")]
    public float enemyActionDelay = 1.5f;

    [Header("Combat Balancing")]
    [Range(0f, 1f)]
    [Tooltip("Base critical hit chance (before luck modifiers)")]
    public float baseCritChance = 0.1f;
    [Tooltip("Damage multiplier for critical hits")]
    public float critDamageMultiplier = 1.5f;
    [Tooltip("Maximum defense bonus cap")]
    public int maxDefenseBonus = 50;
    [Tooltip("Defense decay per turn")]
    public int defenseDecayPerTurn = 2;

    [Header("Status Effects")]
    public StatusEffectData burnEffect;
    public StatusEffectData poisonEffect;
    [Range(0f, 1f)]
    public float burnApplyChance = 0.2f;
    [Range(0f, 1f)]
    public float poisonApplyChance = 0.3f;

    [Header("Buff UI")]
    public Transform buffUIParent;
    public GameObject buffUIPrefab;
    private readonly List<CombatBuff> activeCombatBuffs = new();

    [Serializable]
    public class CombatBuff
    {
        public string name;
        public Sprite icon;
        public float duration;
        [HideInInspector] public float endTime;
        [HideInInspector] public BuffUI ui;
    }

    private PlayerBuffManager PlayerBuffs => PlayerBuffManager.Instance;
    private PlayerStats PlayerStats => PlayerStats.Instance;
    private EquipmentManager Equipment => EquipmentManager.Instance;

    void Awake()
    {
        InitializeSingleton();
    }

    void Start()
    {
        InitializeActions();
        SubscribeToPlayerEvents();
    }

    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    private void InitializeSingleton()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializeActions()
    {
        unlockedActions.Clear();
        unlockedActions.AddRange(defaultActions);
    }

    private void SubscribeToPlayerEvents()
    {
        if (PlayerStats != null)
        {
            PlayerStats.OnHealthChanged += OnPlayerHealthChanged;
        }
    }

    private void UnsubscribeFromEvents()
    {
        if (PlayerStats != null)
        {
            PlayerStats.OnHealthChanged -= OnPlayerHealthChanged;
        }

        if (enemyStats != null)
        {
            enemyStats.OnStatChanged -= OnEnemyStatChanged;
        }
    }

    void Update()
    {
        if (!inCombat) return;
        UpdateBuffTimers();
    }

    private void UpdateBuffTimers()
    {
        for (int i = activeCombatBuffs.Count - 1; i >= 0; i--)
        {
            CombatBuff buff = activeCombatBuffs[i];
            float timeLeft = buff.endTime - Time.time;

            if (buff.ui != null)
            {
                buff.ui.UpdateTimer(timeLeft);
            }

            if (timeLeft <= 0f)
            {
                RemoveBuff(i);
            }
        }
    }

    private void RemoveBuff(int index)
    {
        if (activeCombatBuffs[index].ui != null)
        {
            Destroy(activeCombatBuffs[index].ui.gameObject);
        }

        activeCombatBuffs.RemoveAt(index);
    }

    public void StartCombat(EnemyData enemy)
    {
        if (inCombat || enemy == null)
        {
            Debug.LogWarning("Cannot start combat: already in combat or enemy is null");
            return;
        }

        currentEnemy = enemy;
        InitializeEnemyStats();
        ResetCombatState();
        inCombat = true;
        NotifyCombatStarted();
    }

    private void InitializeEnemyStats()
    {
        if (enemyStats != null)
        {
            enemyStats.InitializeFromData(currentEnemy);
            enemyStats.OnStatChanged -= OnEnemyStatChanged;
            enemyStats.OnStatChanged += OnEnemyStatChanged;
        }
    }

    private void ResetCombatState()
    {
        combatResolved = false;
        isPlayerTurn = true;
        waitingForInput = true;
        playerDefenseBonus = 0;
        enemyDefenseBonus = 0;
    }

    private void NotifyCombatStarted()
    {
        OnCombatStarted?.Invoke();
        OnTurnChanged?.Invoke(true);
        OnCombatStateChanged?.Invoke();
    }

    public void ExecutePlayerAction(CombatAction action)
    {
        if (!CanExecutePlayerAction(action)) return;
        ConsumePlayerEnergy(action.energyCost);
        waitingForInput = false;

        if (action.isDefensive)
        {
            HandleDefensiveAction(action);
        }
        else
        {
            HandleOffensiveAction(action);
        }

        OnCombatStateChanged?.Invoke();
        Invoke(nameof(StartEnemyTurn), turnDelay);
    }

    private bool CanExecutePlayerAction(CombatAction action)
    {
        if (!inCombat || !IsPlayerTurn())
        {
            return false;
        }

        if (!PlayerStats.HasEnoughEnergy(action.energyCost))
        {
            Log("Not enough energy!");
            return false;
        }

        return true;
    }

    private void ConsumePlayerEnergy(int cost)
    {
        PlayerStats.Modify(StatType.Energy, -cost);
    }

    void StartEnemyTurn()
    {
        if (!inCombat) return;
        isPlayerTurn = false;
        OnTurnChanged?.Invoke(false);
        Invoke(nameof(ExecuteEnemyAction), enemyActionDelay);
    }

    void StartPlayerTurn()
    {
        isPlayerTurn = true;
        waitingForInput = true;
        OnTurnChanged?.Invoke(true);
    }

    private void HandleDefensiveAction(CombatAction action)
    {
        ApplyDefenseBonus(action.defenseBonus);
        ApplyHealing(action.healAmount);
    }

    private void ApplyDefenseBonus(int bonus)
    {
        playerDefenseBonus = Mathf.Clamp(
            playerDefenseBonus + bonus,
            0,
            maxDefenseBonus
        );
    }

    private void ApplyHealing(int amount)
    {
        if (amount > 0)
        {
            PlayerStats.Modify(StatType.Health, amount);
        }
    }

    private void HandleOffensiveAction(CombatAction action)
    {
        if (enemyStats == null) return;
        int damage = CalculatePlayerDamage(action);
        ApplyDamageToEnemy(damage, action);

        if (IsEnemyDefeated())
        {
            StartCoroutine(HandleVictoryAfterDelay(turnDelay));
        }
    }

    private int CalculatePlayerDamage(CombatAction action)
    {
        int damage = action.CalculateDamage();

        if (CheckCriticalHit(action))
        {
            damage = ApplyCriticalMultiplier(damage);
            Log("Critical Hit!");
        }

        damage = ApplyPlayerDamageModifiers(damage, action);
        return damage;
    }

    private int ApplyCriticalMultiplier(int damage)
    {
        return Mathf.RoundToInt(damage * critDamageMultiplier);
    }

    private int ApplyPlayerDamageModifiers(int damage, CombatAction action)
    {
        float damageMultiplier = 1f;

        if (PlayerBuffs != null)
            damageMultiplier = PlayerBuffs.GetDamageMultiplier();

        damage = CalculateFinalDamage(
            damage,
            GetEnemyTotalDefense(),
            action.ignoreDefense,
            action.armorPenetration,
            damageMultiplier
        );

        damage = ApplyEnemyDamageReduction(damage);
        return damage;
    }

    private int ApplyEnemyDamageReduction(int damage)
    {
        if (enemyStats.TryGetComponent<StatusEffectManager>(out var effects))
        {
            float reduction = effects.GetDamageReduction();
            damage = Mathf.RoundToInt(damage * (1f - reduction));
        }

        return damage;
    }

    private void ApplyDamageToEnemy(int damage, CombatAction action)
    {
        enemyStats.Modify(StatType.Health, -damage);
        TryApplyOnHitEffects(enemyStats);
        Log($"Dealt {damage} damage to {currentEnemy.enemyName}.");
    }

    private bool IsEnemyDefeated()
    {
        return enemyStats.Get(StatType.Health) <= 0;
    }

    private void TryApplyOnHitEffects(EnemyStats target)
    {
        if (!target.TryGetComponent<StatusEffectManager>(out var effects))
        {
            return;
        }

        float roll = UnityEngine.Random.value;

        if (burnEffect != null && roll < burnApplyChance)
        {
            effects.ApplyEffect(burnEffect);
        }
        else if (poisonEffect != null && roll < poisonApplyChance)
        {
            effects.ApplyEffect(poisonEffect);
        }
    }

    private bool CheckCriticalHit(CombatAction action)
    {
        if (action.guaranteedCrit) return true;
        float critChance = CalculateCritChance(action);
        return UnityEngine.Random.value <= critChance;
    }

    private float CalculateCritChance(CombatAction action)
    {
        float chance = baseCritChance + action.critChanceBonus;
        chance += GetLuckCritBonus();
        return Mathf.Clamp01(chance);
    }

    private float GetLuckCritBonus()
    {
        return PlayerStats.Get(StatType.Luck) * 0.01f;
    }

    void ExecuteEnemyAction()
    {
        if (!CanExecuteEnemyAction()) return;
        float enemyHpPercent = GetEnemyHealthPercent();
        float playerHpPercent = GetPlayerHealthPercent();
        ExecuteAIPattern(enemyHpPercent, playerHpPercent);
        DecayDefenseBonuses();
        OnCombatStateChanged?.Invoke();

        if (!PlayerStats.IsAlive())
        {
            StartCoroutine(HandleDefeatAfterDelay(turnDelay));
            return;
        }

        Invoke(nameof(StartPlayerTurn), turnDelay);
    }

    private bool CanExecuteEnemyAction()
    {
        return inCombat && enemyStats != null && currentEnemy != null;
    }

    private int GetEnemyTotalDefense()
    {
        int baseDefense = enemyStats.Get(StatType.Defense);
        return baseDefense + enemyDefenseBonus;
    }

    private float GetEnemyHealthPercent()
    {
        return (float)enemyStats.Get(StatType.Health) /
               enemyStats.Get(StatType.MaxHealth);
    }

    private float GetPlayerHealthPercent()
    {
        return (float)PlayerStats.Get(StatType.Health) /
               PlayerStats.Get(StatType.MaxHealth);
    }

    private void ExecuteAIPattern(float enemyHp, float playerHp)
    {
        switch (currentEnemy.aiPattern)
        {
            case EnemyAIPattern.Aggressive:
                AggressiveDecision(enemyHp);
                break;

            case EnemyAIPattern.Defensive:
                DefensiveDecision(enemyHp);
                break;

            case EnemyAIPattern.Finisher:
                FinisherDecision(playerHp);
                break;

            default:
                RandomDecision();
                break;
        }
    }

    private void DecayDefenseBonuses()
    {
        playerDefenseBonus = Mathf.Max(0, playerDefenseBonus - defenseDecayPerTurn);
        enemyDefenseBonus = Mathf.Max(0, enemyDefenseBonus - defenseDecayPerTurn);
    }

    private void RandomDecision()
    {
        float roll = UnityEngine.Random.value;

        if (roll < currentEnemy.defendChance)
        {
            Defend();
        }
        else if (roll < currentEnemy.defendChance + currentEnemy.specialAttackChance)
        {
            SpecialAttack();
        }
        else
        {
            Attack();
        }
    }

    private void AggressiveDecision(float enemyHp)
    {
        if (enemyHp < currentEnemy.lowHealthThreshold)
        {
            Defend();
        }
        else
        {
            SpecialAttackOrAttack();
        }
    }

    private void DefensiveDecision(float enemyHp)
    {
        if (enemyHp < 0.5f)
        {
            Defend();
        }
        else
        {
            Attack();
        }
    }

    private void FinisherDecision(float playerHp)
    {
        if (playerHp < currentEnemy.finisherThreshold)
        {
            SpecialAttack();
        }
        else
        {
            Attack();
        }
    }

    private void SpecialAttackOrAttack()
    {
        if (UnityEngine.Random.value < currentEnemy.specialAttackChance)
        {
            SpecialAttack();
        }
        else
        {
            Attack();
        }
    }

    private void Attack()
    {
        DealDamageToPlayer(currentEnemy.attack);
    }

    private void Defend()
    {
        int defenseBonus = Mathf.RoundToInt(currentEnemy.defense * 0.5f);
        enemyDefenseBonus = Mathf.Clamp(
            enemyDefenseBonus + defenseBonus,
            0,
            maxDefenseBonus
        );

        Log($"{currentEnemy.enemyName} defends! (Total Defense: {GetEnemyTotalDefense()})");
    }

    private void SpecialAttack()
    {
        int damage = Mathf.RoundToInt(currentEnemy.attack * 1.5f);
        DealDamageToPlayer(damage);
        Log($"{currentEnemy.enemyName} uses a special attack!");
    }

    private void DealDamageToPlayer(int baseDamage)
    {
        int totalDefense = CalculatePlayerTotalDefense();
        int damage = CalculateEnemyDamageToPlayer(baseDamage, totalDefense);
        PlayerStats.Modify(StatType.Health, -damage);
        Log($"{currentEnemy.enemyName} deals {damage} damage.");
    }

    private int CalculatePlayerTotalDefense()
    {
        int defense = playerDefenseBonus + PlayerStats.Get(StatType.Defense);

        if (Equipment != null)
        {
            defense += Equipment.GetTotalDefenseBonus();
        }

        return defense;
    }

    private int CalculateEnemyDamageToPlayer(int baseDamage, int defense)
    {
        float damageMultiplier = PlayerBuffs?.GetDamageMultiplier() ?? 1f;
        float damageReduction = PlayerBuffs?.GetDamageReduction() ?? 0f;

        int damage = CalculateFinalDamage(
            baseDamage,
            defense,
            false,
            0f,
            damageMultiplier
        );

        damage = Mathf.RoundToInt(damage * (1f - damageReduction));
        return damage;
    }

    private int CalculateFinalDamage(
        int baseDamage,
        int defense,
        bool ignoreDefense = false,
        float armorPenetration = 0f,
        float damageMultiplier = 1f)
    {
        int effectiveDefense = ignoreDefense
            ? 0
            : Mathf.RoundToInt(defense * (1f - armorPenetration));

        int finalDamage = Mathf.RoundToInt(
            (baseDamage - effectiveDefense) * damageMultiplier
        );

        return Mathf.Max(1, finalDamage);
    }

    private void OnPlayerHealthChanged()
    {
        if (!inCombat) return;

        if (!PlayerStats.IsAlive())
        {
            StartCoroutine(HandleDefeatAfterDelay(turnDelay));
        }
    }

    private void OnEnemyStatChanged(StatType type, int oldValue, int newValue)
    {
        if (type == StatType.Health && newValue <= 0)
        {
            StartCoroutine(HandleVictoryAfterDelay(turnDelay));
        }
    }

    IEnumerator HandleVictoryAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Victory();
    }

    IEnumerator HandleDefeatAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Defeat();
    }

    private void Victory()
    {
        if (combatResolved) return;
        combatResolved = true;
        GrantVictoryRewards();
        DropEnemyLoot();
        Log($"Defeated {currentEnemy.enemyName}!");
        EndCombat();
    }

    private void GrantVictoryRewards()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.AddExperience(currentEnemy.experienceReward);
            ProfileManager.Instance.AddCurrency(currentEnemy.currencyReward);
            Log($"Gained {currentEnemy.experienceReward} XP and {currentEnemy.currencyReward} Gold.");
        }
    }

    private void DropEnemyLoot()
    {
        if (enemyStats != null && enemyStats.TryGetComponent<EnemyLoot>(out var loot))
        {
            loot.DropLoot();
        }
    }

    private void Defeat()
    {
        if (combatResolved) return;
        combatResolved = true;
        ApplyDefeatPenalties();
        EndCombat();
        Log("You have been defeated and lost some currency.");
    }

    private void ApplyDefeatPenalties()
    {
        int healAmount = PlayerStats.Get(StatType.MaxHealth) / 3;
        PlayerStats.Set(StatType.Health, healAmount);

        if (ProfileManager.Instance != null)
        {
            int currencyLoss = Mathf.Min(
                ProfileManager.Instance.profile.currency / 4,
                50
            );

            ProfileManager.Instance.SpendCurrency(currencyLoss);
        }
    }

    private void EndCombat()
    {
        inCombat = false;
        ClearAllBuffs();
        ResetCombatVariables();
        RestorePlayerEnergy();
        OnCombatEnded?.Invoke();
        OnCombatStateChanged?.Invoke();

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();

        SaveSystem.SaveGame();
    }

    private void ClearAllBuffs()
    {
        foreach (var buff in activeCombatBuffs)
        {
            if (buff.ui != null)
            {
                Destroy(buff.ui.gameObject);
            }
        }

        activeCombatBuffs.Clear();
    }

    private void ResetCombatVariables()
    {
        currentEnemy = null;
        playerDefenseBonus = 0;
        enemyDefenseBonus = 0;
        isPlayerTurn = true;
        waitingForInput = true;
    }

    private void RestorePlayerEnergy()
    {
        PlayerStats.Modify(StatType.Energy, 50);
    }

    public void AddCombatBuff(string name, Sprite icon, float duration)
    {
        if (!CanAddBuff()) return;
        CombatBuff buff = CreateBuff(name, icon, duration);
        BuffUI buffUI = CreateBuffUI(buff);
        buff.ui = buffUI;
        activeCombatBuffs.Add(buff);
    }

    private bool CanAddBuff()
    {
        return buffUIPrefab != null && buffUIParent != null;
    }

    private CombatBuff CreateBuff(string name, Sprite icon, float duration)
    {
        return new CombatBuff
        {
            name = name,
            icon = icon,
            duration = duration,
            endTime = Time.time + duration
        };
    }

    private BuffUI CreateBuffUI(CombatBuff buff)
    {
        GameObject uiObj = Instantiate(buffUIPrefab, buffUIParent);
        BuffUI buffUI = uiObj.GetComponent<BuffUI>();
        buffUI.Setup(buff.icon, buff.name, buff.duration);
        return buffUI;
    }

    public List<CombatAction> GetAvailableActions()
    {
        return new List<CombatAction>(unlockedActions);
    }

    public void UnlockAction(CombatAction action)
    {
        if (action == null)
        {
            Debug.LogWarning("Attempted to unlock null action");
            return;
        }

        if (!unlockedActions.Contains(action))
        {
            unlockedActions.Add(action);
        }
    }

    public bool IsPlayerTurn()
    {
        return isPlayerTurn && waitingForInput;
    }

    private void Log(string message)
    {
        OnCombatLog?.Invoke(message);
    }
}

public enum EnemyAIPattern
{
    Random,
    Aggressive,
    Defensive,
    Finisher
}
