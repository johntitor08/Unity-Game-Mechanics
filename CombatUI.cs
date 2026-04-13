using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CombatUI : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds2 = new(2f);
    public static CombatUI Instance;
    private readonly List<CombatActionButton> actionButtons = new();
    private bool actionLocked;
    private readonly List<string> logLines = new();
    public bool IsInCombat => CombatManager.Instance != null && CombatManager.Instance.inCombat;
    public bool IsPlayerTurn => CombatManager.Instance != null && CombatManager.Instance.IsPlayerTurn();
    public EnemyData CurrentEnemy => CombatManager.Instance != null ? CombatManager.Instance.currentEnemy : null;

    [Header("Panels")]
    public GameObject combatPanel;
    public GameObject combatMapPanel;

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

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (combatPanel != null)
            combatPanel.SetActive(false);
    }

    void OnEnable()
    {
        StartCoroutine(SubscribeWhenReady());
    }

    void OnDisable()
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

    IEnumerator SubscribeWhenReady()
    {
        while (CombatManager.Instance == null || PlayerStats.Instance == null)
            yield return null;

        CombatManager.Instance.OnCombatStateChanged += UpdateUI;
        CombatManager.Instance.OnCombatLog += AddLogMessage;
        CombatManager.Instance.OnCombatStarted += OnCombatStarted;
        CombatManager.Instance.OnCombatEnded += OnCombatEnded;
        CombatManager.Instance.OnTurnChanged += OnTurnChanged;
        PlayerStats.Instance.OnHealthChanged += UpdatePlayerHealth;
        PlayerStats.Instance.OnEnergyChanged += UpdatePlayerEnergy;
    }

    void OnCombatStarted()
    {
        if (combatPanel != null)
            combatPanel.SetActive(true);

        logLines.Clear();
        UpdateUI();
        SetupActionButtons();
    }

    void OnCombatEnded()
    {
        StartCoroutine(DelayedCombatClose());
    }

    IEnumerator DelayedCombatClose()
    {
        yield return _waitForSeconds2;

        if (combatPanel != null)
            combatPanel.SetActive(false);

        logLines.Clear();
        UpdateLogDisplay();
    }

    void OnTurnChanged(bool isPlayerTurn)
    {
        if (isPlayerTurn)
            actionLocked = false;

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
        if (CombatManager.Instance == null || !CombatManager.Instance.inCombat)
        {
            if (combatPanel != null)
                combatPanel.SetActive(false);

            return;
        }

        UpdateEnemyDisplay();
        UpdatePlayerHealth();
        UpdatePlayerEnergy();
    }

    void UpdateEnemyDisplay()
    {
        var enemyStats = CombatManager.Instance.enemyStats;
        var enemyData = CombatManager.Instance.currentEnemy;

        if (enemyStats == null || enemyData == null)
            return;

        if (enemySprite != null)
            enemySprite.sprite = enemyData.sprite;

        if (enemyNameText != null)
            enemyNameText.text = enemyData.enemyName;

        if (enemyHealthBar != null)
        {
            enemyHealthBar.maxValue = enemyStats.Get(StatType.MaxHealth);
            enemyHealthBar.value = enemyStats.Get(StatType.Health);
        }

        if (enemyHealthText != null)
            enemyHealthText.text = $"HP: {enemyStats.Get(StatType.Health)} / {enemyStats.Get(StatType.MaxHealth)}";
    }

    void UpdatePlayerHealth()
    {
        if (PlayerStats.Instance == null)
            return;

        int current = PlayerStats.Instance.Get(StatType.Health);
        int max = PlayerStats.Instance.Get(StatType.MaxHealth);

        if (playerHealthBar != null)
        {
            playerHealthBar.maxValue = max;
            playerHealthBar.value = current;
        }

        if (playerHealthText != null)
            playerHealthText.text = $"HP: {current} / {max}";
    }

    void UpdatePlayerEnergy()
    {
        if (PlayerStats.Instance == null)
            return;

        int current = PlayerStats.Instance.Get(StatType.Energy);
        int max = PlayerStats.Instance.Get(StatType.MaxEnergy);

        if (playerEnergyBar != null)
        {
            playerEnergyBar.maxValue = max;
            playerEnergyBar.value = current;
        }

        if (playerEnergyText != null)
            playerEnergyText.text = $"Energy: {current} / {max}";

        if (IsInCombat)
            RefreshActionButtonsByEnergy();
    }

    void RefreshActionButtonsByEnergy()
    {
        if (CombatManager.Instance == null || !CombatManager.Instance.IsPlayerTurn() || actionLocked)
            return;

        int currentEnergy = PlayerStats.Instance.Get(StatType.Energy);

        foreach (var btn in actionButtons)
        {
            if (btn == null)
                continue;

            if (btn.action.isFlee)
                btn.UpdateInteractable(true);
            else
                btn.UpdateInteractable(currentEnergy >= btn.action.energyCost);
        }
    }

    void SetupActionButtons()
    {
        foreach (var btn in actionButtons)
            if (btn != null)
                Destroy(btn.gameObject);

        actionButtons.Clear();

        if (CombatManager.Instance == null)
            return;

        foreach (var action in CombatManager.Instance.GetAvailableActions())
        {
            var btn = Instantiate(actionButtonPrefab, actionsParent);

            if (action.isFlee)
            {
                int chance = Mathf.RoundToInt(CombatManager.Instance.GetFleeChance() * 100);
                action.actionName = $"Kaç ({chance}%)";
            }

            btn.Setup(action);
            actionButtons.Add(btn);
        }

        UpdateActionButtons(CombatManager.Instance.IsPlayerTurn());
    }

    public void DisableAllActionButtons()
    {
        actionLocked = true;

        foreach (var btn in actionButtons)
            if (btn != null)
                btn.UpdateInteractable(false);
    }

    void UpdateActionButtons(bool isPlayerTurn)
    {
        if (!isPlayerTurn)
        {
            foreach (var btn in actionButtons)
                if (btn != null)
                    btn.UpdateInteractable(false);
        }
        else
        {
            RefreshActionButtonsByEnergy();
        }
    }

    public void AddLogMessage(string message)
    {
        logLines.Add(message);

        while (logLines.Count > maxLogLines)
            logLines.RemoveAt(0);

        UpdateLogDisplay();
    }

    void UpdateLogDisplay()
    {
        if (combatLogText != null)
            combatLogText.text = string.Join("\n", logLines);
    }

    public void CloseButton()
    {
        if (combatMapPanel != null)
            combatMapPanel.SetActive(false);
    }
}
