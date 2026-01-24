using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

public class CombatUI : MonoBehaviour
{
    public static CombatUI Instance;

    [Header("Panels")]
    public GameObject combatPanel;

    [Header("Enemy Display")]
    public Image enemySprite;
    public TextMeshProUGUI enemyNameText;
    public Slider enemyHealthBar;
    public TextMeshProUGUI enemyHealthText;

    [Header("Player Display")]
    public Slider playerHealthBar;
    public TextMeshProUGUI playerHealthText;
    public Slider playerEnergyBar;
    public TextMeshProUGUI playerEnergyText;

    [Header("Turn Indicator")]
    public GameObject playerTurnIndicator;
    public GameObject enemyTurnIndicator;
    public TextMeshProUGUI turnText;

    [Header("Actions")]
    public Transform actionsParent;
    public CombatActionButton actionButtonPrefab;

    [Header("Combat Log")]
    public TextMeshProUGUI combatLogText;
    public int maxLogLines = 8;

    private readonly List<CombatActionButton> actionButtons = new();
    private readonly List<string> logLines = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStateChanged += UpdateUI;
            CombatManager.Instance.OnCombatLog += AddLogMessage;
            CombatManager.Instance.OnCombatStarted += OnCombatStarted;
            CombatManager.Instance.OnCombatEnded += OnCombatEnded;
            CombatManager.Instance.OnTurnChanged += OnTurnChanged;
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnHealthChanged += UpdatePlayerHealth;
            PlayerStats.Instance.OnEnergyChanged += UpdatePlayerEnergy;
        }

        combatPanel.SetActive(false);
    }

    void OnDestroy()
    {
        if (CombatManager.Instance != null)
        {
            CombatManager.Instance.OnCombatStateChanged -= UpdateUI;
            CombatManager.Instance.OnCombatLog -= AddLogMessage;
            CombatManager.Instance.OnCombatStarted -= OnCombatStarted;
            CombatManager.Instance.OnCombatEnded -= OnCombatEnded;
            CombatManager.Instance.OnTurnChanged -= OnTurnChanged;
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnHealthChanged -= UpdatePlayerHealth;
            PlayerStats.Instance.OnEnergyChanged -= UpdatePlayerEnergy;
        }
    }

    void OnCombatStarted()
    {
        combatPanel.SetActive(true);
        logLines.Clear();
        UpdateUI();
    }

    void OnCombatEnded()
    {
        combatPanel.SetActive(false);
    }

    void OnTurnChanged(bool isPlayerTurn)
    {
        if (playerTurnIndicator != null)
            playerTurnIndicator.SetActive(isPlayerTurn);

        if (enemyTurnIndicator != null)
            enemyTurnIndicator.SetActive(!isPlayerTurn);

        if (turnText != null)
            turnText.text = isPlayerTurn ? "YOUR TURN" : "ENEMY TURN";

        UpdateActionButtons(isPlayerTurn);
    }

    void UpdateUI()
    {
        if (!CombatManager.Instance.inCombat)
        {
            combatPanel.SetActive(false);
            return;
        }

        UpdateEnemyDisplay();
        UpdatePlayerHealth();
        UpdatePlayerEnergy();
        SetupActionButtons();
    }

    void UpdateEnemyDisplay()
    {
        var enemyStats = CombatManager.Instance.enemyStats;
        if (enemyStats == null) return;
        var enemyData = CombatManager.Instance.currentEnemy;

        if (enemySprite != null && enemyData != null)
            enemySprite.sprite = enemyData.sprite;

        if (enemyNameText != null && enemyData != null)
            enemyNameText.text = enemyData.enemyName;

        if (enemyHealthBar != null)
        {
            enemyHealthBar.maxValue = enemyStats.Get(StatType.MaxHealth);
            enemyHealthBar.value = enemyStats.Get(StatType.Health);
        }

        if (enemyHealthText != null)
        {
            enemyHealthText.text = $"HP: {enemyStats.Get(StatType.Health)} / {enemyStats.Get(StatType.MaxHealth)}";
        }
    }

    void UpdatePlayerHealth()
    {
        if (PlayerStats.Instance == null) return;
        int currentHealth = PlayerStats.Instance.Get(StatType.Health);
        int maxHealth = PlayerStats.Instance.Get(StatType.MaxHealth);

        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = maxHealth;
            playerHealthBar.value = currentHealth;
        }

        if (playerHealthText != null)
        {
            playerHealthText.text = $"HP: {currentHealth} / {maxHealth}";
        }
    }

    void UpdatePlayerEnergy()
    {
        if (PlayerStats.Instance == null) return;
        int currentEnergy = PlayerStats.Instance.Get(StatType.Energy);
        int maxEnergy = PlayerStats.Instance.Get(StatType.MaxEnergy);

        if (playerEnergyBar != null)
        {
            playerEnergyBar.maxValue = maxEnergy;
            playerEnergyBar.value = currentEnergy;
        }

        if (playerEnergyText != null)
        {
            playerEnergyText.text = $"Energy: {currentEnergy} / {maxEnergy}";
        }
    }

    void SetupActionButtons()
    {
        // Clear existing buttons
        foreach (var btn in actionButtons)
        {
            if (btn != null) Destroy(btn.gameObject);
        }

        actionButtons.Clear();
        var actions = CombatManager.Instance.GetAvailableActions();

        foreach (var action in actions)
        {
            var btn = Instantiate(actionButtonPrefab, actionsParent);
            btn.Setup(action);
            actionButtons.Add(btn);
        }

        UpdateActionButtons(CombatManager.Instance.IsPlayerTurn());
    }

    void UpdateActionButtons(bool interactable)
    {
        foreach (var btn in actionButtons)
        {
            if (btn != null)
                btn.UpdateInteractable(interactable);
        }
    }

    public void AddLogMessage(string message)
    {
        logLines.Add(message);

        // Keep only last N lines
        while (logLines.Count > maxLogLines)
        {
            logLines.RemoveAt(0);
        }

        UpdateLogDisplay();
    }

    void UpdateLogDisplay()
    {
        if (combatLogText == null) return;
        combatLogText.text = string.Join("\n", logLines);
    }
}
