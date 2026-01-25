using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;
    public event Action<string> OnCombatLog;
    public event Action OnCombatStarted;
    public event Action OnCombatEnded;
    public event Action OnCombatStateChanged;
    public event Action<bool> OnTurnChanged;
    public Transform buffUIParent;
    public GameObject buffUIPrefab;
    private readonly List<CombatBuff> activeCombatBuffs = new();

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
    public float turnDelay = 1f;
    public float enemyActionDelay = 1.5f;

    [Header("Combat Balancing")]
    public float baseCritChance = 0.1f;
    public float critDamageMultiplier = 1.5f;

    [Header("Status Effects")]
    public StatusEffectData burnEffect;
    public StatusEffectData poisonEffect;

    [Serializable]
    public class CombatBuff
    {
        public string name;
        public Sprite icon;
        public float duration;
        [HideInInspector] public float endTime;
        [HideInInspector] public BuffUI ui;
    }

    PlayerBuffManager PlayerBuffs => PlayerBuffManager.Instance;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    void Start()
    {
        unlockedActions.Clear();
        unlockedActions.AddRange(defaultActions);

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnHealthChanged += OnPlayerHealthChanged;
    }

    void Update()
    {
        if (!inCombat) return;

        for (int i = activeCombatBuffs.Count - 1; i >= 0; i--)
        {
            var buff = activeCombatBuffs[i];
            float timeLeft = buff.endTime - Time.time;

            if (buff.ui != null)
                buff.ui.UpdateTimer(timeLeft);

            if (timeLeft <= 0f)
            {
                if (buff.ui != null)
                    Destroy(buff.ui.gameObject);

                activeCombatBuffs.RemoveAt(i);
            }
        }
    }

    void OnDestroy()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.OnHealthChanged -= OnPlayerHealthChanged;

        if (enemyStats != null)
            enemyStats.OnStatChanged -= OnEnemyStatChanged;
    }

    public void StartCombat(EnemyData enemy)
    {
        if (inCombat || enemy == null) return;
        currentEnemy = enemy;

        if (enemyStats != null)
        {
            enemyStats.InitializeFromData(enemy);
            enemyStats.OnStatChanged += OnEnemyStatChanged;
        }

        inCombat = true;
        combatResolved = false;
        isPlayerTurn = true;
        waitingForInput = true;
        OnCombatStarted?.Invoke();
        OnTurnChanged?.Invoke(true);
        OnCombatStateChanged?.Invoke();
    }

    public void ExecutePlayerAction(CombatAction action)
    {
        if (!inCombat || !IsPlayerTurn()) return;

        if (!PlayerStats.Instance.HasEnoughEnergy(action.energyCost))
        {
            Log("Not enough energy!");
            return;
        }

        waitingForInput = false;
        PlayerStats.Instance.Modify(StatType.Energy, -action.energyCost);

        if (action.isDefensive)
            HandleDefensiveAction(action);
        else
            HandleOffensiveAction(action);

        OnCombatStateChanged?.Invoke();
        Invoke(nameof(StartEnemyTurn), turnDelay);
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

    void HandleDefensiveAction(CombatAction action)
    {
        playerDefenseBonus = Mathf.Clamp(playerDefenseBonus + action.defenseBonus, 0, 50);

        if (action.healAmount > 0)
            PlayerStats.Instance.Modify(StatType.Health, action.healAmount);
    }

    void HandleOffensiveAction(CombatAction action)
    {
        if (enemyStats == null) return;
        int damage = action.CalculateDamage();

        if (CheckCriticalHit(action))
        {
            damage = Mathf.RoundToInt(damage * critDamageMultiplier);
            Log("Critical Hit!");
        }

        float dmgMult = PlayerBuffs != null ? PlayerBuffs.GetDamageMultiplier() : 1f;
        damage = CalculateFinalDamage(
            damage,
            enemyDefenseBonus,
            action.ignoreDefense,
            action.armorPenetration,
            dmgMult
        );

        if (enemyStats.TryGetComponent<StatusEffectManager>(out var effects))
        {
            float reduction = effects.GetDamageReduction();
            damage = Mathf.RoundToInt(damage * (1f - reduction));
        }

        enemyStats.Modify(StatType.Health, -damage);
        TryApplyOnHitEffects(enemyStats);
        Log($"Dealt {damage} damage to {currentEnemy.enemyName}.");

        if (enemyStats.Get(StatType.Health) <= 0)
            StartCoroutine(HandleVictoryAfterDelay(turnDelay));
    }

    void TryApplyOnHitEffects(EnemyStats target)
    {
        if (!target.TryGetComponent<StatusEffectManager>(out var effects))
            return;

        float roll = UnityEngine.Random.value;

        if (burnEffect != null && roll < 0.2f)
            effects.ApplyEffect(burnEffect);
        else if (poisonEffect != null && roll < 0.5f)
            effects.ApplyEffect(poisonEffect);
    }

    bool CheckCriticalHit(CombatAction action)
    {
        if (action.guaranteedCrit) return true;
        float chance = baseCritChance + action.critChanceBonus;
        chance += PlayerStats.Instance.Get(StatType.Luck) * 0.01f;
        return UnityEngine.Random.value <= Mathf.Clamp01(chance);
    }

    void ExecuteEnemyAction()
    {
        if (!inCombat || enemyStats == null || currentEnemy == null) return;

        float enemyHp =
            (float)enemyStats.Get(StatType.Health) /
            enemyStats.Get(StatType.MaxHealth);

        float playerHp =
            (float)PlayerStats.Instance.Get(StatType.Health) /
            PlayerStats.Instance.Get(StatType.MaxHealth);

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

        playerDefenseBonus = Mathf.Max(0, playerDefenseBonus - 2);
        enemyDefenseBonus = Mathf.Max(0, enemyDefenseBonus - 2);
        OnCombatStateChanged?.Invoke();

        if (!PlayerStats.Instance.IsAlive())
        {
            StartCoroutine(HandleDefeatAfterDelay(turnDelay));
            return;
        }

        Invoke(nameof(StartPlayerTurn), turnDelay);
    }

    void RandomDecision()
    {
        float roll = UnityEngine.Random.value;

        if (roll < currentEnemy.defendChance)
            Defend();
        else if (roll < currentEnemy.defendChance + currentEnemy.specialAttackChance)
            SpecialAttack();
        else
            Attack();
    }

    void AggressiveDecision(float enemyHp)
    {
        if (enemyHp < currentEnemy.lowHealthThreshold)
            Defend();
        else
            SpecialAttackOrAttack();
    }

    void DefensiveDecision(float enemyHp)
    {
        if (enemyHp < 0.5f)
            Defend();
        else
            Attack();
    }

    void FinisherDecision(float playerHp)
    {
        if (playerHp < currentEnemy.finisherThreshold)
            SpecialAttack();
        else
            Attack();
    }

    void Attack()
    {
        enemyDefenseBonus = 0;
        DealDamageToPlayer(currentEnemy.attack);
    }

    void Defend()
    {
        enemyDefenseBonus = Mathf.Clamp(
            enemyDefenseBonus + Mathf.RoundToInt(currentEnemy.defense * 0.5f),
            0, 50);

        Log($"{currentEnemy.enemyName} defends!");
    }

    void SpecialAttack()
    {
        enemyDefenseBonus = 0;
        int dmg = Mathf.RoundToInt(currentEnemy.attack * 1.5f);
        DealDamageToPlayer(dmg);
        Log($"{currentEnemy.enemyName} uses a special attack!");
    }

    void SpecialAttackOrAttack()
    {
        if (UnityEngine.Random.value < currentEnemy.specialAttackChance)
            SpecialAttack();
        else
            Attack();
    }

    void DealDamageToPlayer(int baseDamage)
    {
        int totalDefense = playerDefenseBonus + PlayerStats.Instance.Get(StatType.Defense);

        if (EquipmentManager.Instance != null)
            totalDefense += EquipmentManager.Instance.GetTotalDefenseBonus();

        float dmgMult = PlayerBuffs != null ? PlayerBuffs.GetDamageMultiplier() : 1f;
        float reduction = PlayerBuffs != null ? PlayerBuffs.GetDamageReduction() : 0f;
        int damage = CalculateFinalDamage(baseDamage, totalDefense, false, 0f, dmgMult);
        damage = Mathf.RoundToInt(damage * (1f - reduction));
        PlayerStats.Instance.Modify(StatType.Health, -damage);
        Log($"{currentEnemy.enemyName} deals {damage} damage.");
    }

    int CalculateFinalDamage(
        int baseDamage,
        int defense,
        bool ignoreDefense = false,
        float armorPenetration = 0f,
        float damageMultiplier = 1f)
    {
        int effectiveDefense =
            ignoreDefense ? 0 : Mathf.RoundToInt(defense * (1f - armorPenetration));

        return Mathf.Max(1,
            Mathf.RoundToInt((baseDamage - effectiveDefense) * damageMultiplier));
    }

    void OnPlayerHealthChanged()
    {
        if (!inCombat) return;

        if (!PlayerStats.Instance.IsAlive())
            StartCoroutine(HandleDefeatAfterDelay(turnDelay));
    }

    void OnEnemyStatChanged(StatType type, int oldValue, int newValue)
    {
        if (type == StatType.Health && newValue <= 0)
            StartCoroutine(HandleVictoryAfterDelay(turnDelay));
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

    void Victory()
    {
        if (combatResolved) return;
        combatResolved = true;

        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.AddExperience(currentEnemy.experienceReward);
            ProfileManager.Instance.AddCurrency(currentEnemy.currencyReward);
            Log($"Gained {currentEnemy.experienceReward} XP and {currentEnemy.currencyReward} Gold.");
        }

        if (enemyStats != null && enemyStats.TryGetComponent<EnemyLoot>(out var loot))
            loot.DropLoot();

        Log($"Defeated {currentEnemy.enemyName}!");
        EndCombat();
    }

    void Defeat()
    {
        if (combatResolved) return;
        combatResolved = true;
        int heal = PlayerStats.Instance.Get(StatType.MaxHealth) / 3;
        PlayerStats.Instance.Set(StatType.Health, heal);

        if (ProfileManager.Instance != null)
        {
            int loss = Mathf.Min(ProfileManager.Instance.profile.currency / 4, 50);
            ProfileManager.Instance.SpendCurrency(loss);
        }

        EndCombat();
        Log("You have been defeated and lost some currency.");
    }

    void EndCombat()
    {
        inCombat = false;

        foreach (var buff in activeCombatBuffs)
            if (buff.ui != null)
                Destroy(buff.ui.gameObject);

        activeCombatBuffs.Clear();
        currentEnemy = null;
        playerDefenseBonus = 0;
        enemyDefenseBonus = 0;
        isPlayerTurn = true;
        waitingForInput = true;
        PlayerStats.Instance.Modify(StatType.Energy, 50);
        OnCombatEnded?.Invoke();
        OnCombatStateChanged?.Invoke();
        SaveSystem.SaveGame();
    }

    public void AddCombatBuff(string name, Sprite icon, float duration)
    {
        if (buffUIPrefab == null || buffUIParent == null) return;

        var buff = new CombatBuff
        {
            name = name,
            icon = icon,
            duration = duration,
            endTime = Time.time + duration
        };

        var uiObj = Instantiate(buffUIPrefab, buffUIParent);
        buff.ui = uiObj.GetComponent<BuffUI>();
        buff.ui.Setup(icon, name, duration);
        activeCombatBuffs.Add(buff);
    }

    public List<CombatAction> GetAvailableActions() => new(unlockedActions);

    public void UnlockAction(CombatAction action)
    {
        if (!unlockedActions.Contains(action))
            unlockedActions.Add(action);
    }

    public bool IsPlayerTurn() => isPlayerTurn && waitingForInput;

    void Log(string msg) => OnCombatLog?.Invoke(msg);
}

public enum EnemyAIPattern
{
    Random,
    Aggressive,
    Defensive,
    Finisher
}
