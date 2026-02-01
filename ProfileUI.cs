using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;

public class ProfileUI : MonoBehaviour
{
    public static ProfileUI Instance;
    private bool statsSubscribed = false;

    [Header("Panel")]
    public GameObject profilePanel;

    [Header("Profile Texts")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI expText;
    public Slider expBar;

    [Header("Profile Icon")]
    public Image profileIcon;

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
    public Color energyColor = new(0.3f, 0.5f, 1f);

    [Header("Stats Display")]
    public TextMeshProUGUI strengthText;
    public TextMeshProUGUI intelligenceText;
    public TextMeshProUGUI charismaText;
    public TextMeshProUGUI defenseText;
    public TextMeshProUGUI speedText;
    public TextMeshProUGUI luckText;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.P;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnEnable()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnProfileChanged += RefreshProfile;
            ProfileManager.Instance.OnCurrencyChanged += RefreshProfile;
        }

        RefreshAll();

        if (PlayerStats.Instance != null)
        {
            SubscribeToStats();
        }
        else
        {
            PlayerStats.OnReady += SubscribeToStats;
        }
    }

    private void OnDisable()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnProfileChanged -= RefreshProfile;
            ProfileManager.Instance.OnCurrencyChanged -= RefreshProfile;
        }

        if (statsSubscribed && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnHealthChanged -= UpdateHealthBar;
            PlayerStats.Instance.OnEnergyChanged -= UpdateEnergyBar;
            PlayerStats.Instance.OnStatChanged -= OnStatChanged;
            statsSubscribed = false;
        }

        PlayerStats.OnReady -= SubscribeToStats;
    }

    private void Update()
    {
        if (Input.GetKeyDown(toggleKey) && profilePanel != null)
        {
            profilePanel.SetActive(!profilePanel.activeSelf);

            if (profilePanel.activeSelf)
                RefreshAll();
        }
    }

    private void SubscribeToStats()
    {
        if (PlayerStats.Instance != null && !statsSubscribed)
        {
            PlayerStats.Instance.OnHealthChanged += UpdateHealthBar;
            PlayerStats.Instance.OnEnergyChanged += UpdateEnergyBar;
            PlayerStats.Instance.OnStatChanged += OnStatChanged;
            statsSubscribed = true;
            RefreshAll();
        }
    }

    public void RefreshProfile(PlayerProfile profile = null)
    {
        profile ??= ProfileManager.Instance != null ? ProfileManager.Instance.profile : null;
        if (profile == null) return;
        playerNameText.text = profile.playerName;
        levelText.text = $"Level {profile.level}";
        currencyText.text = $"{profile.currency} Gold";
        expBar.maxValue = Mathf.Max(1, profile.experienceToNextLevel);
        expBar.value = Mathf.Clamp(profile.experience, 0, profile.experienceToNextLevel);
        expText.text = $"{profile.experience} / {profile.experienceToNextLevel} XP";

        if (profileIcon != null)
            profileIcon.sprite = ProfileIconDatabase.GetSprite(profile.profileIconID);
    }

    public void RefreshAll()
    {
        RefreshProfile();
        UpdateHealthBar();
        UpdateEnergyBar();
        RefreshAllStats();
    }

    private void RefreshAllStats()
    {
        if (PlayerStats.Instance == null) return;
        strengthText.text = $"Strength: {PlayerStats.Instance.Get(StatType.Strength)}";
        intelligenceText.text = $"Intelligence: {PlayerStats.Instance.Get(StatType.Intelligence)}";
        charismaText.text = $"Charisma: {PlayerStats.Instance.Get(StatType.Charisma)}";
        defenseText.text = $"Defense: {PlayerStats.Instance.Get(StatType.Defense)}";
        speedText.text = $"Speed: {PlayerStats.Instance.Get(StatType.Speed)}";
        luckText.text = $"Luck: {PlayerStats.Instance.Get(StatType.Luck)}";
    }

    private void OnStatChanged(StatType type, int oldValue, int newValue)
    {
        int displayValue = PlayerStats.Instance != null ? PlayerStats.Instance.Get(type) : 0;

        switch (type)
        {
            case StatType.Strength: strengthText.text = $"Strength: {displayValue}"; break;
            case StatType.Intelligence: intelligenceText.text = $"Intelligence: {displayValue}"; break;
            case StatType.Charisma: charismaText.text = $"Charisma: {displayValue}"; break;
            case StatType.Defense: defenseText.text = $"Defense: {displayValue}"; break;
            case StatType.Speed: speedText.text = $"Speed: {displayValue}"; break;
            case StatType.Luck: luckText.text = $"Luck: {displayValue}"; break;
        }

        if (type != StatType.Health && type != StatType.Energy)
            StartCoroutine(PulseText(GetTextForStat(type)));
    }

    private TextMeshProUGUI GetTextForStat(StatType type)
    {
        return type switch
        {
            StatType.Strength => strengthText,
            StatType.Intelligence => intelligenceText,
            StatType.Charisma => charismaText,
            StatType.Defense => defenseText,
            StatType.Speed => speedText,
            StatType.Luck => luckText,
            _ => null
        };
    }

    private void UpdateHealthBar()
    {
        if (PlayerStats.Instance == null) return;
        int currentHealth = PlayerStats.Instance.Get(StatType.Health);
        int maxHealth = PlayerStats.Instance.Get(StatType.MaxHealth);
        float percentage = PlayerStats.Instance.GetHealthPercentage();

        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }

        if (healthText != null)
            healthText.text = $"Health: {currentHealth} / {maxHealth}";

        if (healthFillImage != null)
        {
            if (percentage > 0.6f) healthFillImage.color = healthHighColor;
            else if (percentage > 0.3f) healthFillImage.color = healthMediumColor;
            else healthFillImage.color = healthLowColor;
        }
    }

    private void UpdateEnergyBar()
    {
        if (PlayerStats.Instance == null) return;
        int currentEnergy = PlayerStats.Instance.Get(StatType.Energy);
        int maxEnergy = PlayerStats.Instance.Get(StatType.MaxEnergy);

        if (energyBar != null)
        {
            energyBar.maxValue = maxEnergy;
            energyBar.value = currentEnergy;
        }

        if (energyText != null)
            energyText.text = $"Energy: {currentEnergy} / {maxEnergy}";

        if (energyFillImage != null)
            energyFillImage.color = energyColor;
    }

    private IEnumerator PulseText(TextMeshProUGUI text)
    {
        if (text == null) yield break;
        Vector3 original = text.transform.localScale;
        Vector3 target = original * 1.2f;
        float duration = 0.2f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(original, target, elapsed / duration);
            yield return null;
        }

        elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            text.transform.localScale = Vector3.Lerp(target, original, elapsed / duration);
            yield return null;
        }

        text.transform.localScale = original;
    }
}
