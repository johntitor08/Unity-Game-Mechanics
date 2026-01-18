using UnityEngine;
using System.Collections.Generic;
using System;

public class CombatManager : MonoBehaviour
{
    public static CombatManager Instance;

    [Header("Combat State")]
    public bool inCombat = false;
    public EnemyData currentEnemy;
    public int enemyCurrentHealth;
    public int enemyDefenseBonus = 0;

    [Header("Player Actions")]
    public List<CombatAction> defaultActions = new();
    private List<CombatAction> unlockedActions = new();

    [Header("Turn Settings")]
    public float turnDelay = 1f;
    public float enemyActionDelay = 1.5f;

    [Header("Combat Balancing")]
    public float baseCritChance = 0.1f;
    public float critDamageMultiplier = 1.5f;

    private int playerDefenseBonus = 0;
    private bool isPlayerTurn = true;
    private bool waitingForInput = true;

    public event Action<string> OnCombatLog;
    public event Action OnCombatStateChanged;
    public event Action OnCombatStarted;
    public event Action OnCombatEnded;
    public event Action<bool> OnTurnChanged; // true = player turn

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
        }
    }

    void Start()
    {
        // Initialize with default actions
        unlockedActions = new List<CombatAction>(defaultActions);
    }

    public void StartCombat(EnemyData enemy)
    {
        if (inCombat) return;

        currentEnemy = enemy;
        enemyCurrentHealth = enemy.maxHealth;
        playerDefenseBonus = 0;
        enemyDefenseBonus = 0;
        inCombat = true;
        isPlayerTurn = true;
        waitingForInput = true;

        Log($"Combat started! {currentEnemy.enemyName} appeared!");
        OnCombatStarted?.Invoke();
        OnCombatStateChanged?.Invoke();
        OnTurnChanged?.Invoke(true);
    }

    public void ExecutePlayerAction(CombatAction action)
    {
        if (!inCombat || !isPlayerTurn || !waitingForInput) return;

        // Check energy
        if (!PlayerStats.Instance.HasEnoughEnergy(action.energyCost))
        {
            Log("Not enough energy!");
            return;
        }

        waitingForInput = false;

        // Consume energy
        PlayerStats.Instance.Modify(StatType.Energy, -action.energyCost);

        if (action.isDefensive)
        {
            // Defensive action
            playerDefenseBonus += action.defenseBonus;
            Log($"You use {action.actionName}! Defense +{action.defenseBonus}");

            if (action.healAmount > 0)
            {
                PlayerStats.Instance.Modify(StatType.Health, action.healAmount);
                Log($"Restored {action.healAmount} HP!");
            }
        }
        else
        {
            // Offensive action
            int damage = action.CalculateDamage();

            // Check for critical hit
            bool isCrit = CheckCriticalHit(action);
            if (isCrit)
            {
                damage = Mathf.RoundToInt(damage * critDamageMultiplier);
                Log($"Critical Hit!");
            }

            // Apply defense
            if (!action.ignoreDefense)
            {
                damage = Mathf.Max(1, damage - enemyDefenseBonus);
            }

            enemyCurrentHealth -= damage;
            Log($"You use {action.actionName}! Dealt {damage} damage to {currentEnemy.enemyName}!");

            // Check if enemy defeated
            if (enemyCurrentHealth <= 0)
            {
                Invoke(nameof(Victory), turnDelay);
                OnCombatStateChanged?.Invoke();
                return;
            }
        }

        OnCombatStateChanged?.Invoke();

        // Enemy turn after delay
        Invoke(nameof(StartEnemyTurn), turnDelay);
    }

    bool CheckCriticalHit(CombatAction action)
    {
        if (action.guaranteedCrit) return true;

        float critChance = baseCritChance + action.critChanceBonus;

        // Add luck bonus
        if (PlayerStats.Instance != null)
        {
            int luck = PlayerStats.Instance.Get(StatType.Luck);
            critChance += luck * 0.01f; // 1% per luck point
        }

        return UnityEngine.Random.value <= critChance;
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

        float actionRoll = UnityEngine.Random.value;

        // Enemy AI decision making
        if (actionRoll < currentEnemy.defendChance)
        {
            // Defend
            enemyDefenseBonus += Mathf.RoundToInt(currentEnemy.defense * 0.5f);
            Log($"{currentEnemy.enemyName} takes a defensive stance!");
        }
        else if (actionRoll < currentEnemy.defendChance + currentEnemy.specialAttackChance)
        {
            // Special attack (more damage, costs defense bonus)
            int damage = Mathf.RoundToInt(currentEnemy.attack * 1.5f);
            enemyDefenseBonus = 0;
            DealDamageToPlayer(damage, true);
        }
        else
        {
            // Normal attack
            DealDamageToPlayer(currentEnemy.attack, false);
        }

        // Decay defense bonuses
        playerDefenseBonus = Mathf.Max(0, playerDefenseBonus - 2);
        enemyDefenseBonus = Mathf.Max(0, enemyDefenseBonus - 2);

        OnCombatStateChanged?.Invoke();

        // Check if player defeated
        if (!PlayerStats.Instance.IsAlive())
        {
            Invoke(nameof(Defeat), turnDelay);
            return;
        }

        // Back to player turn
        Invoke(nameof(StartPlayerTurn), turnDelay);
    }

    void DealDamageToPlayer(int baseDamage, bool isSpecial)
    {
        int damage = baseDamage;

        // Apply player defense
        int totalDefense = playerDefenseBonus;
        if (PlayerStats.Instance != null)
        {
            totalDefense += PlayerStats.Instance.Get(StatType.Defense);
        }

        //if (EquipmentManager.Instance != null)
        //{
        //    totalDefense += EquipmentManager.Instance.GetTotalDefenseBonus();
        //}

        damage = Mathf.Max(1, damage - totalDefense);

        PlayerStats.Instance.Modify(StatType.Health, -damage);

        string attackType = isSpecial ? "uses a powerful attack" : "attacks";
        Log($"{currentEnemy.enemyName} {attackType}! Dealt {damage} damage!");
    }

    void StartPlayerTurn()
    {
        if (!inCombat) return;

        isPlayerTurn = true;
        waitingForInput = true;
        OnTurnChanged?.Invoke(true);
        Log("Your turn!");
    }

    void Victory()
    {
        Log($"Victory! {currentEnemy.enemyName} defeated!");

        // Award experience
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.AddExperience(currentEnemy.experienceReward);
            Log($"Gained {currentEnemy.experienceReward} EXP!");
        }

        // Award currency
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.AddCurrency(currentEnemy.currencyReward);
            Log($"Gained {currentEnemy.currencyReward} Gold!");
        }

        // Roll for loot
        DropLoot();

        EndCombat();
    }

    void Defeat()
    {
        Log($"Defeated by {currentEnemy.enemyName}...");

        // Penalty: lose some currency
        if (ProfileManager.Instance != null)
        {
            int currencyLoss = Mathf.Min(ProfileManager.Instance.profile.currency / 4, 50);
            if (currencyLoss > 0)
            {
                ProfileManager.Instance.SpendCurrency(currencyLoss);
                Log($"Lost {currencyLoss} Gold...");
            }
        }

        // Restore some health
        if (PlayerStats.Instance != null)
        {
            int healAmount = PlayerStats.Instance.Get(StatType.MaxHealth) / 3;
            PlayerStats.Instance.Set(StatType.Health, healAmount);
            Log($"Recovered to {healAmount} HP");
        }

        EndCombat();
    }

    void DropLoot()
    {
        if (currentEnemy.possibleLoot == null || currentEnemy.possibleLoot.Length == 0)
            return;

        for (int i = 0; i < currentEnemy.possibleLoot.Length && i < currentEnemy.lootChances.Length; i++)
        {
            if (UnityEngine.Random.value <= currentEnemy.lootChances[i])
            {
                ItemData item = currentEnemy.possibleLoot[i];
                InventoryManager.Instance.AddItem(item, 1);
                Log($"Obtained {item.itemName}!");
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

        // Restore some energy after combat
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.Modify(StatType.Energy, 50);
        }

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

    public List<CombatAction> GetAvailableActions()
    {
        return new List<CombatAction>(unlockedActions);
    }

    public bool IsPlayerTurn() => isPlayerTurn && waitingForInput;

    void Log(string message)
    {
        //Debug.Log($"[Combat] {message}");
        OnCombatLog?.Invoke(message);
    }
}
