using UnityEngine;
using System;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Combat State")]
    public bool inCombat;
    public EnemyData currentEnemy;
    public EnemyStats enemyStats;
    public int enemyCurrentHealth;
    public int enemyDefenseBonus;

    [Header("Player Actions")]
    public List<CombatAction> defaultActions = new();
    private readonly List<CombatAction> unlockedActions = new();

    [Header("Turn Settings")]
    public float turnDelay = 1f;
    public float enemyActionDelay = 1.5f;

    [Header("Combat Balancing")]
    public float baseCritChance = 0.1f;
    public float critDamageMultiplier = 1.5f;

    private int playerDefenseBonus;
    private bool isPlayerTurn;
    private bool waitingForInput;

    public event Action<string> OnCombatLog;
    public event Action OnCombatStarted;
    public event Action OnCombatEnded;
    public event Action OnCombatStateChanged;
    public event Action<bool> OnTurnChanged;

    private StatusEffectManager PlayerEffects
    {
        get
        {
            var playerStats = PlayerStats.Instance;

            if (playerStats == null)
                return null;

            return playerStats.GetComponent<StatusEffectManager>();
        }
    }

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
    }

    public void StartCombat(EnemyData enemy)
    {
        if (inCombat) return;
        currentEnemy = enemy;
        enemyStats.InitializeFromData(enemy);
        inCombat = true;
        isPlayerTurn = true;
        waitingForInput = true;
        OnCombatStarted?.Invoke();
        OnCombatStateChanged?.Invoke();
        OnTurnChanged?.Invoke(true);
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

    void HandleDefensiveAction(CombatAction action)
    {
        playerDefenseBonus += action.defenseBonus;

        if (action.healAmount > 0)
        {
            PlayerStats.Instance.Modify(StatType.Health, action.healAmount);
        }
    }

    void HandleOffensiveAction(CombatAction action)
    {
        int damage = action.CalculateDamage();

        if (CheckCriticalHit(action))
        {
            damage = Mathf.RoundToInt(damage * critDamageMultiplier);
            Log("Critical Hit!");
        }

        if (!action.ignoreDefense)
            damage = Mathf.Max(1, damage - enemyDefenseBonus);

        float multiplier = 1f;
        var effects = PlayerEffects;

        if (effects != null)
            multiplier = effects.GetDamageMultiplier();

        damage = Mathf.RoundToInt(damage * multiplier);
        enemyStats.Modify(StatType.Health, -damage);

        if (enemyStats != null && enemyStats.Get(StatType.Health) <= 0)
            Invoke(nameof(Victory), turnDelay);
    }

    bool CheckCriticalHit(CombatAction action)
    {
        if (action.guaranteedCrit) return true;
        float chance = baseCritChance + action.critChanceBonus;
        chance += PlayerStats.Instance.Get(StatType.Luck) * 0.01f;
        return UnityEngine.Random.value <= chance;
    }

    void StartEnemyTurn()
    {
        if (!inCombat) return;
        isPlayerTurn = false;
        OnTurnChanged?.Invoke(false);
        Invoke(nameof(ExecuteEnemyAction), enemyActionDelay);
    }

    void ExecuteEnemyAction()
    {
        if (!inCombat) return;
        float roll = UnityEngine.Random.value;

        if (roll < currentEnemy.defendChance)
        {
            enemyDefenseBonus += Mathf.RoundToInt(currentEnemy.defense * 0.5f);
            Log($"{currentEnemy.enemyName} defends!");
        }
        else if (roll < currentEnemy.defendChance + currentEnemy.specialAttackChance)
        {
            enemyDefenseBonus = 0;
            DealDamageToPlayer(Mathf.RoundToInt(currentEnemy.attack * 1.5f));
        }
        else
        {
            DealDamageToPlayer(currentEnemy.attack);
        }

        playerDefenseBonus = Mathf.Max(0, playerDefenseBonus - 2);
        enemyDefenseBonus = Mathf.Max(0, enemyDefenseBonus - 2);
        OnCombatStateChanged?.Invoke();

        if (!PlayerStats.Instance.IsAlive())
        {
            Invoke(nameof(Defeat), turnDelay);
            return;
        }

        Invoke(nameof(StartPlayerTurn), turnDelay);
    }

    void DealDamageToPlayer(int baseDamage)
    {
        int totalDefense = playerDefenseBonus + PlayerStats.Instance.Get(StatType.Defense);

        if (EquipmentManager.Instance != null)
            totalDefense += EquipmentManager.Instance.GetTotalDefenseBonus();

        int damage = Mathf.Max(1, baseDamage - totalDefense);
        float multiplier = 1f;
        var effects = PlayerEffects;

        if (effects != null)
            multiplier = effects.GetDamageMultiplier();

        damage = Mathf.RoundToInt(damage * multiplier);
        PlayerStats.Instance.Modify(StatType.Health, -damage);
        Log($"{currentEnemy.enemyName} attacks! {damage} damage!");
    }

    void StartPlayerTurn()
    {
        isPlayerTurn = true;
        waitingForInput = true;
        OnTurnChanged?.Invoke(true);
        Log("Your turn!");
    }

    void Victory()
    {
        Log($"Victory! {currentEnemy.enemyName} defeated!");

        // Loot düşür
        if (enemyStats != null)
        {
            if (enemyStats.TryGetComponent<EnemyLoot>(out var loot))
            {
                loot.DropLoot();
            }
        }

        // XP & para
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.AddExperience(currentEnemy.experienceReward);
            ProfileManager.Instance.AddCurrency(currentEnemy.currencyReward);
        }

        DropLoot();
        EndCombat();
    }


    void Defeat()
    {
        Log($"Defeated by {currentEnemy.enemyName}...");

        if (ProfileManager.Instance != null)
        {
            int loss = Mathf.Min(ProfileManager.Instance.profile.currency / 4, 50);
            ProfileManager.Instance.SpendCurrency(loss);
        }

        int heal = PlayerStats.Instance.Get(StatType.MaxHealth) / 3;
        PlayerStats.Instance.Set(StatType.Health, heal);
        EndCombat();
    }

    void DropLoot()
    {
        if (currentEnemy.possibleLoot == null || currentEnemy.lootChances == null) return;

        for (int i = 0; i < currentEnemy.possibleLoot.Length; i++)
        {
            if (currentEnemy.possibleLoot[i] == null) continue;
            float chance = currentEnemy.lootChances[i];
            
            if (chance > 0) {
                chance = Mathf.Clamp01(chance + PlayerStats.Instance.Get(StatType.Luck) * 0.005f);
            }

            if (UnityEngine.Random.value <= chance)
            {
                // Loot ekle
                InventoryManager.Instance.AddItem(currentEnemy.possibleLoot[i], 1);
                string itemName = currentEnemy.possibleLoot[i].itemName;

                if (CombatUI.Instance != null)
                {
                    CombatUI.Instance.AddLogMessage($"<color=yellow>Found 1 {itemName}!</color>");
                }
            }
        }
    }

    void EndCombat()
    {
        inCombat = false;
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

    public void UnlockAction(CombatAction action)
    {
        if (!unlockedActions.Contains(action))
        {
            unlockedActions.Add(action);
            Log($"Unlocked new combat action: {action.actionName}!");
        }
    }

    public List<CombatAction> GetAvailableActions() => new(unlockedActions);

    public bool IsPlayerTurn() => isPlayerTurn && waitingForInput;

    void Log(string msg) => OnCombatLog?.Invoke(msg);
}
