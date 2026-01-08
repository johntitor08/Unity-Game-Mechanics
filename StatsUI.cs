using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class StatsUI : MonoBehaviour
{
    public static StatsUI Instance;

    [Header("Health Bar")]
    public Slider healthBar;
    public TextMeshProUGUI healthText;
    public Image healthFillImage;
    public Color healthHighColor = Color.green;
    public Color healthMediumColor = Color.yellow;
    public Color healthLowColor = Color.red;

    [Header("Energy Bar")]
    public Slider energyBar;
    public TextMeshProUGUI energyText;
    public Image energyFillImage;
    public Color energyColor = new Color(0.3f, 0.5f, 1f);

    [Header("Stats Display")]
    public GameObject statsPanel;
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI charismaText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI luckText;

    [Header("Settings")]
    public bool showStatsPanel = true;
    public KeyCode toggleKey = KeyCode.C;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnHealthChanged += UpdateHealthBar;
            PlayerStats.Instance.OnEnergyChanged += UpdateEnergyBar;
            PlayerStats.Instance.OnStatChanged += OnStatChanged;
        }

        if (statsPanel != null)
            statsPanel.SetActive(showStatsPanel);

        RefreshAllStats();
    }

    void OnDestroy()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnHealthChanged -= UpdateHealthBar;
            PlayerStats.Instance.OnEnergyChanged -= UpdateEnergyBar;
            PlayerStats.Instance.OnStatChanged -= OnStatChanged;
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && statsPanel != null)
        {
            statsPanel.SetActive(!statsPanel.activeSelf);
        }
    }

    void OnStatChanged(StatType type, int oldValue, int newValue)
    {
        // Update specific stat display
        switch (type)
        {
            case StatType.Strength:
                if (strengthText != null)
                    strengthText.text = $"Strength: {newValue}";
                break;
            case StatType.Intelligence:
                if (intelligenceText != null)
                    intelligenceText.text = $"Intelligence: {newValue}";
                break;
            case StatType.Charisma:
                if (charismaText != null)
                    charismaText.text = $"Charisma: {newValue}";
                break;
            case StatType.Defense:
                if (defenseText != null)
                    defenseText.text = $"Defense: {newValue}";
                break;
            case StatType.Speed:
                if (speedText != null)
                    speedText.text = $"Speed: {newValue}";
                break;
            case StatType.Luck:
                if (luckText != null)
                    luckText.text = $"Luck: {newValue}";
                break;
        }

        // Animate the change
        if (type != StatType.Health && type != StatType.Energy)
        {
            AnimateStatChange(type, oldValue, newValue);
        }
    }

    void UpdateHealthBar()
    {
        if (PlayerStats.Instance == null) return;

        int currentHealth = PlayerStats.Instance.Get(StatType.Health);
        int maxHealth = PlayerStats.Instance.Get(StatType.MaxHealth);
        float percentage = PlayerStats.Instance.GetHealthPercentage();

        // Update slider
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        // Update text
        if (healthText != null)
        {
            healthText.text = $"{currentHealth} / {maxHealth}";
        }

        // Update color based on percentage
        if (healthFillImage != null)
        {
            if (percentage > 0.6f)
                healthFillImage.color = healthHighColor;
            else if (percentage > 0.3f)
                healthFillImage.color = healthMediumColor;
            else
                healthFillImage.color = healthLowColor;
        }
    }

    void UpdateEnergyBar()
    {
        if (PlayerStats.Instance == null) return;

        int currentEnergy = PlayerStats.Instance.Get(StatType.Energy);
        int maxEnergy = PlayerStats.Instance.Get(StatType.MaxEnergy);

        // Update slider
        if (energyBar != null)
        {
            energyBar.maxValue = maxEnergy;
            energyBar.value = currentEnergy;
        }

        // Update text
        if (energyText != null)
        {
            energyText.text = $"{currentEnergy} / {maxEnergy}";
        }

        // Update color
        if (energyFillImage != null)
        {
            energyFillImage.color = energyColor;
        }
    }

    void RefreshAllStats()
    {
        UpdateHealthBar();
        UpdateEnergyBar();

        if (PlayerStats.Instance == null) return;

        if (strengthText != null)
            strengthText.text = $"Strength: {PlayerStats.Instance.Get(StatType.Strength)}";
        if (intelligenceText != null)
            intelligenceText.text = $"Intelligence: {PlayerStats.Instance.Get(StatType.Intelligence)}";
        if (charismaText != null)
            charismaText.text = $"Charisma: {PlayerStats.Instance.Get(StatType.Charisma)}";
        if (defenseText != null)
            defenseText.text = $"Defense: {PlayerStats.Instance.Get(StatType.Defense)}";
        if (speedText != null)
            speedText.text = $"Speed: {PlayerStats.Instance.Get(StatType.Speed)}";
        if (luckText != null)
            luckText.text = $"Luck: {PlayerStats.Instance.Get(StatType.Luck)}";
    }

    void AnimateStatChange(StatType type, int oldValue, int newValue)
    {
        // Simple pulse animation on stat change
        TextMeshProUGUI targetText = type switch
        {
            StatType.Strength => strengthText,
            StatType.Intelligence => intelligenceText,
            StatType.Charisma => charismaText,
            StatType.Defense => defenseText,
            StatType.Speed => speedText,
            StatType.Luck => luckText,
            _ => null
        };

        if (targetText != null)
        {
            StartCoroutine(PulseText(targetText));
        }
    }

    IEnumerator PulseText(TextMeshProUGUI text)
    {
        Vector3 originalScale = text.transform.localScale;
        Vector3 targetScale = originalScale * 1.2f;
        float duration = 0.2f;
        float elapsed = 0f;

        // Scale up
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            text.transform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        elapsed = 0f;

        // Scale down
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            text.transform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        text.transform.localScale = originalScale;
    }
}
