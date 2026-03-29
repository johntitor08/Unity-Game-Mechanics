using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileUI : MonoBehaviour
{
    public static ProfileUI Instance;
    private bool statsSubscribed = false;
    private readonly int[] pendingStatIncreases = new int[6];
    private readonly Dictionary<StatType, Vector3> _originalTextScales = new();
    private readonly Dictionary<StatType, Coroutine> _pulseCoroutines = new();

    [Header("Panel")]
    public GameObject profilePanel;

    [Header("Profile Texts")]
    public TextMeshProUGUI playerNameText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI currencyText;
    public TextMeshProUGUI expText;
    public Slider expBar;
    public TextMeshProUGUI statPointsText;

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

    [Header("Stats Buttons")]
    public Button strengthIncrementButton;
    public Button intelligenceIncrementButton;
    public Button charismaIncrementButton;
    public Button defenseIncrementButton;
    public Button speedIncrementButton;
    public Button luckIncrementButton;
    public Button strengthDecrementButton;
    public Button intelligenceDecrementButton;
    public Button charismaDecrementButton;
    public Button defenseDecrementButton;
    public Button speedDecrementButton;
    public Button luckDecrementButton;

    [Header("Stat Button Sprites")]
    public Sprite incrementButtonDefault;
    public Sprite incrementButtonAvailable;

    [Header("Equipment Slots")]
    public Button slot1; public Image slot1Icon;
    public Button slot2; public Image slot2Icon;
    public Button slot3; public Image slot3Icon;
    public Button slot4; public Image slot4Icon;
    public Button slot5; public Image slot5Icon;
    public Button slot6; public Image slot6Icon;

    [Header("Rarity Border Sprites")]
    public Sprite commonBorder;
    public Sprite rareBorder;
    public Sprite epicBorder;
    public Sprite legendaryBorder;
    public Sprite godlyBorder;
    public Sprite emptyBorder;

    [Header("Settings")]
    public KeyCode toggleKey = KeyCode.P;

    [Header("Database")]
    public ProfileIconDatabase iconDatabase;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnProfileChanged -= RefreshProfile;
            ProfileManager.Instance.OnProfileChanged += RefreshProfile;
            ProfileManager.Instance.OnCurrencyChanged -= RefreshProfile;
            ProfileManager.Instance.OnCurrencyChanged += RefreshProfile;
        }
        else
        {
            ProfileManager.OnReady -= OnProfileManagerReady;
            ProfileManager.OnReady += OnProfileManagerReady;
        }

        if (EquipmentManager.Instance != null)
        {
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshEquipmentSlots;
            EquipmentManager.Instance.OnEquipmentChanged += RefreshEquipmentSlots;
        }
        else
            EquipmentManager.OnReady += OnEquipmentManagerReady;

        SetupEquipmentSlots();

        if (PlayerStats.Instance != null)
            SubscribeToStats();
        else
            PlayerStats.OnReady += SubscribeToStats;

        WireStatButtons();
        RefreshAll();
    }

    void OnDisable()
    {
        if (ProfileManager.Instance != null)
        {
            ProfileManager.Instance.OnProfileChanged -= RefreshProfile;
            ProfileManager.Instance.OnCurrencyChanged -= RefreshProfile;
        }

        ProfileManager.OnReady -= OnProfileManagerReady;

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= RefreshEquipmentSlots;

        EquipmentManager.OnReady -= OnEquipmentManagerReady;

        if (statsSubscribed && PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnHealthChanged -= UpdateHealthBar;
            PlayerStats.Instance.OnEnergyChanged -= UpdateEnergyBar;
            PlayerStats.Instance.OnStatChanged -= OnStatChanged;
            statsSubscribed = false;
        }

        PlayerStats.OnReady -= SubscribeToStats;

        foreach (var kvp in _pulseCoroutines)
            if (kvp.Value != null)
                StopCoroutine(kvp.Value);

        foreach (var kvp in _originalTextScales)
        {
            var t = GetTextForStat(kvp.Key);

            if (t != null)
                t.transform.localScale = kvp.Value;
        }

        _pulseCoroutines.Clear();
        System.Array.Clear(pendingStatIncreases, 0, pendingStatIncreases.Length);
    }

    void Update()
    {
        if (Input.GetKeyDown(toggleKey) && profilePanel != null)
        {
            profilePanel.SetActive(!profilePanel.activeSelf);

            if (profilePanel.activeSelf)
                RefreshAll();
        }
    }

    (Button btn, Image icon, EquipmentSlot slot)[] SlotDefs() => new[]
    {
        (slot1, slot1Icon, EquipmentSlot.Weapon),
        (slot2, slot2Icon, EquipmentSlot.Armor),
        (slot3, slot3Icon, EquipmentSlot.Helmet),
        (slot4, slot4Icon, EquipmentSlot.Accessory),
        (slot5, slot5Icon, EquipmentSlot.Shield),
        (slot6, slot6Icon, EquipmentSlot.Boots),
    };

    void SetupEquipmentSlots()
    {
        foreach (var (btn, _, slotType) in SlotDefs())
        {
            if (btn == null)
                continue;

            EquipmentSlot captured = slotType;
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnSlotClicked(captured));
        }

        RefreshEquipmentSlots();
    }

    void OnSlotClicked(EquipmentSlot slotType)
    {
        if (EquipmentManager.Instance == null || EquipmentInfoPanel.Instance == null)
            return;

        EquipmentInstance inst = EquipmentManager.Instance.GetEquipped(slotType);

        if (inst == null)
            return;

        EquipmentInfoPanel.Instance.ShowPanel(inst);
    }

    public void RefreshEquipmentSlots()
    {
        if (EquipmentManager.Instance == null)
            return;

        foreach (var (btn, icon, slotType) in SlotDefs())
        {
            if (btn == null)
                continue;

            EquipmentInstance inst = EquipmentManager.Instance.GetEquipped(slotType);
            bool filled = inst != null;
            var data = inst?.baseData;

            if (icon != null)
            {
                icon.sprite = filled ? data.icon : null;
                icon.enabled = filled;
            }

            if (btn.TryGetComponent<Image>(out var slotImg))
            {
                slotImg.sprite = filled ? GetRarityBorder(data.rarity) : emptyBorder;
                slotImg.color = Color.white;
            }

            var clickable = btn.GetComponent<ClickableIcon>();

            if (filled)
            {
                if (clickable == null)
                    clickable = btn.gameObject.AddComponent<ClickableIcon>();
            }
            else
            {
                if (clickable != null)
                    Destroy(clickable);
            }

            btn.interactable = filled;
        }

        UpdateHealthBar();
    }

    Sprite GetRarityBorder(Rarity rarity) => rarity switch
    {
        Rarity.Common => commonBorder,
        Rarity.Rare => rareBorder,
        Rarity.Epic => epicBorder,
        Rarity.Legendary => legendaryBorder,
        Rarity.Godly => godlyBorder,
        _ => emptyBorder
    };

    void SubscribeToStats()
    {
        if (PlayerStats.Instance == null || statsSubscribed)
            return;

        PlayerStats.Instance.OnHealthChanged += UpdateHealthBar;
        PlayerStats.Instance.OnEnergyChanged += UpdateEnergyBar;
        PlayerStats.Instance.OnStatChanged += OnStatChanged;
        statsSubscribed = true;
        RefreshAll();
    }

    void OnProfileManagerReady()
    {
        ProfileManager.OnReady -= OnProfileManagerReady;
        ProfileManager.Instance.OnProfileChanged -= RefreshProfile;
        ProfileManager.Instance.OnProfileChanged += RefreshProfile;
        ProfileManager.Instance.OnCurrencyChanged -= RefreshProfile;
        ProfileManager.Instance.OnCurrencyChanged += RefreshProfile;
        RefreshAll();
    }

    void OnEquipmentManagerReady()
    {
        EquipmentManager.OnReady -= OnEquipmentManagerReady;
        EquipmentManager.Instance.OnEquipmentChanged -= RefreshEquipmentSlots;
        EquipmentManager.Instance.OnEquipmentChanged += RefreshEquipmentSlots;
        SetupEquipmentSlots();
    }

    void WireStatButtons()
    {
        WireIncrDec(strengthIncrementButton, strengthDecrementButton, StatType.Strength, 0);
        WireIncrDec(intelligenceIncrementButton, intelligenceDecrementButton, StatType.Intelligence, 1);
        WireIncrDec(charismaIncrementButton, charismaDecrementButton, StatType.Charisma, 2);
        WireIncrDec(defenseIncrementButton, defenseDecrementButton, StatType.Defense, 3);
        WireIncrDec(speedIncrementButton, speedDecrementButton, StatType.Speed, 4);
        WireIncrDec(luckIncrementButton, luckDecrementButton, StatType.Luck, 5);
        RefreshStatButtons();
    }

    void WireIncrDec(Button incr, Button decr, StatType type, int index)
    {
        if (incr != null)
        {
            incr.onClick.RemoveAllListeners(); incr.onClick.AddListener(() => IncrementStat(type, index));
        }

        if (decr != null)
        {
            decr.onClick.RemoveAllListeners(); decr.onClick.AddListener(() => DecrementStat(type, index));
        }

    }

    void IncrementStat(StatType type, int index)
    {
        if (ProfileManager.Instance == null)
            return;

        var profile = ProfileManager.Instance.profile;

        if (profile == null || profile.statPoints <= 0 || PlayerStats.Instance == null)
            return;

        PlayerStats.Instance.Modify(type, 1);
        profile.statPoints--;
        pendingStatIncreases[index]++;
        RefreshStatButtons();
        RefreshStatPointsText();
        SaveSystem.SaveGame();
    }

    void DecrementStat(StatType type, int index)
    {
        if (pendingStatIncreases[index] <= 0 || PlayerStats.Instance == null)
            return;

        PlayerStats.Instance.Modify(type, -1);

        if (ProfileManager.Instance != null)
            ProfileManager.Instance.profile.statPoints++;

        pendingStatIncreases[index]--;
        RefreshStatButtons();
        RefreshStatPointsText();
        SaveSystem.SaveGame();
    }

    void RefreshStatButtons()
    {
        if (ProfileManager.Instance == null)
            return;

        int pts = ProfileManager.Instance.profile?.statPoints ?? 0;
        bool can = pts > 0;
        SetIncrementButton(strengthIncrementButton, can);
        SetIncrementButton(intelligenceIncrementButton, can);
        SetIncrementButton(charismaIncrementButton, can);
        SetIncrementButton(defenseIncrementButton, can);
        SetIncrementButton(speedIncrementButton, can);
        SetIncrementButton(luckIncrementButton, can);
        SetButtonInteractable(strengthDecrementButton, pendingStatIncreases[0] > 0);
        SetButtonInteractable(intelligenceDecrementButton, pendingStatIncreases[1] > 0);
        SetButtonInteractable(charismaDecrementButton, pendingStatIncreases[2] > 0);
        SetButtonInteractable(defenseDecrementButton, pendingStatIncreases[3] > 0);
        SetButtonInteractable(speedDecrementButton, pendingStatIncreases[4] > 0);
        SetButtonInteractable(luckDecrementButton, pendingStatIncreases[5] > 0);
    }

    void SetIncrementButton(Button btn, bool canIncrement)
    {
        if (btn == null)
            return;

        btn.interactable = canIncrement;

        if (btn.TryGetComponent<Image>(out var img))
            img.sprite = canIncrement && incrementButtonAvailable != null ? incrementButtonAvailable : incrementButtonDefault;
    }

    static void SetButtonInteractable(Button btn, bool state)
    {
        if (btn != null)
            btn.interactable = state;
    }

    public void RefreshAll()
    {
        RefreshProfile();
        UpdateHealthBar();
        UpdateEnergyBar();
        RefreshAllStats();
        RefreshEquipmentSlots();
    }

    public void RefreshProfile(PlayerProfile profile = null)
    {
        if (ProfileManager.Instance == null)
            return;

        profile ??= ProfileManager.Instance.profile;

        if (profile == null)
            return;

        playerNameText.text = profile.playerName;
        levelText.text = $"Level {profile.level}";
        int gold = CurrencyManager.Instance != null ? CurrencyManager.Instance.Get(CurrencyType.Gold) : 0;
        currencyText.text = $"{gold} Gold";
        expBar.maxValue = Mathf.Max(1, profile.experienceToNextLevel);
        expBar.value = Mathf.Clamp(profile.experience, 0, profile.experienceToNextLevel);
        expText.text = $"{profile.experience} / {profile.experienceToNextLevel} XP";

        if (profileIcon != null && iconDatabase != null)
        {
            Sprite icon = iconDatabase.GetIconSprite(profile.profileIconID);

            if (icon != null)
                profileIcon.sprite = icon;
        }

        RefreshStatPointsText();
        RefreshStatButtons();
    }

    void RefreshStatPointsText()
    {
        if (statPointsText == null || ProfileManager.Instance == null)
            return;

        int pts = ProfileManager.Instance.profile?.statPoints ?? 0;
        statPointsText.text = pts > 0 ? $"Stat Points: {pts}" : "Stat Points: 0";
        statPointsText.color = pts > 0 ? Color.yellow : Color.white;
    }

    void RefreshAllStats()
    {
        if (PlayerStats.Instance == null)
            return;

        strengthText.text = $"Strength: {PlayerStats.Instance.Get(StatType.Strength)}";
        intelligenceText.text = $"Intelligence: {PlayerStats.Instance.Get(StatType.Intelligence)}";
        charismaText.text = $"Charisma: {PlayerStats.Instance.Get(StatType.Charisma)}";
        defenseText.text = $"Defense: {PlayerStats.Instance.Get(StatType.Defense)}";
        speedText.text = $"Speed: {PlayerStats.Instance.Get(StatType.Speed)}";
        luckText.text = $"Luck: {PlayerStats.Instance.Get(StatType.Luck)}";
    }

    void OnStatChanged(StatType type, int oldValue, int newValue)
    {
        if (PlayerStats.Instance == null)
            return;

        int val = PlayerStats.Instance.Get(type);

        switch (type)
        {
            case StatType.Strength:
                strengthText.text = $"Strength: {val}";
                break;

            case StatType.Intelligence:
                intelligenceText.text = $"Intelligence: {val}";
                break;

            case StatType.Charisma:
                charismaText.text = $"Charisma: {val}";
                break;

            case StatType.Defense:
                defenseText.text = $"Defense: {val}";
                break;

            case StatType.Speed:
                speedText.text = $"Speed: {val}";
                break;

            case StatType.Luck:
                luckText.text = $"Luck: {val}";
                break;
        }

        if (type == StatType.Health || type == StatType.Energy)
            return;

        if (_pulseCoroutines.TryGetValue(type, out var existing) && existing != null)
            StopCoroutine(existing);

        _pulseCoroutines[type] = StartCoroutine(PulseText(type, GetTextForStat(type)));
    }

    TextMeshProUGUI GetTextForStat(StatType type) => type switch
    {
        StatType.Strength => strengthText,
        StatType.Intelligence => intelligenceText,
        StatType.Charisma => charismaText,
        StatType.Defense => defenseText,
        StatType.Speed => speedText,
        StatType.Luck => luckText,
        _ => null
    };

    void UpdateHealthBar()
    {
        if (PlayerStats.Instance == null)
            return;

        int cur = PlayerStats.Instance.Get(StatType.Health);
        int max = PlayerStats.Instance.Get(StatType.MaxHealth);
        float pct = PlayerStats.Instance.GetHealthPercentage();

        if (healthBar != null)
        {
            healthBar.maxValue = max;
            healthBar.value = cur;
        }

        if (healthText != null)
            healthText.text = $"Health: {cur} / {max}";

        if (healthFillImage != null)
            healthFillImage.color = pct > 0.6f ? healthHighColor : pct > 0.3f ? healthMediumColor : healthLowColor;
    }

    void UpdateEnergyBar()
    {
        if (PlayerStats.Instance == null)
            return;

        int cur = PlayerStats.Instance.Get(StatType.Energy);
        int max = PlayerStats.Instance.Get(StatType.MaxEnergy);

        if (energyBar != null)
        {
            energyBar.maxValue = max;
            energyBar.value = cur;
        }

        if (energyText != null)
            energyText.text = $"Energy: {cur} / {max}";

        if (energyFillImage != null)
            energyFillImage.color = energyColor;
    }

    IEnumerator PulseText(StatType type, TextMeshProUGUI text)
    {
        if (text == null)
            yield break;

        if (!_originalTextScales.ContainsKey(type))
            _originalTextScales[type] = text.transform.localScale;

        Vector3 original = _originalTextScales[type];
        Vector3 target = original * 1.2f;
        float dur = 0.2f, t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime; text.transform.localScale = Vector3.Lerp(original, target, t / dur);
            yield return null;
        }

        t = 0f;

        while (t < dur)
        {
            t += Time.deltaTime; text.transform.localScale = Vector3.Lerp(target, original, t / dur);
            yield return null;
        }

        text.transform.localScale = original;
    }
}
