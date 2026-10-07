using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum SceneProgress
{
    Scene1,
    Scene2,
    Scene3,
    Scene4,
    Scene5,
    Scene6,
    Scene7,
    Scene8,
    Scene9,
    SceneHome,
    SceneMarket,
    SceneGym,
    SceneOffice,
    SceneChurch
}

public class SceneEvent : MonoBehaviour, IDialoguePanelAnimator
{
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime0_6 = new(0.6f);
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime0_12 = new(0.12f);
    private static readonly WaitForSecondsRealtime _bgSwapPause = new(0.2f);
    public static SceneEvent Instance { get; private set; }
    private static readonly int DialoguePanelCloseHash = Animator.StringToHash("DialoguePanelClose");
    private SceneProgress progress = SceneProgress.Scene1;
    private int currentMapIndex;
    private int _validMapCursor;
    private static readonly int[] validMapIndices = { 6, 8, 13 };
    private bool isDialogueSubscribed;
    private bool isHoverEffectsSubscribed;
    private readonly List<(UIHoverRegion hover, Action handler)> _hoverClickActions = new();
    private Action<EnemyData> _currentVictoryHandler;
    private Action _currentDefeatHandler;
    private Action _currentFleeHandler;
    private int _lastBgIndex = -1;
    private int _lastCharIndex = -1;
    private string _currentQuestLocation;
    private bool dayScenarioPending;
    private float _transitionBusySince = -1f;
    private bool _sceneCharacterActive;
    private bool _doorClicked;
    private TimePhase _lastObservedPhase = TimePhase.Morning;
    private float[] _itemDefaultY;
    private Vector2[] _mapIconDefaultPos;
    private Vector2 charRtAnchoredTransform;
    private Vector2 charRtSizeDelta;
    private Coroutine charFadeCoroutine;
    private bool _charFadingOut;
    private bool _charImageFromDialogue;
    private Sprite _dialogueBgCurrent;
    private Sprite _dialogueBgPending;
    public event Action<int> OnBackgroundChanged;
    public ItemDatabase itemDatabase;
    public TMP_Text mapTitleText;
    public GameObject townNpc;
    private Coroutine _hudRoutine;
    public bool IsHudReturning => _hudRoutine != null;
    private const float HudRevealDelay = 0.3f;
    private bool _wasInCombat;
    private bool _keepingSpeaker;
    public bool IsSleeping => sleepingPanel != null && sleepingPanel.activeSelf;
    public bool IsShowingQuestLocation => !string.IsNullOrEmpty(_currentQuestLocation);
    public bool IsInTownSquare => _lastBgIndex == 0 && !IsShowingQuestLocation;

    public bool IsInNightOnlyRoom => _lastBgIndex == 7 || _lastBgIndex == 42;
    public bool IsDayStoryWaiting => (dayScenarioPending && TimeUI.Instance != null && TimeUI.Instance.GetCurrentDay() >= 2 && HasUnplayedDayScenario()) || (ScenarioManager.Instance != null && ScenarioManager.Instance.IsWaitingAfterRetreat);
    public bool IsSceneDarkened => _transitionDepth > 0 || (_sceneFade != null && _sceneFade.gameObject.activeSelf && _sceneFade.color.a > 0.05f);
    private const float SceneDipOut = 0.35f;
    private const float SceneDipIn = 0.4f;
    private Image _sceneFade;
    private Coroutine _sceneFadeRoutine;
    private Image _holdFrame;
    private Sprite _holdSprite;
    private int _transitionDepth;
    public bool IsHoldingScene => _holdFrame != null && _holdFrame.gameObject.activeSelf;
    private const int BedroomBackground = 13;
    private static readonly string[] LastDayQuests = { "q10_acik_hesap", "q11_fincan_basinda" };
    public const string EndingDoorHeld = "ending_door_held";
    public const string EndingOpenAccount = "ending_open_account";
    public const string EndingTwoInTheDoor = "ending_two_in_the_door";
    private const string SharedEpilogueScenario = "ashenveil_red_saint";
    private const string SharedEpilogueStartFlag = "red_saint_start";

    public bool DayScenarioPending
    {
        get => dayScenarioPending;
        set => dayScenarioPending = value;
    }

    public SceneProgress Progress
    {
        get => progress;

        private set
        {
            if (progress == value)
                return;

            progress = value;
            SyncTimePhaseToScene(value);
            SaveSystem.SaveGame();
        }
    }

    [System.Serializable]
    public struct BackgroundEntry
    {
        public string name;
        public Sprite morning;
        public Sprite noon;
        public Sprite evening;
        public Sprite night;

        public readonly Sprite Resolve(TimePhase phase)
        {
            Sprite s = phase switch
            {
                TimePhase.Morning => morning,
                TimePhase.Noon => noon,
                TimePhase.Evening => evening,
                TimePhase.Night => night,
                _ => morning
            };

            return s != null ? s : morning != null ? morning : night;
        }
    }

    [System.Serializable]
    public struct CharacterEntry
    {
        public string name;
        public Sprite morning;
        public Sprite noon;
        public Sprite evening;
        public Sprite night;

        public readonly Sprite Resolve(TimePhase phase)
        {
            Sprite s = phase switch
            {
                TimePhase.Morning => morning,
                TimePhase.Noon => noon,
                TimePhase.Evening => evening,
                TimePhase.Night => night,
                _ => morning
            };

            return s != null ? s : morning;
        }
    }

    [System.Serializable]
    public struct NamedBackground
    {
        public string name;
        public Sprite sprite;
        public Sprite noonSprite;
        public Sprite eveningSprite;
        public Sprite nightSprite;

        public readonly Sprite Resolve(TimePhase phase)
        {
            if (phase == TimePhase.Noon && noonSprite != null)
                return noonSprite;

            if (phase == TimePhase.Night && nightSprite != null)
                return nightSprite;

            bool dusk = phase == TimePhase.Evening || phase == TimePhase.Night;
            return dusk && eveningSprite != null ? eveningSprite : sprite;
        }
    }

    public enum PhaseCondition { Any, MorningOrNoon, NotNight, NightOnly, NoonOrEvening, MorningOnly, EveningOrNight }

    public enum MapGroup { Modern, Fantasy, Combat }

    public enum HoverAction
    {
        None,
        EnterTown,
        OpenMarket,
        OpenChurch,
        OpenHome,
        OpenOffice,
        OpenGym,
        SleepToNextDay,
        ShowGardenHole,
        ShowPool,
        CollectItem,
        OpenStove,
        QuestInteract,
        QuestTalk,
        QuestCombat,
        GoToQuestLocation,
        ReturnToTown
    }

    [System.Serializable]
    public struct HoverRegionEntry
    {
        public string name;
        public GameObject region;
        public int[] visibleOnBackgrounds;
        public PhaseCondition phase;
        public HoverAction action;
        public int worldItemIndex;
        public string questObjectiveTag;
        public int questProgressAmount;
        public string questLocationName;
        public EnemyData questEnemy;
        public ItemData questGrantItem;
        public string questID;
        public DialogueNode questDialogueNode;
        public string hideIfFlag;
    }

    [System.Serializable]
    public struct WorldItemEntry
    {
        public string name;
        public GameObject item;
        public int[] visibleOnBackgrounds;
        public string questLocationName;
    }

    [System.Serializable]
    public struct MapLocationEntry
    {
        public string name;
        public GameObject icon;
        public MapGroup group;
        public PhaseCondition interactablePhase;
        public Vector2 interactableOffset;

        [Header("Quest travel")]
        public string questID;
        public string questObjectiveID;
        public string travelToLocation;
    }

    [System.Serializable]
    public struct RoomIconEntry
    {
        public string name;
        public GameObject icon;
        public int background;
    }

    [Header("Backgrounds")]
    public Image backgroundImage;
    public BackgroundEntry[] backgrounds;

    [Header("Quest Location Backgrounds")]
    public NamedBackground[] questLocationBackgrounds;

    [Header("Characters")]
    public Image charImage;
    public CharacterEntry[] characters;

    [Header("Interactive Regions")]
    public HoverRegionEntry[] hoverRegions;
    public WorldItemEntry[] worldItems;

    [Header("Dialogue Character Layout")]
    public Vector2 dialogueCharacterPosition = new(0f, 0f);
    public Vector2 dialogueCharacterSize = new(700f, 700f);

    [System.Serializable]
    public struct CharacterLayoutOverride
    {
        [Tooltip("Name of the dialogue background sprite this layout applies to.")]
        public string backgroundName;
        public Vector2 position;
        public Vector2 size;
    }

    static readonly string[] HalfBodyCharacterSprites = { "char_mireya_00001_" };

    static readonly CharacterLayoutOverride[] DialogueCharacterLayoutOverrides =
    {
        new() { backgroundName = "ComfyUI_01231_", position = new Vector2(0f, -260f), size = new Vector2(1000f, 1000f) },
        new() { backgroundName = "ComfyUI_01241_", position = new Vector2(0f, -260f), size = new Vector2(1000f, 1000f) },
        new() { backgroundName = "bg26_morning", position = new Vector2(250f, -260f), size = new Vector2(1000f, 1000f) },
    };

    [Header("Dialogues")]
    public DialogueNode[] sceneStartDialogueNodes;
    public DialogueNode scene9SecondNode;
    public DialogueNode scene9FifthNode;
    public DialogueNode finaleCutsceneNode;
    public DialogueNode marenTeaDialogue;
    public DialogueNode marenDeliveryNode;

    [Header("Enemies")]
    public EnemyData[] enemies;

    [Header("UI Maps")]
    public Image mapImage;
    public Image combatMapImage;
    public Sprite[] maps;

    [Header("UI Panels")]
    public GameObject settingsIconPanel;
    public GameObject iconPanel;
    public GameObject timePanel;
    public GameObject settingsPanel;
    public GameObject savePanel;
    public GameObject minigameLauncherPanel;
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject mapPanel;
    public GameObject combatMapPanel;
    public GameObject equipmentPanel;
    public GameObject coinPanel;
    public GameObject questPanel;
    public GameObject houseIconsPanel;
    public GameObject sleepingPanel;
    public GameObject diversionsPanel;

    [Header("Sleep Rules")]
    public TimePhase earliestSleepPhase = TimePhase.Evening;

    [Header("UI Icons")]
    public GameObject settingsIcon;
    public GameObject profileIcon;
    public GameObject inventoryIcon;
    public GameObject mapIcon;
    public GameObject coinIcon;
    public GameObject questIcon;
    public GameObject combatIcon;
    public GameObject diversionsIcon;

    [Header("Map Locations & Room Icons")]
    public MapLocationEntry[] mapLocations;
    public RoomIconEntry[] roomIcons;

    [Header("UI Buttons")]
    public GameObject closeMapButton;
    public GameObject closeCombatMapButton;

    [Header("Animation")]
    public Animator timePanelAnimator;
    public Animator iconPanelAnimator;
    public Animator minigameLauncherPanelAnimator;
    public Animator dialoguePanelAnimator;
    public string timePanelOpenTrigger = "TimePanelOpened";
    public string timePanelCloseTrigger = "TimePanelClosed";
    public string iconPanelOpenTrigger = "IconPanelOpened";
    public string iconPanelCloseTrigger = "IconPanelClosed";
    public string dialoguePanelOpenTrigger = "DialoguePanelOpened";
    public string dialoguePanelCloseTrigger = "DialoguePanelClosed";
    public string minigameLauncherPanelOpenTrigger = "MinigameLauncherPanelOpened";
    public string minigameLauncherPanelCloseTrigger = "MinigameLauncherPanelClosed";
    public float dialogueCloseFallbackDuration = 0.25f;

    [Header("Icon Settings")]
    public bool closeOtherPanelsOnOpen = true;
    public bool allowMultiplePanels = false;

    private static class WorldItemIndex
    {
        public const int AppleTeaSeed = 0;
        public const int HistoryBook = 1;
        public const int Apple = 2;
        public const int Cinnamon = 3;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (itemDatabase != null)
            itemDatabase.SetInstance();

        if (mapLocations != null)
        {
            _mapIconDefaultPos = new Vector2[mapLocations.Length];

            for (int i = 0; i < mapLocations.Length; i++)
                if (mapLocations[i].icon != null && mapLocations[i].icon.TryGetComponent<RectTransform>(out var iconRt))
                    _mapIconDefaultPos[i] = iconRt.anchoredPosition;
        }

        if (worldItems != null)
        {
            _itemDefaultY = new float[worldItems.Length];

            for (int i = 0; i < worldItems.Length; i++)
            {
                if (worldItems[i].name == "HistoryBook" && worldItems[i].item != null && worldItems[i].item.TryGetComponent<RectTransform>(out var rt))
                    _itemDefaultY[i] = rt.anchoredPosition.y;
            }
        }
    }

    void Start()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.PanelAnimator = this;

        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;

        if (!SaveSystem.IsLoading)
        {
            SetBackground(0);
            SetCharacter(0);
        }

        currentMapIndex = validMapIndices[0];
        SetupIconButtons();
        HideAllPanels();
        SubscribeDialogue();
        SubscribeHoverEffects();
        SetActive(settingsIconPanel, false);
        SetActive(timePanel, false);
        SetActive(iconPanel, false);
        SetActive(minigameLauncherPanel, false);

        if (charImage != null)
        {
            charImage.preserveAspect = true;
            charImage.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            TogglePanel(mapPanel, "Map");

        GuardSceneFade();
        WatchCombatEnd();
    }

    void GuardSceneFade()
    {
        bool idle = _sceneFadeRoutine == null && charFadeCoroutine == null;

        if (idle && !IsHoldingScene && _transitionDepth == 0 && _sceneFade != null && _sceneFade.gameObject.activeSelf && _sceneFade.color.a > 0f)
            StartCoroutine(RunSceneFade(0f, SceneDipIn));

        if (_transitionDepth > 0)
        {
            if (_transitionBusySince < 0f)
                _transitionBusySince = Time.unscaledTime;
            else if (Time.unscaledTime - _transitionBusySince > 6f)
            {
                _transitionDepth = 0;

                if (_holdFrame != null)
                    _holdFrame.gameObject.SetActive(false);
            }
        }
        else
        {
            _transitionBusySince = -1f;
        }
    }

    void OnEnable()
    {
        SubscribeDialogue();
        SubscribeHoverEffects();

        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged += OnPhaseChanged;

        if (ScenarioManager.Instance != null)
        {
            ScenarioManager.Instance.OnScenarioStart += OnScenarioStarted;
            ScenarioManager.Instance.OnScenarioComplete += OnScenarioCompleted;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemUsed += HandleItemUsed;
    }

    void OnDisable()
    {
        UnsubscribeDialogue();
        UnsubscribeHoverEffects();
        _hudRoutine = null;

        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;

        if (ScenarioManager.Instance != null)
        {
            ScenarioManager.Instance.OnScenarioStart -= OnScenarioStarted;
            ScenarioManager.Instance.OnScenarioComplete -= OnScenarioCompleted;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnItemUsed -= HandleItemUsed;
    }

    void SetupIconButtons()
    {
        if (settingsIcon != null)
            settingsIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(settingsPanel, "Settings"));

        if (profileIcon != null)
            profileIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(profilePanel, "Profile"));

        if (inventoryIcon != null)
            inventoryIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(inventoryPanel, "Inventory"));

        if (mapIcon != null)
            mapIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(mapPanel, "Map"));

        if (coinIcon != null)
            coinIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(coinPanel, "Coin"));

        if (questIcon != null)
            questIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(questPanel, "Quest"));

        if (diversionsIcon != null)
            diversionsIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(diversionsPanel, "Diversions"));

        if (combatIcon != null)
            combatIcon.GetComponentInChildren<Button>().onClick.AddListener(() => TogglePanel(combatMapPanel, "Combat Map"));

        if (closeMapButton != null)
            closeMapButton.GetComponent<Button>().onClick.AddListener(HideAllPanels);

        if (closeCombatMapButton != null)
            closeCombatMapButton.GetComponent<Button>().onClick.AddListener(HideAllPanels);

        SetActive(combatIcon, false);

        if (mapLocations != null)
            foreach (var loc in mapLocations)
            {
                SetActive(loc.icon, false);

                if (loc.icon != null && !string.IsNullOrEmpty(loc.travelToLocation))
                {
                    Button questIconButton = loc.icon.GetComponentInChildren<Button>(true);

                    if (questIconButton != null)
                    {
                        string target = loc.travelToLocation;
                        questIconButton.onClick.AddListener(() => ShowQuestLocation(target));
                    }
                }
            }

        if (roomIcons != null)
            foreach (var room in roomIcons)
                SetActive(room.icon, false);
    }

    public void InitializeGame()
    {
        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();

        TryStartDialogue(0);
        InitializeGameContinue();
    }

    public void InitializeGameContinue()
    {
        if (ProfileUI.Instance != null)
            ProfileUI.Instance.OnGameStarted();

        if (InventoryUI.Instance != null)
            InventoryUI.Instance.OnGameStarted();

        if (EquipmentUI.Instance != null)
            EquipmentUI.Instance.OnGameStarted();

        if (MarketUI.Instance != null)
            MarketUI.Instance.OnGameStarted();

        if (CurrencyUI.Instance != null)
            CurrencyUI.Instance.OnGameStarted();
    }

    void OpenPanel(GameObject panel, string panelName)
    {
        if (IsInDialogue())
            return;

        if (panel == null)
        {
            Debug.LogWarning($"[SceneEvent] {panelName} panel reference is null.");
            return;
        }

        if (closeOtherPanelsOnOpen && !allowMultiplePanels)
            HideAllPanels();

        OpenAnimated(panel);
    }

    public void OpenSettings()
    {
        OpenPanel(settingsPanel, "Settings");

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenProfile()
    {
        OpenPanel(profilePanel, "Profile");

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenInventory()
    {
        OpenPanel(inventoryPanel, "Inventory");

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenShop()
    {
        if (MarketUI.Instance != null)
        {
            HideAllPanels();
            MarketUI.Instance.OpenShop();
        }
        else
        {
            OpenPanel(shopPanel, "Shop");
        }

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenMap()
    {
        OpenPanel(mapPanel, "Map");

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenEquipment()
    {
        OpenPanel(equipmentPanel, "Equipment");

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenCoin()
    {
        OpenPanel(coinPanel, "Coin");

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenQuest()
    {
        OpenPanel(questPanel, "Quest");

        if (QuestUI.Instance != null)
            QuestUI.Instance.questLogPanel.SetActive(true);

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenSave()
    {
        OpenPanel(savePanel, "Save");

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void OpenCombatMap()
    {
        OpenPanel(combatMapPanel, "Combat Map");

        if (ProfileUI.Instance != null)
            ProfileUI.Instance.RefreshAll();
    }

    public void ClosePanel(GameObject panel) => CloseAnimated(panel);

    public void HideAllPanels()
    {
        CloseAnimated(settingsPanel);
        CloseAnimated(savePanel);
        CloseAnimated(profilePanel);
        CloseAnimated(inventoryPanel);
        CloseAnimated(mapPanel);
        CloseAnimated(equipmentPanel);
        CloseAnimated(coinPanel);
        CloseAnimated(questPanel);
        CloseAnimated(combatMapPanel);
        CloseAnimated(diversionsPanel);

        if (MarketUI.Instance != null)
            MarketUI.Instance.CloseAll();
        else
            CloseAnimated(shopPanel);

        if (GuideUI.Instance != null)
            GuideUI.Instance.Close();
    }

    void TogglePanel(GameObject panel, string panelName)
    {
        if (IsInDialogue())
            return;

        if (panel == null)
        {
            Debug.LogWarning($"[SceneEvent] {panelName} panel reference is null.");
            return;
        }

        bool isActive = panel.activeSelf;

        if (closeOtherPanelsOnOpen && !allowMultiplePanels && !isActive)
            HideAllPanels();

        if (isActive)
            CloseAnimated(panel);
        else
            OpenAnimated(panel);

        if (!isActive && panel == mapPanel)
            SetMap(currentMapIndex);

        if (QuestUI.Instance != null && !isActive && panel == questPanel)
        {
            QuestUI.Instance.questLogPanel.SetActive(true);
            QuestUI.Instance.RefreshQuestLog();
        }
    }

    public void ToggleInventoryInCombat()
    {
        if (inventoryPanel == null)
            return;

        if (inventoryPanel.activeSelf)
            CloseAnimated(inventoryPanel);
        else
            OpenAnimated(inventoryPanel);
    }

    public void SetCharacter(int index)
    {
        _lastCharIndex = index;

        if (charImage == null)
            return;

        charImage.preserveAspect = true;

        if (index >= 0 && index < characters.Length)
        {
            TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;
            Sprite s = characters[index].Resolve(phase);

            if (s != null)
            {
                charImage.sprite = s;
                FitCharacterWidth();
                _sceneCharacterActive = true;
                _charImageFromDialogue = false;

                if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue())
                    charImage.gameObject.SetActive(true);
            }
        }
    }

    public void SetBackground(int index)
    {
        _lastBgIndex = index;
        _currentQuestLocation = null;

        if (backgroundImage != null && index >= 0 && index < backgrounds.Length)
            backgroundImage.sprite = ResolveBg(index);

        ClearDialogueBackground();
        SetActive(townNpc, index == 0);

        if (index == 0 && !SaveSystem.IsLoading && ScenarioManager.Instance != null && ScenarioManager.Instance.IsWaitingAfterRetreat)
            ScenarioManager.Instance.ResumeAfterRetreat();
        else if (index == 0)
            TryStartDayScenario();

        OnBackgroundChanged?.Invoke(index);
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.UpdatePhaseButtons();

        ApplyHoverVisibility(index);
        ApplyItemVisibility(index);
        ApplyHouseIconVisibility(IsHouseBackground(index));
        bool showChar = index >= 8 && index <= 12;

        if (index == 11 && (IsNight() || IsEvening()))
            showChar = false;

        _sceneCharacterActive = showChar;

        if (!DialogueManager.Instance.IsInDialogue())
            SetActive(charImage.gameObject, showChar);

        if (showChar)
        {
            RectTransform charRt = charImage.rectTransform;

            if (index == 9)
            {
                charRt.anchorMin = new Vector2(1, 0);
                charRt.anchorMax = new Vector2(1, 0);
                charRt.pivot = new Vector2(0.5f, 0.5f);
                charRt.anchoredPosition = new Vector2(-260f, 450f);
                charRt.sizeDelta = new Vector2(700f, 1200f);
                charRt.localScale = new Vector3(0.75f, 0.75f, 0.75f);
                SetCharacter(24);
            }
            else if (index == 11)
            {
                charRt.anchorMin = new Vector2(0.5f, 0);
                charRt.anchorMax = new Vector2(0.5f, 0);
                charRt.pivot = new Vector2(0.5f, 0.5f);
                charRt.anchoredPosition = new Vector2(375f, 375f);
                charRt.sizeDelta = new Vector2(75f, 75f);
                charRt.localScale = new Vector3(10f, 10f, 10f);
                SetCharacter(27);
            }
            else
            {
                charRt.anchorMin = new Vector2(0.5f, 0);
                charRt.anchorMax = new Vector2(0.5f, 0);
                charRt.pivot = new Vector2(0.5f, 0.5f);
                charRt.anchoredPosition = new Vector2(0, 375f);
                charRt.sizeDelta = new Vector2(75f, 75f);
                charRt.localScale = new Vector3(10f, 10f, 10f);

                if (index == 8)
                    SetCharacter(22);
                else if (index == 10)
                    SetCharacter(26);
                else if (index == 12)
                    SetCharacter(29);
            }
        }
    }

    public Sprite PhaseVariantOf(Sprite sprite)
    {
        if (sprite == null)
            return null;

        TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;

        if (questLocationBackgrounds != null)
            foreach (var q in questLocationBackgrounds)
                if (sprite == q.sprite || sprite == q.noonSprite || sprite == q.eveningSprite || sprite == q.nightSprite)
                    return q.Resolve(phase) ?? sprite;

        return sprite;
    }

    private Sprite ResolveBg(int index)
    {
        if (index < 0 || backgrounds == null || index >= backgrounds.Length)
            return null;

        TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;
        return backgrounds[index].Resolve(phase);
    }

    private void ClearDialogueBackground()
    {
        if (DialogueManager.Instance == null || DialogueManager.Instance.backgroundImage == null || DialogueManager.Instance.IsInDialogue() || _charFadingOut)
            return;

        DialogueManager.Instance.backgroundImage.enabled = false;
        DialogueManager.Instance.backgroundImage.sprite = null;
        _dialogueBgCurrent = null;
        _dialogueBgPending = null;

        if (backgroundImage != null && backgroundImage.color != Color.white)
            backgroundImage.color = Color.white;
    }

    public void SetMap(int index)
    {
        if (mapImage == null)
            return;

        int resolvedIndex = ResolveMapIndex(index);

        if (resolvedIndex < 0 || resolvedIndex >= maps.Length || maps[resolvedIndex] == null)
            return;

        mapImage.sprite = maps[resolvedIndex];
        bool isModern = resolvedIndex == 8 || resolvedIndex == 9 || resolvedIndex == 10;
        bool isFantasy = resolvedIndex == 6 || resolvedIndex == 7 || resolvedIndex == 11;
        bool isCombat = resolvedIndex == 13 || resolvedIndex == 19 || resolvedIndex == 20;
        MapGroup? group = isModern ? MapGroup.Modern : isFantasy ? MapGroup.Fantasy : isCombat ? MapGroup.Combat : (MapGroup?)null;
        ApplyMapIcons(group);
        SetActive(combatIcon, isCombat);

        if (mapTitleText != null)
        {
            if (isModern)
                mapTitleText.text = Loc.T("Neighborhood", "Mahalle");
            else if (isFantasy)
                mapTitleText.text = Loc.T("Ashenveil Town", "Ashenveil Kasabası");
            else if (isCombat)
                mapTitleText.text = Loc.T("Combat Region", "Savaş Bölgesi");
        }
    }

    public void SetCombatMap(int index)
    {
        if (combatMapImage == null || (index < 0 || index >= maps.Length || maps[index] == null))
            return;

        combatMapImage.sprite = maps[index];
    }

    private void ApplyMapIcons(MapGroup? group)
    {
        if (mapLocations == null)
            return;

        for (int i = 0; i < mapLocations.Length; i++)
        {
            MapLocationEntry loc = mapLocations[i];

            if (loc.icon == null)
                continue;

            bool visible = group.HasValue && loc.group == group.Value;

            if (!string.IsNullOrEmpty(loc.questID))
                visible = visible && IsQuestIconActive(loc) && (!IsNight() || !IsCurrentDayContentComplete());

            SetActive(loc.icon, visible);

            if (!visible)
                continue;

            bool interactable = PhaseMatches(loc.interactablePhase);
            SetInteractable(loc.icon, interactable);

            if (loc.interactableOffset != Vector2.zero && _mapIconDefaultPos != null && i < _mapIconDefaultPos.Length && loc.icon.TryGetComponent<RectTransform>(out var rt))
                rt.anchoredPosition = _mapIconDefaultPos[i] + (interactable ? loc.interactableOffset : Vector2.zero);
        }
    }

    private int ResolveMapIndex(int index)
    {
        TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;

        if (index == 8 || index == 9 || index == 10)
            return phase switch
            {
                TimePhase.Morning => 9,
                TimePhase.Noon => 8,
                TimePhase.Evening => 8,
                TimePhase.Night => 10,
                _ => 9
            };

        if (index == 6 || index == 7 || index == 11)
            return phase switch
            {
                TimePhase.Morning => 6,
                TimePhase.Noon => 7,
                TimePhase.Evening => 7,
                TimePhase.Night => 11,
                _ => 6
            };

        if (index == 13 || index == 19 || index == 20)
            return phase switch
            {
                TimePhase.Morning => 13,
                TimePhase.Noon => 19,
                TimePhase.Evening => 19,
                TimePhase.Night => 20,
                _ => 13
            };

        return index;
    }

    public void NextMap()
    {
        _validMapCursor = (_validMapCursor + 1) % validMapIndices.Length;
        currentMapIndex = validMapIndices[_validMapCursor];
        SetMap(currentMapIndex);
    }

    public void PreviousMap()
    {
        _validMapCursor = (_validMapCursor - 1 + validMapIndices.Length) % validMapIndices.Length;
        currentMapIndex = validMapIndices[_validMapCursor];
        SetMap(currentMapIndex);
    }

    public void SubscribeDialogue()
    {
        if (isDialogueSubscribed || DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.OnDialogueStart += HandleDialogueStart;
        DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        DialogueManager.Instance.OnLineShown += HandleLineShown;
        DialogueManager.Instance.OnNodeAdvanced += HandleNodeAdvanced;
        isDialogueSubscribed = true;
    }

    public void UnsubscribeDialogue()
    {
        if (!isDialogueSubscribed || DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.OnDialogueStart -= HandleDialogueStart;
        DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        DialogueManager.Instance.OnLineShown -= HandleLineShown;
        DialogueManager.Instance.OnNodeAdvanced -= HandleNodeAdvanced;
        isDialogueSubscribed = false;
    }

    void TryStartDialogue(int nodeIndex)
    {
        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[SceneEvent] TryStartDialogue: DialogueManager.Instance is null.");
            return;
        }

        if (sceneStartDialogueNodes == null || nodeIndex >= sceneStartDialogueNodes.Length)
        {
            Debug.LogWarning($"[SceneEvent] TryStartDialogue: no node at index {nodeIndex}.");
            return;
        }

        DialogueNode node = sceneStartDialogueNodes[nodeIndex];

        if (node != null)
            DialogueManager.Instance.StartDialogue(node);
    }

    public IEnumerator StartDialogueAfterLoad(int nodeIndex)
    {
        yield return null;
        bool isFreeRoamScene = Progress == SceneProgress.SceneHome || Progress == SceneProgress.SceneMarket || Progress == SceneProgress.SceneGym || Progress == SceneProgress.SceneOffice || Progress == SceneProgress.SceneChurch;
        DialogueNode node = sceneStartDialogueNodes != null && nodeIndex >= 0 && nodeIndex < sceneStartDialogueNodes.Length ? sceneStartDialogueNodes[nodeIndex] : null;
        int currentDay = TimeUI.Instance != null ? TimeUI.Instance.GetCurrentDay() : 1;
        bool pastDay1Sequence = currentDay > 1 || (ScenarioManager.Instance != null && ScenarioManager.Instance.GetCompletedScenarios().Count > 0);

        if (isFreeRoamScene || pastDay1Sequence || node == null || DialogueManager.Instance == null)
        {
            if (pastDay1Sequence && !isFreeRoamScene)
            {
                Progress = SceneProgress.Scene1;
                SetBackground(0);
            }

            ShowHudPanels();
            yield break;
        }

        DialogueManager.Instance.StartDialogue(node);
    }

    void HandleDialogueStart(DialogueNode node)
    {
        if (node != null && node.backgroundImage != null)
            SetActive(townNpc, false);

        if (timePanelAnimator != null)
        {
            timePanelAnimator.ResetTrigger(timePanelOpenTrigger);
            timePanelAnimator.SetTrigger(timePanelCloseTrigger);
        }

        if (iconPanelAnimator != null)
        {
            iconPanelAnimator.ResetTrigger(iconPanelOpenTrigger);
            iconPanelAnimator.SetTrigger(iconPanelCloseTrigger);
        }

        if (minigameLauncherPanelAnimator != null)
        {
            minigameLauncherPanelAnimator.ResetTrigger(minigameLauncherPanelOpenTrigger);
            minigameLauncherPanelAnimator.SetTrigger(minigameLauncherPanelCloseTrigger);
        }

        SetActive(settingsIconPanel, false);
        HideAllPanels();
        SetActive(houseIconsPanel, false);
        bool isFinaleCutscene = node != null && node.name.StartsWith("Finale_");

        if (DialogueManager.Instance != null && DialogueManager.Instance.backgroundImage != null)
            DialogueManager.Instance.backgroundImage.preserveAspect = isFinaleCutscene;

        if (backgroundImage != null)
        {
            if (isFinaleCutscene)
            {
                backgroundImage.enabled = true;
                backgroundImage.color = Color.black;
            }
            else if (backgroundImage.color != Color.white)
            {
                backgroundImage.color = Color.white;
            }
        }

        if (isFinaleCutscene)
        {
            if (charFadeCoroutine != null)
            {
                StopCoroutine(charFadeCoroutine);
                charFadeCoroutine = null;
            }

            _charFadingOut = false;
            _sceneCharacterActive = false;

            if (charImage != null)
                charImage.gameObject.SetActive(false);

            return;
        }

        if (charImage != null)
        {
            if (charFadeCoroutine != null)
            {
                StopCoroutine(charFadeCoroutine);
                charFadeCoroutine = null;
            }

            bool wasFadingOut = _charFadingOut;
            _charFadingOut = false;

            if (charImage.gameObject.activeSelf && !wasFadingOut)
            {
                Color cc = charImage.color;
                charImage.color = new Color(cc.r, cc.g, cc.b, 1f);
            }

            if (node != null && node.characterImage != null)
            {
                _dialogueBgPending = PhaseVariantOf(node.backgroundImage);
                ShowCharacterFaded(node.characterImage);
            }
            else if (_sceneCharacterActive)
            {
                if (_lastBgIndex >= 0 && _lastBgIndex <= 7)
                    ApplySceneCharacterLayout();

                charImage.preserveAspect = true;
                bool alreadyVisible = charImage.gameObject.activeSelf && charImage.color.a > 0.99f;
                charImage.gameObject.SetActive(true);

                if (alreadyVisible)
                {
                    Color c = charImage.color;
                    charImage.color = new Color(c.r, c.g, c.b, 1f);
                }
                else
                {
                    charFadeCoroutine = StartCoroutine(FadeInCurrentCharacter());
                }
            }

            else if (charImage.gameObject.activeSelf && charImage.color.a > 0.05f)
            {
                charFadeCoroutine = StartCoroutine(FadeOutSpeaker(0.3f));
            }
            else
            {
                charImage.gameObject.SetActive(false);
            }
        }
    }

    void HandleNodeAdvanced(DialogueNode node)
    {
        if (charImage == null || node == null || node.characterImage == null)
            return;

        _dialogueBgPending = PhaseVariantOf(node.backgroundImage);
        ShowCharacterFaded(node.characterImage);
    }

    void ShowCharacterFaded(Sprite sprite)
    {
        if (charImage == null || sprite == null)
            return;

        bool visible = charImage.gameObject.activeSelf && charImage.color.a > 0.05f;
        bool deferBg = visible && charImage.sprite != sprite && _dialogueBgPending != null && _dialogueBgCurrent != null && _dialogueBgPending != _dialogueBgCurrent;

        if (charImage.sprite == sprite)
        {
            if (charFadeCoroutine != null)
            {
                StopCoroutine(charFadeCoroutine);
                charFadeCoroutine = null;
            }

            _charFadingOut = false;
            bool bgChanged = _dialogueBgPending != null && _dialogueBgPending != _dialogueBgCurrent;

            if (bgChanged && visible && DialogueCharacterLayout(_dialogueBgPending, sprite) != DialogueCharacterLayout(_dialogueBgCurrent, sprite))
            {
                charFadeCoroutine = StartCoroutine(SwapCharacter(sprite));
                return;
            }

            charImage.gameObject.SetActive(true);
            Color c = charImage.color;
            charImage.color = new Color(c.r, c.g, c.b, 1f);
            ApplyDialogueCharacterLayout();

            if (bgChanged && visible)
            {
                charFadeCoroutine = StartCoroutine(SwapBackgroundOnly(_dialogueBgPending));
            }
            else if (_dialogueBgPending != null)
            {
                _dialogueBgCurrent = _dialogueBgPending;

                if (DialogueManager.Instance != null && DialogueManager.Instance.backgroundImage != null)
                {
                    DialogueManager.Instance.backgroundImage.sprite = _dialogueBgPending;
                    DialogueManager.Instance.backgroundImage.enabled = true;
                }
            }

            return;
        }

        if (charFadeCoroutine != null)
        {
            StopCoroutine(charFadeCoroutine);
            charFadeCoroutine = null;
        }

        if (visible && charImage.sprite != sprite)
        {
            if (deferBg && DialogueManager.Instance != null && DialogueManager.Instance.backgroundImage != null)
                DialogueManager.Instance.backgroundImage.sprite = _dialogueBgCurrent;

            charFadeCoroutine = StartCoroutine(SwapCharacter(sprite));
        }
        else if (IsHoldingScene)
        {
            if (_dialogueBgPending != null)
                SetBackdrop(_dialogueBgPending);

            charImage.sprite = sprite;
            ApplyDialogueCharacterLayout();
            Color c = charImage.color;
            charImage.color = new Color(c.r, c.g, c.b, 0f);
            charImage.gameObject.SetActive(true);
            charFadeCoroutine = StartCoroutine(FadeCharacterInAfterHold());
        }
        else if (_dialogueBgPending != null && _dialogueBgPending != VisibleBackdrop())
        {
            charFadeCoroutine = StartCoroutine(DipToScene(_dialogueBgPending, sprite));
        }
        else
        {
            if (_dialogueBgPending != null)
                SetBackdrop(_dialogueBgPending);

            charFadeCoroutine = StartCoroutine(FadeInCharacter(sprite));
        }
    }

    IEnumerator SwapBackgroundOnly(Sprite newBg)
    {
        yield return RunSceneFade(1f, SceneDipOut);

        if (newBg != null)
            SetBackdrop(newBg);

        yield return RunSceneFade(0f, SceneDipIn);
        charFadeCoroutine = null;
    }

    IEnumerator SwapCharacter(Sprite newSprite)
    {
        _charImageFromDialogue = true;
        _sceneCharacterActive = false;
        _charFadingOut = true;
        Color col = charImage.color;
        float startA = col.a;

        for (float e = 0f; e < 0.3f; e += Time.deltaTime)
        {
            charImage.color = new Color(col.r, col.g, col.b, Mathf.Lerp(startA, 0f, e / 0.3f));
            yield return null;
        }

        _charFadingOut = false;
        charImage.color = new Color(col.r, col.g, col.b, 0f);
        charImage.gameObject.SetActive(false);
        bool placeChanges = _dialogueBgPending != null && _dialogueBgPending != VisibleBackdrop();

        if (placeChanges)
            yield return RunSceneFade(1f, SceneDipOut);
        else
        {
            yield return _bgSwapPause;
        }

        if (_dialogueBgPending != null)
            SetBackdrop(_dialogueBgPending);

        charImage.sprite = newSprite;
        ApplyDialogueCharacterLayout();
        charImage.gameObject.SetActive(true);
        charImage.color = new Color(col.r, col.g, col.b, 0f);

        if (placeChanges)
            yield return RunSceneFade(0f, SceneDipIn);

        for (float e = 0f; e < 0.28f; e += Time.deltaTime)
        {
            charImage.color = new Color(col.r, col.g, col.b, Mathf.Lerp(0f, 1f, e / 0.28f));
            yield return null;
        }

        charImage.color = new Color(col.r, col.g, col.b, 1f);
        charFadeCoroutine = null;
    }

    IEnumerator FadeInCharacter(Sprite newSprite)
    {
        _charFadingOut = false;
        _charImageFromDialogue = true;
        _sceneCharacterActive = false;
        const float inDur = 0.28f;
        Color baseColor = charImage.color;
        charImage.gameObject.SetActive(true);
        charImage.sprite = newSprite;
        ApplyDialogueCharacterLayout();
        charImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 0f);

        for (float t = 0f; t < inDur; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(0f, 1f, t / inDur);
            charImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        charImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        charFadeCoroutine = null;
    }

    IEnumerator FadeInCurrentCharacter()
    {
        _charFadingOut = false;
        const float inDur = 0.28f;
        Color baseColor = charImage.color;

        for (float t = 0f; t < inDur; t += Time.deltaTime)
        {
            float a = Mathf.Lerp(0f, 1f, t / inDur);
            charImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, a);
            yield return null;
        }

        charImage.color = new Color(baseColor.r, baseColor.g, baseColor.b, 1f);
        charFadeCoroutine = null;
    }

    IEnumerator FadeOutCharacter(float duration, DialogueNode endedNode = null)
    {
        if (charImage == null)
            yield break;

        _charFadingOut = true;
        Color startColor = charImage.color;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(startColor.a, 0f, elapsed / duration);
            charImage.color = new Color(startColor.r, startColor.g, startColor.b, alpha);
            yield return null;
        }

        charImage.color = new Color(startColor.r, startColor.g, startColor.b, 0f);
        charImage.gameObject.SetActive(false);
        charImage.color = new Color(startColor.r, startColor.g, startColor.b, 1f);
        charFadeCoroutine = null;
        yield return _bgSwapPause;
        _charFadingOut = false;

        if (endedNode != null)
            TransitionScene(() =>
            {
                HandleSceneTransition(endedNode);
                ClearDialogueBackground();
            });
        else if (CombatManager.Instance == null || !CombatManager.Instance.inCombat)
        {
            ShowHudPanels();
        }
    }

    void ApplyDialogueCharacterLayout()
    {
        if (charImage == null)
            return;

        charImage.preserveAspect = true;
        RectTransform rt = charImage.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0f);
        rt.localScale = Vector3.one;
        Sprite bg = _dialogueBgPending != null ? _dialogueBgPending : DialogueManager.Instance != null && DialogueManager.Instance.backgroundImage != null ? DialogueManager.Instance.backgroundImage.sprite : null;
        var (position, size) = DialogueCharacterLayout(bg, charImage.sprite);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        FitCharacterWidth();
    }

    void FitCharacterWidth()
    {
        if (charImage == null || charImage.sprite == null)
            return;

        RectTransform rt = charImage.rectTransform;
        Rect r = charImage.sprite.rect;
        float width = rt.sizeDelta.y * r.width / r.height;

        if (width > rt.sizeDelta.x)
            rt.sizeDelta = new Vector2(width, rt.sizeDelta.y);
    }

    (Vector2 position, Vector2 size) DialogueCharacterLayout(Sprite bg, Sprite character)
    {
        if (bg != null && !(character != null && System.Array.IndexOf(HalfBodyCharacterSprites, character.name) >= 0))
            foreach (var o in DialogueCharacterLayoutOverrides)
                if (o.backgroundName == bg.name)
                    return (o.position, o.size);

        return (dialogueCharacterPosition, dialogueCharacterSize);
    }

    void ApplySceneCharacterLayout()
    {
        if (charImage == null)
            return;

        charImage.preserveAspect = true;
        RectTransform rt = charImage.rectTransform;
        rt.anchorMin = new Vector2(0.5f, 0f);
        rt.anchorMax = new Vector2(0.5f, 0f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = new Vector2(0f, 375f);
        rt.sizeDelta = new Vector2(75f, 75f);
        rt.localScale = new Vector3(10f, 10f, 10f);
        FitCharacterWidth();
    }

    bool IsInDialogue() => DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue();

    static bool IsHouseBackground(int index) => index == 11 || index == 13 || index == 14 || index == 15 || index == 16 || index == 17 || index == 20 || index == 38 || index == 39 || index == 40;

    void ApplyHouseIconVisibility(bool isHouse)
    {
        SetActive(houseIconsPanel, isHouse);

        if (roomIcons != null)
            foreach (var room in roomIcons)
                SetActive(room.icon, isHouse);
    }

    public void ShowHudPanels()
    {
        if (!isActiveAndEnabled)
        {
            ShowHudPanelsNow();
            return;
        }

        _hudRoutine ??= StartCoroutine(ShowHudWhenSettled());
    }

    IEnumerator ShowHudWhenSettled()
    {
        var dm = DialogueManager.Instance;
        float busyFor = 0f;
        float quietFor = 0f;

        while (quietFor < HudRevealDelay)
        {
            if (CombatOpen() || (dm != null && dm.IsInDialogue()))
            {
                busyFor = 0f;
                quietFor = 0f;
            }
            else if (dm != null && (dm.IsConversationBusy() || dm.IsSceneTransitionBusy()) && busyFor < 3f)
            {
                busyFor += Time.unscaledDeltaTime;
                quietFor = 0f;
            }
            else
            {
                quietFor += Time.unscaledDeltaTime;
            }

            yield return null;
        }

        _hudRoutine = null;
        ShowHudPanelsNow();
    }

    static bool CombatOpen() => (CombatManager.Instance != null && CombatManager.Instance.inCombat) || (CombatUI.Instance != null && CombatUI.Instance.combatPanel != null && CombatUI.Instance.combatPanel.activeSelf);

    void WatchCombatEnd()
    {
        bool inCombat = CombatOpen();

        if (_wasInCombat && !inCombat)
            ShowHudPanels();

        _wasInCombat = inCombat;
    }

    void ShowHudPanelsNow()
    {
        SetActive(settingsIconPanel, true);
        SetActive(timePanel, true);
        SetActive(iconPanel, true);
        SetActive(minigameLauncherPanel, true);

        if (timePanelAnimator != null)
        {
            timePanelAnimator.ResetTrigger(timePanelCloseTrigger);
            timePanelAnimator.SetTrigger(timePanelOpenTrigger);
        }

        if (iconPanelAnimator != null)
        {
            iconPanelAnimator.ResetTrigger(iconPanelCloseTrigger);
            iconPanelAnimator.SetTrigger(iconPanelOpenTrigger);
        }

        if (minigameLauncherPanelAnimator != null)
        {
            minigameLauncherPanelAnimator.ResetTrigger(minigameLauncherPanelCloseTrigger);
            minigameLauncherPanelAnimator.SetTrigger(minigameLauncherPanelOpenTrigger);
        }

        ApplyHouseIconVisibility(IsHouseBackground(_lastBgIndex));

        if (!_charFadingOut && string.IsNullOrEmpty(_currentQuestLocation))
            ClearDialogueBackground();
    }

    void HandleDialogueEnd(DialogueNode endedNode)
    {
        bool scenarioActive = ScenarioManager.Instance != null && ScenarioManager.Instance.IsScenarioActive();
        bool isFinal = !scenarioActive && endedNode != null && endedNode.isFinalNode;

        if (charImage != null && charImage.gameObject.activeSelf)
        {
            if (charFadeCoroutine != null)
            {
                StopCoroutine(charFadeCoroutine);
                charFadeCoroutine = null;
            }

            if (!isFinal && NextDialogueKeepsSpeaker())
            {
                _keepingSpeaker = true;
                StartCoroutine(KeepSpeakerForNextDialogue());
                return;
            }

            charFadeCoroutine = StartCoroutine(DeferredFadeOut(0.5f, isFinal ? endedNode : null));
        }
        else if (isFinal)
        {
            TransitionScene(() => HandleSceneTransition(endedNode));
        }
        else if (CombatManager.Instance == null || !CombatManager.Instance.inCombat)
        {
            ShowHudPanels();
        }

        if (!isFinal)
            return;

        ShowHudPanels();

        if (Progress == SceneProgress.SceneMarket && sceneStartDialogueNodes != null && sceneStartDialogueNodes.Length > 9 && sceneStartDialogueNodes[9] != null && endedNode == sceneStartDialogueNodes[9] && MarketUI.Instance != null)
            MarketUI.Instance.OpenMarket();
    }

    bool NextDialogueKeepsSpeaker()
    {
        var sm = ScenarioManager.Instance;
        var dm = DialogueManager.Instance;

        if (sm == null || dm == null || charImage == null || charImage.sprite == null || charImage.color.a < 0.5f || _charFadingOut)
            return false;

        DialogueNode next = sm.PeekFollowUpDialogue(dm.LastStartNode);

        if (next == null || next.characterImage == null || next.characterImage != charImage.sprite)
            return false;

        return next.backgroundImage == null || PhaseVariantOf(next.backgroundImage) == VisibleBackdrop();
    }

    IEnumerator KeepSpeakerForNextDialogue()
    {
        var dm = DialogueManager.Instance;

        for (float t = 0f; t < 2f; t += Time.unscaledDeltaTime)
        {
            if (dm == null || dm.IsInDialogue())
            {
                _keepingSpeaker = false;
                yield break;
            }

            yield return null;
        }

        _keepingSpeaker = false;

        if (charImage != null && charImage.gameObject.activeSelf)
            charFadeCoroutine = StartCoroutine(DeferredFadeOut(0.5f, null));
        else
            ShowHudPanels();
    }

    IEnumerator DeferredFadeOut(float duration, DialogueNode endedNode)
    {
        _charFadingOut = true;
        yield return _waitForSecondsRealtime0_12;
        yield return FadeOutCharacter(duration, endedNode);
    }

    void OnScenarioStarted(ScenarioData scenario)
    {
        DialogueNode first = scenario == null ? null : scenario.introDialogue != null ? scenario.introDialogue : scenario.steps != null && scenario.steps.Length > 0 && scenario.steps[0].type == ScenarioStepType.Dialogue ? scenario.steps[0].dialogue : null;

        if (charImage != null && charImage.gameObject.activeSelf && charImage.color.a > 0.05f && !_charFadingOut && first != null && first.characterImage != null && first.characterImage == charImage.sprite)
            return;

        if (charImage != null && charImage.gameObject.activeSelf && charImage.color.a > 0.05f)
        {
            _sceneCharacterActive = false;
            _charImageFromDialogue = false;

            if (!_charFadingOut)
            {
                if (charFadeCoroutine != null)
                    StopCoroutine(charFadeCoroutine);

                charFadeCoroutine = StartCoroutine(FadeOutSpeaker(0.3f));
            }

            return;
        }

        ForceHideSceneCharacter();
    }

    void OnScenarioCompleted(ScenarioData scenario)
    {
        StartCoroutine(TryStartDayScenarioAfterScenario());

        if (charImage == null)
            return;

        _sceneCharacterActive = false;

        if (charFadeCoroutine != null && _charFadingOut)
            return;

        if (charImage.gameObject.activeSelf && charImage.color.a > 0.01f)
        {
            charFadeCoroutine = StartCoroutine(DeferredFadeOut(0.5f, null));
        }
        else
        {
            charImage.gameObject.SetActive(false);
            ShowHudPanels();
        }
    }

    IEnumerator TryStartDayScenarioAfterScenario()
    {
        yield return null;

        while (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue())
            yield return null;

        if (_lastBgIndex == 0 && string.IsNullOrEmpty(_currentQuestLocation))
            TryStartDayScenario();
    }

    bool HasUnplayedDayScenario()
    {
        ScenarioManager sm = ScenarioManager.Instance;

        if (sm == null || sm.availableScenarios == null)
            return false;

        foreach (ScenarioData scenario in sm.availableScenarios)
            if (scenario != null && !string.IsNullOrEmpty(scenario.scenarioID) && scenario.scenarioID.StartsWith("ashenveil_day") && !sm.IsScenarioCompleted(scenario.scenarioID))
                return true;

        return false;
    }

    void ForceHideSceneCharacter()
    {
        if (charFadeCoroutine != null)
        {
            StopCoroutine(charFadeCoroutine);
            charFadeCoroutine = null;
        }

        _charFadingOut = false;
        _sceneCharacterActive = false;
        _charImageFromDialogue = false;

        if (charImage == null)
            return;

        Color cc = charImage.color;
        charImage.color = new Color(cc.r, cc.g, cc.b, 1f);
        charImage.gameObject.SetActive(false);
    }

    static bool IsReachableFrom(DialogueNode root, DialogueNode node, int depth)
    {
        if (root == null || node == null || depth <= 0 || root.choices == null)
            return false;

        foreach (var choice in root.choices)
            if (choice != null && choice.nextNode != null && (choice.nextNode == node || IsReachableFrom(choice.nextNode, node, depth - 1)))
                return true;

        return false;
    }

    void HandleSceneTransition(DialogueNode endedNode)
    {
        switch (endedNode.sceneContext)
        {
            case SceneProgress.Scene2:
                TriggerScene3();
                break;

            case SceneProgress.Scene3:
                TriggerScene4();
                break;

            case SceneProgress.Scene4:
                StartCombatForScene4();
                break;

            case SceneProgress.Scene5:
                if (endedNode == sceneStartDialogueNodes[4] || IsReachableFrom(sceneStartDialogueNodes[4], endedNode, 3))
                    TriggerScene6();

                break;

            case SceneProgress.Scene6:
                TriggerScene7();
                break;

            case SceneProgress.Scene7:
                TriggerScene8();
                break;

            case SceneProgress.Scene8:
                TriggerScene9();
                break;
        }
    }

    void HandleLineShown(DialogueNode node, int lineIndex)
    {
        if (node == sceneStartDialogueNodes[0] && lineIndex == 1)
            SetCharacter(8);

        if (node == sceneStartDialogueNodes[3] && lineIndex == 0)
            SetCharacter(14);

        if (node == scene9SecondNode && lineIndex == 0)
        {
            SetCharacter(20);
            SetBackground(42);

            if (charImage != null)
            {
                RectTransform rt = charImage.rectTransform;
                charRtAnchoredTransform = rt.anchoredPosition;
                charRtSizeDelta = rt.sizeDelta;
                rt.anchoredPosition = new Vector2(0f, 500f);
                rt.sizeDelta = new Vector2(100f, 100f);
            }
        }

        if (node == scene9FifthNode && lineIndex == 0)
            SetCharacter(38);
    }

    public void OpenDialoguePanel()
    {
        if (dialoguePanelAnimator != null)
        {
            dialoguePanelAnimator.ResetTrigger(dialoguePanelCloseTrigger);
            dialoguePanelAnimator.SetTrigger(dialoguePanelOpenTrigger);
        }
    }

    public void CloseDialoguePanel()
    {
        if (dialoguePanelAnimator == null || !dialoguePanelAnimator.gameObject.activeInHierarchy)
            return;

        dialoguePanelAnimator.ResetTrigger(dialoguePanelOpenTrigger);
        dialoguePanelAnimator.SetTrigger(dialoguePanelCloseTrigger);
        dialoguePanelAnimator.Play(DialoguePanelCloseHash, 0, 0f);
        dialoguePanelAnimator.Update(0f);
    }

    public float DialogueOpenAnimationDuration() => 0f;

    public bool IsSceneTransitionActive() => charFadeCoroutine != null || _transitionDepth > 0;

    Image SceneFade()
    {
        if (_sceneFade == null)
        {
            Transform root = charImage != null ? charImage.canvas.rootCanvas.transform : transform;
            var go = new GameObject("SceneFade", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(root, false);
            var rt = (RectTransform)go.transform;
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            _sceneFade = go.GetComponent<Image>();
            _sceneFade.color = new Color(0f, 0f, 0f, 0f);
            _sceneFade.raycastTarget = false;
            go.SetActive(false);
        }

        _sceneFade.transform.SetAsLastSibling();
        return _sceneFade;
    }

    IEnumerator FadeSceneTo(float target, float duration)
    {
        Image img = SceneFade();
        img.gameObject.SetActive(true);
        float from = img.color.a;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            img.color = new Color(0f, 0f, 0f, Mathf.Lerp(from, target, t / duration));
            yield return null;
        }

        img.color = new Color(0f, 0f, 0f, target);

        if (target <= 0f)
            img.gameObject.SetActive(false);
    }

    IEnumerator RunSceneFade(float target, float duration)
    {
        if (_sceneFadeRoutine != null)
            StopCoroutine(_sceneFadeRoutine);

        _sceneFadeRoutine = StartCoroutine(FadeSceneTo(target, duration));
        yield return _sceneFadeRoutine;
        _sceneFadeRoutine = null;
    }

    Sprite ShownBackdrop()
    {
        Image img = DialogueManager.Instance != null ? DialogueManager.Instance.backgroundImage : null;
        return img != null && img.enabled ? img.sprite : null;
    }

    Sprite VisibleBackdrop() => ShownBackdrop() != null ? ShownBackdrop() : (backgroundImage != null && backgroundImage.enabled ? backgroundImage.sprite : null);

    void SetBackdrop(Sprite sprite)
    {
        Image img = DialogueManager.Instance != null ? DialogueManager.Instance.backgroundImage : null;

        if (img == null)
            return;

        img.sprite = sprite;
        img.enabled = sprite != null;
        _dialogueBgCurrent = sprite;
    }

    public void ShowDialogueBackdrop(Sprite sprite)
    {
        if (DialogueManager.Instance == null || DialogueManager.Instance.backgroundImage == null)
            return;

        _dialogueBgPending = sprite;

        if (sprite == ShownBackdrop() || sprite == VisibleBackdrop() || IsHoldingScene)
        {
            SetBackdrop(sprite);
            return;
        }

        StartCoroutine(BackdropAfterTransition(sprite));
    }

    IEnumerator BackdropAfterTransition(Sprite sprite)
    {
        _transitionDepth++;

        while (charFadeCoroutine != null || _sceneFadeRoutine != null)
            yield return null;

        if (_dialogueBgPending == sprite && sprite != ShownBackdrop())
        {
            if (sprite != VisibleBackdrop())
            {
                yield return RunSceneFade(1f, SceneDipOut);
                SetBackdrop(sprite);
                yield return RunSceneFade(0f, SceneDipIn);
            }
            else
                SetBackdrop(sprite);
        }

        _transitionDepth--;
    }

    IEnumerator FadeOutSpeaker(float duration)
    {
        _charFadingOut = true;
        Color c = charImage.color;
        float from = c.a;

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            charImage.color = new Color(c.r, c.g, c.b, Mathf.Lerp(from, 0f, t / duration));
            yield return null;
        }

        charImage.gameObject.SetActive(false);
        charImage.color = new Color(c.r, c.g, c.b, 1f);
        _charFadingOut = false;
        charFadeCoroutine = null;
    }

    IEnumerator FadeCharacterInAfterHold()
    {
        while (IsHoldingScene || _sceneFadeRoutine != null)
            yield return null;

        yield return FadeCharacterIn(0.28f);
        charFadeCoroutine = null;
    }

    IEnumerator DipToScene(Sprite backdrop, Sprite character)
    {
        yield return RunSceneFade(1f, SceneDipOut);
        SetBackdrop(backdrop);

        if (character != null && charImage != null)
        {
            charImage.sprite = character;
            ApplyDialogueCharacterLayout();
            Color c = charImage.color;
            charImage.color = new Color(c.r, c.g, c.b, 0f);
            charImage.gameObject.SetActive(true);
        }

        yield return RunSceneFade(0f, SceneDipIn);

        if (character != null && charImage != null)
            yield return FadeCharacterIn(0.28f);

        charFadeCoroutine = null;
    }

    IEnumerator FadeCharacterIn(float duration)
    {
        _charImageFromDialogue = true;
        _sceneCharacterActive = false;
        Color c = charImage.color;
        charImage.gameObject.SetActive(true);

        for (float t = 0f; t < duration; t += Time.deltaTime)
        {
            charImage.color = new Color(c.r, c.g, c.b, Mathf.Lerp(0f, 1f, t / duration));
            yield return null;
        }

        charImage.color = new Color(c.r, c.g, c.b, 1f);
    }

    public void BeginSceneHold()
    {
        Sprite shown = ShownBackdrop();

        if (shown == null || DialogueManager.Instance == null || IsHoldingScene)
            return;

        Image src = DialogueManager.Instance.backgroundImage;

        if (_holdFrame == null)
        {
            var go = new GameObject("SceneHold", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(src.canvas.rootCanvas.transform, false);
            _holdFrame = go.GetComponent<Image>();
            _holdFrame.raycastTarget = false;
        }

        var rt = (RectTransform)_holdFrame.transform;
        var srt = src.rectTransform;
        rt.anchorMin = srt.anchorMin;
        rt.anchorMax = srt.anchorMax;
        rt.pivot = srt.pivot;
        rt.position = srt.position;
        rt.sizeDelta = srt.sizeDelta;
        rt.localScale = srt.localScale;
        _holdFrame.sprite = shown;
        _holdFrame.preserveAspect = src.preserveAspect;
        _holdFrame.color = src.color;
        _holdFrame.gameObject.SetActive(true);
        _holdFrame.transform.SetAsLastSibling();
        _holdSprite = shown;
        _transitionDepth++;
    }

    public void EndSceneHold()
    {
        if (_holdFrame == null || !_holdFrame.gameObject.activeSelf)
            return;

        Sprite now = ShownBackdrop() != null ? ShownBackdrop() : (backgroundImage != null ? backgroundImage.sprite : null);

        if (now == _holdSprite)
        {
            _holdFrame.gameObject.SetActive(false);
            _transitionDepth--;
            return;
        }

        StartCoroutine(DipOutOfHold());
    }

    IEnumerator DipOutOfHold()
    {
        yield return RunSceneFade(1f, SceneDipOut);
        _holdFrame.gameObject.SetActive(false);
        yield return RunSceneFade(0f, SceneDipIn);
        _transitionDepth--;
    }

    public void TransitionScene(System.Action change)
    {
        if (_keepingSpeaker)
        {
            change?.Invoke();
            return;
        }

        BeginSceneHold();
        change?.Invoke();
        StartCoroutine(EndHoldNextFrame());
    }

    IEnumerator EndHoldNextFrame()
    {
        yield return null;
        EndSceneHold();
    }

    public float DialogueCloseAnimationDuration()
    {
        float len = dialogueCloseFallbackDuration;

        if (dialoguePanelAnimator != null)
        {
            AnimatorStateInfo info = dialoguePanelAnimator.GetCurrentAnimatorStateInfo(0);

            if (info.length > 0f)
                len = info.length;
        }

        return Mathf.Max(len, 0.4f);
    }

    public void ApplySceneProgress(SceneProgress targetProgress)
    {
        progress = targetProgress;

        switch (progress)
        {
            case SceneProgress.Scene1:
                SetBackground(0);
                SetCharacter(0);
                break;

            case SceneProgress.Scene2:
                SetBackground(1);
                SetCharacter(8);
                break;

            case SceneProgress.Scene3:
                SetBackground(2);
                SetCharacter(8);
                break;

            case SceneProgress.Scene4:
                SetBackground(3);
                SetCharacter(14);
                break;

            case SceneProgress.Scene5:
                SetBackground(3);
                SetCharacter(8);
                break;

            case SceneProgress.Scene6:
                SetBackground(4);
                SetCharacter(8);
                break;

            case SceneProgress.Scene7:
                SetBackground(5);
                SetCharacter(8);
                break;

            case SceneProgress.Scene8:
                SetBackground(6);
                SetCharacter(8);
                break;

            case SceneProgress.Scene9:
                SetBackground(7);
                SetCharacter(8);
                break;

            case SceneProgress.SceneHome:
                SetBackground(11);
                SetCharacter(27);
                break;

            case SceneProgress.SceneMarket:
                SetBackground(12);
                SetCharacter(29);
                break;

            case SceneProgress.SceneGym:
                SetBackground(8);
                SetCharacter(22);
                break;

            case SceneProgress.SceneOffice:
                SetBackground(9);
                SetCharacter(24);
                break;

            case SceneProgress.SceneChurch:
                SetBackground(10);
                SetCharacter(26);
                break;
        }
    }

    private void SubscribeHoverEffects()
    {
        if (hoverRegions == null || hoverRegions.Length == 0 || isHoverEffectsSubscribed)
            return;

        _hoverClickActions.Clear();

        foreach (var entry in hoverRegions)
        {
            if (entry.region == null || !entry.region.TryGetComponent<UIHoverRegion>(out var hover))
                continue;

            HoverRegionEntry captured = entry;
            void handler() => OnHoverClicked(captured);
            _hoverClickActions.Add((hover, handler));
            hover.OnRegionClicked += handler;
        }

        isHoverEffectsSubscribed = true;
    }

    public void UnsubscribeHoverEffects()
    {
        if (!isHoverEffectsSubscribed)
            return;

        foreach (var (hover, handler) in _hoverClickActions)
        {
            if (hover != null)
                hover.OnRegionClicked -= handler;
        }

        _hoverClickActions.Clear();
        isHoverEffectsSubscribed = false;
    }

    private void OnHoverClicked(HoverRegionEntry entry)
    {
        SetActive(entry.region, false);

        switch (entry.action)
        {
            case HoverAction.EnterTown:
                OnDoorClicked();
                break;

            case HoverAction.OpenMarket:
                TriggerMarketScene();
                break;

            case HoverAction.OpenChurch:
                TriggerChurchScene();
                break;

            case HoverAction.OpenHome:
                TriggerHomeScene();
                break;

            case HoverAction.OpenOffice:
                TriggerOfficeScene();
                break;

            case HoverAction.OpenGym:
                TriggerGymScene();
                break;

            case HoverAction.SleepToNextDay:
                SkipToNextDay();
                break;

            case HoverAction.ShowGardenHole:
                ShowGardenHole();
                break;

            case HoverAction.ShowPool:
                ShowPool();
                break;

            case HoverAction.CollectItem:
                CollectWorldItem(entry.worldItemIndex);
                break;

            case HoverAction.OpenStove:
                OpenStove(entry.worldItemIndex);
                break;

            case HoverAction.QuestInteract:
                if (entry.questGrantItem != null && InventoryManager.Instance != null)
                    InventoryManager.Instance.AddItem(entry.questGrantItem);

                PlayQuestDialogueThen(entry, () =>
                {
                    if (QuestManager.Instance != null && !string.IsNullOrEmpty(entry.questObjectiveTag))
                        QuestManager.Instance.NotifyObjectInteracted(entry.questObjectiveTag, Mathf.Max(1, entry.questProgressAmount));
                });

                break;

            case HoverAction.QuestTalk:
                PlayQuestDialogueThen(entry, () =>
                {
                    if (QuestManager.Instance != null)
                        QuestManager.Instance.NotifyTalkToNPC(entry.questObjectiveTag, Mathf.Max(1, entry.questProgressAmount));
                });

                break;

            case HoverAction.QuestCombat:
                StartQuestCombat(entry);
                break;

            case HoverAction.GoToQuestLocation:
                ShowQuestLocation(entry.questLocationName);
                break;

            case HoverAction.ReturnToTown:
                ReturnToTownFromQuestLocation();
                break;
        }
    }

    private void PlayQuestDialogueThen(HoverRegionEntry entry, System.Action onDone)
    {
        if (entry.questDialogueNode == null || DialogueManager.Instance == null || DialogueManager.Instance.IsInDialogue())
        {
            onDone?.Invoke();
            return;
        }

        DialogueNode node = entry.questDialogueNode;

        if (string.IsNullOrEmpty(node.speakerName))
        {
            node = Instantiate(node);
            node.speakerName = ProfileManager.Instance != null && ProfileManager.Instance.profile != null ? ProfileManager.Instance.profile.playerName : Loc.T("You", "Sen");
        }

        DialogueManager.Instance.StartDialogue(node, onDone);
    }

    private void StartQuestCombat(HoverRegionEntry entry)
    {
        if (entry.questEnemy == null || CombatManager.Instance == null || CombatManager.Instance.inCombat)
            return;

        GameObject region = entry.region;

        PlayQuestDialogueThen(entry, () =>
        {
            if (CombatManager.Instance == null || CombatManager.Instance.inCombat)
                return;

            Action onDefeat = null;
            Action onFlee = null;
            Action<EnemyData> onVictory = null;
            string location = _currentQuestLocation;

            void Cleanup()
            {
                CombatManager.Instance.OnCombatDefeat -= onDefeat;
                CombatManager.Instance.OnCombatFled -= onFlee;
                CombatManager.Instance.OnCombatVictory -= onVictory;
            }

            onDefeat = () =>
            {
                Cleanup();
                StartCoroutine(RestoreQuestLocationAfterLostCombat(location, region));
            };

            onFlee = onDefeat;

            onVictory = (_) =>
            {
                Cleanup();
                StartCoroutine(CountGroupFight(entry));
                StartCoroutine(RestoreHudAfterQuestCombat());
            };

            CombatManager.Instance.OnCombatDefeat += onDefeat;
            CombatManager.Instance.OnCombatFled += onFlee;
            CombatManager.Instance.OnCombatVictory += onVictory;
            CombatManager.Instance.StartCombat(entry.questEnemy);
        });
    }

    private IEnumerator RestoreQuestLocationAfterLostCombat(string location, GameObject region)
    {
        yield return new WaitUntil(() => (CombatManager.Instance == null || !CombatManager.Instance.inCombat) && (CombatUI.Instance == null || CombatUI.Instance.combatPanel == null || !CombatUI.Instance.combatPanel.activeSelf));

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();

        if (!string.IsNullOrEmpty(location))
            ShowQuestLocation(location);
        else if (region != null)
            region.SetActive(true);

        ShowHudPanels();
    }

    private IEnumerator CountGroupFight(HoverRegionEntry entry)
    {
        yield return null;

        var qm = QuestManager.Instance;

        if (qm == null || entry.questProgressAmount <= 1 || string.IsNullOrEmpty(entry.questID) || string.IsNullOrEmpty(entry.questObjectiveTag))
            yield break;

        var state = qm.GetObjectiveState(entry.questID, entry.questObjectiveTag);

        if (state != null && !state.isCompleted && state.currentProgress < entry.questProgressAmount)
            qm.UpdateObjectiveProgress(entry.questID, entry.questObjectiveTag, entry.questProgressAmount - state.currentProgress);

        ApplyHoverVisibility(IsShowingQuestLocation ? -1 : _lastBgIndex);
    }

    private IEnumerator RestoreHudAfterQuestCombat()
    {
        yield return null;
        yield return null;
        ShowHudPanels();
    }

    private void ReturnToTownFromQuestLocation()
    {
        _currentQuestLocation = null;
        Progress = SceneProgress.Scene1;
        SetBackground(0);
        ShowHudPanels();
    }

    private static bool IsQuestObjectiveAction(HoverAction a) => a == HoverAction.QuestTalk || a == HoverAction.QuestInteract || a == HoverAction.QuestCombat;

    private bool IsQuestObjectiveActive(string questID, string objectiveID)
    {
        if (string.IsNullOrEmpty(questID) || QuestManager.Instance == null || !QuestManager.Instance.IsQuestActive(questID))
            return false;

        if (string.IsNullOrEmpty(objectiveID))
            return true;

        var objState = QuestManager.Instance.GetObjectiveState(questID, objectiveID);
        return objState == null || !objState.isCompleted;
    }

    private bool IsQuestIconActive(MapLocationEntry loc)
    {
        if (string.IsNullOrEmpty(loc.questID) || QuestManager.Instance == null || !QuestManager.Instance.IsQuestActive(loc.questID))
            return false;

        if (string.IsNullOrEmpty(loc.questObjectiveID))
            return true;

        foreach (var rawId in loc.questObjectiveID.Split(','))
        {
            string id = rawId.Trim();

            if (id.Length == 0)
                continue;

            var objState = QuestManager.Instance.GetObjectiveState(loc.questID, id);

            if (objState == null || !objState.isCompleted)
                return true;
        }

        return false;
    }

    private void ApplyHoverVisibility(int bgIndex)
    {
        if (hoverRegions == null)
            return;

        foreach (var entry in hoverRegions)
        {
            if (entry.region == null)
                continue;

            bool isQuestLocationRegion = !string.IsNullOrEmpty(entry.questLocationName) && (IsQuestObjectiveAction(entry.action) || entry.action == HoverAction.ReturnToTown);
            bool visible;

            if (isQuestLocationRegion)
            {
                visible = _currentQuestLocation == entry.questLocationName && PhaseMatches(entry.phase);

                if (IsQuestObjectiveAction(entry.action))
                    visible = visible && IsQuestObjectiveActive(entry.questID, entry.questObjectiveTag);
            }
            else
            {
                if (entry.visibleOnBackgrounds == null || entry.visibleOnBackgrounds.Length == 0)
                    continue;

                visible = string.IsNullOrEmpty(_currentQuestLocation) && System.Array.IndexOf(entry.visibleOnBackgrounds, bgIndex) >= 0 && PhaseMatches(entry.phase);

                if (entry.action == HoverAction.EnterTown)
                    visible = visible && !_doorClicked && !StoryFlags.Has("day1_complete");
            }

            if (!string.IsNullOrEmpty(entry.hideIfFlag) && StoryFlags.Has(entry.hideIfFlag))
                visible = false;

            SetActive(entry.region, visible);
        }
    }

    private void ApplyItemVisibility(int bgIndex)
    {
        if (worldItems == null)
            return;

        for (int i = 0; i < worldItems.Length; i++)
        {
            WorldItemEntry entry = worldItems[i];

            if (entry.item == null)
                continue;

            if (!string.IsNullOrEmpty(entry.questLocationName))
                SetActive(entry.item, _currentQuestLocation == entry.questLocationName);
            else if (entry.visibleOnBackgrounds != null && entry.visibleOnBackgrounds.Length > 0)
                SetActive(entry.item, string.IsNullOrEmpty(_currentQuestLocation) && System.Array.IndexOf(entry.visibleOnBackgrounds, bgIndex) >= 0);

            if (entry.name == "HistoryBook" && entry.item.activeSelf && _itemDefaultY != null && i < _itemDefaultY.Length && entry.item.TryGetComponent<RectTransform>(out var rt))
            {
                Vector2 p = rt.anchoredPosition;
                rt.anchoredPosition = new Vector2(p.x, _itemDefaultY[i] + (IsNight() || IsEvening() ? 25f : 0f));
            }
        }
    }

    private bool PhaseMatches(PhaseCondition condition) => condition switch
    {
        PhaseCondition.Any => true,
        PhaseCondition.MorningOrNoon => IsMorningOrNoon(),
        PhaseCondition.NotNight => !IsNight(),
        PhaseCondition.NightOnly => IsNight(),
        PhaseCondition.NoonOrEvening => IsNoonOrEvening(),
        PhaseCondition.MorningOnly => IsMorning(),
        PhaseCondition.EveningOrNight => IsEvening() || IsNight(),
        _ => true
    };

    public void ResetSceneProgress()
    {
        _doorClicked = false;

        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.SetPhase(TimePhase.Morning);

        Progress = SceneProgress.Scene1;
        SetBackground(0);
        SetCharacter(0);
        TryStartDialogue(0);
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null)
            go.SetActive(active);
    }

    static void OpenAnimated(GameObject go) => UIPanelAnimator.Show(go);

    static void CloseAnimated(GameObject go) => UIPanelAnimator.Hide(go);

    static void SetInteractable(GameObject go, bool interactable)
    {
        if (go == null)
            return;

        Button btn = go.GetComponentInChildren<Button>();

        if (btn != null)
            btn.interactable = interactable;
    }

    private bool IsEvening() => TimePhaseManager.Instance != null && TimePhaseManager.Instance.currentPhase == TimePhase.Evening;

    private bool IsNight() => TimePhaseManager.Instance != null && TimePhaseManager.Instance.currentPhase == TimePhase.Night;

    private bool IsMorningOrNoon() => TimePhaseManager.Instance != null && (TimePhaseManager.Instance.currentPhase == TimePhase.Morning || TimePhaseManager.Instance.currentPhase == TimePhase.Noon);

    private bool IsNoonOrEvening() => TimePhaseManager.Instance != null && (TimePhaseManager.Instance.currentPhase == TimePhase.Noon || TimePhaseManager.Instance.currentPhase == TimePhase.Evening);

    private bool IsMorning() => TimePhaseManager.Instance != null && TimePhaseManager.Instance.currentPhase == TimePhase.Morning;

    private void OnPhaseChanged(TimePhase newPhase)
    {
        if (_lastObservedPhase == TimePhase.Night && newPhase == TimePhase.Morning)
            dayScenarioPending = true;

        _lastObservedPhase = newPhase;

        if (mapPanel != null && mapPanel.activeSelf)
            SetMap(currentMapIndex);

        if (backgroundImage != null && _lastBgIndex >= 0)
        {
            Sprite resolved = ResolveBg(_lastBgIndex);

            if (resolved != null)
                backgroundImage.sprite = resolved;
        }
        else if (backgroundImage != null && !string.IsNullOrEmpty(_currentQuestLocation))
        {
            Sprite resolved = ResolveQuestLocationBg(_currentQuestLocation, newPhase);

            if (resolved != null)
                backgroundImage.sprite = resolved;

            ApplyHoverVisibility(-1);
            ApplyItemVisibility(-1);
        }

        Sprite shownBackdrop = ShownBackdrop();
        Sprite shownForPhase = PhaseVariantOf(shownBackdrop);

        if (shownBackdrop != null && shownForPhase != shownBackdrop)
            SetBackdrop(shownForPhase);

        ClearDialogueBackground();

        if (_lastBgIndex == 11 && _sceneCharacterActive && charImage != null && DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue())
        {
            bool showHomeChar = !IsNight() && !IsEvening();
            SetActive(charImage.gameObject, showHomeChar);

            if (showHomeChar)
                SetCharacter(27);
        }

        if (_sceneCharacterActive && charImage != null && charImage.gameObject.activeSelf && _lastCharIndex >= 0 && characters != null && _lastCharIndex < characters.Length)
        {
            TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;
            Sprite s = characters[_lastCharIndex].Resolve(phase);

            if (s != null)
            {
                charImage.sprite = s;
                FitCharacterWidth();
            }
        }

        if (_lastBgIndex >= 0)
        {
            ApplyHoverVisibility(_lastBgIndex);
            ApplyItemVisibility(_lastBgIndex);
        }
    }

    private CombatAction GetFleeAction()
    {
        var cm = CombatManager.Instance;

        if (cm == null || cm.defaultActions == null)
            return null;

        return cm.defaultActions.Find(a => a.isFlee);
    }

    private void SetFleeDisabled(bool disabled)
    {
        var flee = GetFleeAction();

        if (flee != null)
            flee.isDisabled = disabled;
    }

    private void StartCombatForScene(int enemyIndex, Action<EnemyData> onVictory, Action onDefeat, Action onFlee = null)
    {
        var cm = CombatManager.Instance;

        if (cm == null)
        {
            Debug.LogWarning("[SceneEvent] StartCombatForScene: CombatManager not found.");
            return;
        }

        if (enemies == null || enemyIndex < 0 || enemyIndex >= enemies.Length || enemies[enemyIndex] == null)
        {
            Debug.LogWarning($"[SceneEvent] StartCombatForScene: no enemy at index {enemyIndex}.");
            return;
        }

        UnsubscribeSceneCombat();
        _currentVictoryHandler = onVictory;
        _currentDefeatHandler = onDefeat;
        _currentFleeHandler = onFlee;
        cm.OnCombatVictory += _currentVictoryHandler;
        cm.OnCombatDefeat += _currentDefeatHandler;

        if (_currentFleeHandler != null)
            cm.OnCombatFled += _currentFleeHandler;

        SetFleeDisabled(onFlee == null);
        cm.StartCombat(enemies[enemyIndex]);
    }

    private void UnsubscribeSceneCombat()
    {
        var cm = CombatManager.Instance;

        if (cm == null)
            return;

        if (_currentVictoryHandler != null)
        {
            cm.OnCombatVictory -= _currentVictoryHandler;
            _currentVictoryHandler = null;
        }

        if (_currentDefeatHandler != null)
        {
            cm.OnCombatDefeat -= _currentDefeatHandler;
            _currentDefeatHandler = null;
        }

        if (_currentFleeHandler != null)
        {
            cm.OnCombatFled -= _currentFleeHandler;
            _currentFleeHandler = null;
        }
    }

    private void StartCombatForScene4()
    {
        StartCombatForScene(enemyIndex: 0,
            onVictory: (enemy) =>
            {
                SetFleeDisabled(false);
                UnsubscribeSceneCombat();
                StoryFlags.Add("day1_complete");
                StartCoroutine(TriggerScene5());
            },
            onDefeat: () =>
            {
                SetFleeDisabled(false);
                UnsubscribeSceneCombat();
                StartCoroutine(RestartScene4AfterCombatCloses());
            },
            onFlee: () =>
            {
                SetFleeDisabled(false);
                UnsubscribeSceneCombat();
                StartCoroutine(RestartDay1AfterCombatCloses());
            }
        );
    }

    private IEnumerator RestartScene4AfterCombatCloses()
    {
        yield return new WaitUntil(() => CombatUI.Instance == null || CombatUI.Instance.combatPanel == null || !CombatUI.Instance.combatPanel.activeSelf);

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();

        SetBackground(3);
        SetCharacter(14);
        TryStartDialogue(3);
    }

    private IEnumerator RestartDay1AfterCombatCloses()
    {
        yield return new WaitUntil(() => CombatUI.Instance == null || CombatUI.Instance.combatPanel == null || !CombatUI.Instance.combatPanel.activeSelf);

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();

        ResetSceneProgress();
    }

    public void WakeUpInBedToRetryDay() => StartCoroutine(WakeUpInBedRoutine());

    IEnumerator WakeUpInBedRoutine()
    {
        _transitionDepth++;
        yield return RunSceneFade(1f, SceneDipOut);
        HideAllPanels();

        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.SetPhase(TimePhase.Morning);

        dayScenarioPending = true;
        Progress = SceneProgress.SceneHome;
        SetBackground(BedroomBackground);
        yield return _waitForSecondsRealtime0_6;
        yield return RunSceneFade(0f, SceneDipIn);
        _transitionDepth--;
        ShowHudPanels();
        ShowForegroundMessage(Loc.T("You wake up in your bed, aching all over. The day begins again.", "Yatağında, her yerin sızlayarak uyanıyorsun. Gün yeniden başlıyor."), 4f);
        SaveSystem.SaveGame();
    }

    public void SkipToNextDay()
    {
        if (TimePhaseManager.Instance == null)
            return;

        if (TimePhaseManager.Instance.currentPhase < earliestSleepPhase)
        {
            ShowForegroundMessage(Loc.T("It's too early to sleep.", "Uyumak için çok erken."), 2f);
            return;
        }

        if (ScenarioManager.Instance != null && ScenarioManager.Instance.IsWaitingAfterRetreat)
        {
            ShowForegroundMessage(Loc.T("Not yet. The fight you ran from is waiting for you in the village square.", "Henüz değil. Kaçtığın dövüş seni köy meydanında bekliyor."), 3f);
            return;
        }

        if (!IsCurrentDayContentComplete())
        {
            if (IsDayStoryWaiting && GetNextDayScenario() != null)
                ShowForegroundMessage(Loc.T("Not yet. Today's story is waiting for you in the village square.", "Henüz değil. Bugünün hikayesi seni köy meydanında bekliyor."), 3f);
            else
                ShowForegroundMessage(Loc.T("Not yet. Finish the quests in your journal before resting.", "Henüz değil. Dinlenmeden önce günlüğündeki görevleri bitir."), 3f);

            return;
        }

        StartCoroutine(ShowSleepingPanelAfterDelay(3f));

        while (TimePhaseManager.Instance.currentPhase != TimePhase.Night)
            TimePhaseManager.Instance.NextPhase();

        TimePhaseManager.Instance.NextPhase();

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();
    }

    private bool IsCurrentDayContentComplete()
    {
        int day = TimeUI.Instance != null ? TimeUI.Instance.GetCurrentDay() : 1;

        if (day <= 1)
            return StoryFlags.Has("day1_complete");

        ScenarioManager sm = ScenarioManager.Instance;

        if (sm == null)
            return true;

        string scenarioID = $"ashenveil_day{day}";

        if (sm.GetScenarioByID(scenarioID) == null)
            return true;

        if (!sm.IsScenarioCompleted(scenarioID))
            return false;

        if (sm.GetScenarioByID($"ashenveil_day{day + 1}") == null && QuestManager.Instance != null)
            foreach (string questID in LastDayQuests)
                if (QuestManager.Instance.IsQuestActive(questID))
                    return false;

        return true;
    }

    private void SyncTimePhaseToScene(SceneProgress scene)
    {
        if (TimePhaseManager.Instance == null)
            return;

        TimePhase? target = scene switch
        {
            SceneProgress.Scene1 => TimePhase.Morning,
            SceneProgress.Scene2 => TimePhase.Morning,
            SceneProgress.Scene3 => TimePhase.Noon,
            SceneProgress.Scene4 => TimePhase.Noon,
            SceneProgress.Scene5 => TimePhase.Noon,
            SceneProgress.Scene6 => TimePhase.Noon,
            SceneProgress.Scene7 => TimePhase.Evening,
            SceneProgress.Scene8 => TimePhase.Evening,
            SceneProgress.Scene9 => TimePhase.Night,
            _ => null
        };

        if (target.HasValue && target.Value > TimePhaseManager.Instance.currentPhase)
            TimePhaseManager.Instance.SetPhase(target.Value);
    }

    private void TryStartDayScenario()
    {
        if (ScenarioManager.Instance == null || !dayScenarioPending || ScenarioManager.Instance.IsScenarioActive())
            return;

        ScenarioData scenario = GetNextDayScenario();

        if (scenario == null)
        {
            TryPlayFinaleCutscene();
            return;
        }

        if (OriginManager.Instance != null && OriginManager.Instance.ShouldHoldDayScenario(scenario.scenarioID))
            return;

        dayScenarioPending = false;

        if (TimePhaseManager.Instance != null && TimePhaseManager.Instance.currentPhase != TimePhase.Morning)
            TimePhaseManager.Instance.SetPhase(TimePhase.Morning);

        ScenarioManager.Instance.StartScenario(scenario);
    }

    private void TryPlayFinaleCutscene()
    {
        const string finaleFlag = "ashenveil_finale_shown";

        if (finaleCutsceneNode == null || DialogueManager.Instance == null || StoryFlags.Has(finaleFlag) || !AllDayScenariosCompleted() || TryStartSharedEpilogue())
            return;

        dayScenarioPending = false;
        StoryFlags.Add(finaleFlag);
        StoryFlags.Add(ChooseEnding());
        DialogueManager.Instance.StartDialogue(finaleCutsceneNode);
    }

    static string ChooseEnding()
    {
        int serena = AffinityManager.Instance != null ? AffinityManager.Instance.Get("Serena") : 0;

        if (StoryFlags.Has(AshenveilVossWeakPoint.SerenaBondBroken) && serena >= 35)
            return EndingTwoInTheDoor;

        return StoryFlags.Has("court_parley") ? EndingOpenAccount : EndingDoorHeld;
    }

    private bool TryStartSharedEpilogue()
    {
        ScenarioManager sm = ScenarioManager.Instance;
        ScenarioData epilogue = sm != null ? sm.GetScenarioByID(SharedEpilogueScenario) : null;

        if (epilogue == null || sm.IsScenarioCompleted(SharedEpilogueScenario))
            return false;

        StoryFlags.Add(SharedEpilogueStartFlag);

        if (!sm.IsScenarioActive() && sm.CanStartScenario(epilogue))
            sm.StartScenario(epilogue);

        return sm.IsScenarioActive();
    }

    private bool AllDayScenariosCompleted()
    {
        ScenarioManager sm = ScenarioManager.Instance;

        if (sm == null || sm.availableScenarios == null)
            return false;

        bool anyDayScenario = false;

        foreach (ScenarioData scenario in sm.availableScenarios)
        {
            if (scenario == null || string.IsNullOrEmpty(scenario.scenarioID) || !scenario.scenarioID.StartsWith("ashenveil_day"))
                continue;

            anyDayScenario = true;

            if (!sm.IsScenarioCompleted(scenario.scenarioID))
                return false;
        }

        return anyDayScenario;
    }

    private ScenarioData GetNextDayScenario()
    {
        ScenarioManager sm = ScenarioManager.Instance;

        if (sm == null || sm.availableScenarios == null)
            return null;

        foreach (ScenarioData scenario in sm.availableScenarios)
        {
            if (scenario == null || string.IsNullOrEmpty(scenario.scenarioID) || !scenario.scenarioID.StartsWith("ashenveil_day") || sm.IsScenarioCompleted(scenario.scenarioID))
                continue;

            if (sm.CanStartScenario(scenario))
                return scenario;
        }

        return null;
    }

    IEnumerator ShowSleepingPanelAfterDelay(float delay)
    {
        SetActive(sleepingPanel, true);
        yield return new WaitForSeconds(delay);
        SetActive(sleepingPanel, false);
    }

    public void ShowForegroundMessage(string message, float seconds)
    {
        if (ForegroundNotifier.Instance != null)
            ForegroundNotifier.Instance.ShowMessage(message, seconds);
    }

    public void StartMapCombat(EnemyData enemy)
    {
        if (enemy == null || CombatManager.Instance == null || CombatManager.Instance.inCombat)
            return;

        HideAllPanels();
        CombatManager.Instance.StartCombat(enemy);
    }

    public void ShowGardenHole() => SetBackground(38);

    public void ShowPool() => SetBackground(39);

    private Sprite ResolveQuestLocationBg(string bgName, TimePhase phase)
    {
        if (questLocationBackgrounds != null)
            foreach (var qb in questLocationBackgrounds)
                if (qb.name == bgName)
                    return qb.Resolve(phase);

        return null;
    }

    public void ShowQuestLocation(string bgName)
    {
        if (string.IsNullOrEmpty(bgName) || backgroundImage == null)
            return;

        TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;
        Sprite spr = ResolveQuestLocationBg(bgName, phase);

        if (spr == null)
            return;

        backgroundImage.sprite = spr;
        _currentQuestLocation = bgName;
        _lastBgIndex = -1;
        CloseAnimated(mapPanel);
        ClearDialogueBackground();
        SetActive(townNpc, false);
        ForceHideSceneCharacter();
        ApplyHoverVisibility(-1);
        ApplyItemVisibility(-1);
        ApplyHouseIconVisibility(false);

        if (bgName == "bg_templeborn_archives")
            StartTeaHandIn();
    }

    private void StartTeaHandIn()
    {
        if (QuestManager.Instance == null || !QuestManager.Instance.IsQuestActive("q01_bir_fincan_huzur"))
            return;

        var objective = QuestManager.Instance.GetObjectiveState("q01_bir_fincan_huzur", "q01_obj4");

        if (objective == null || objective.isCompleted)
            return;

        bool hasCup = InventoryManager.Instance != null && InventoryManager.Instance.GetTotalQuantity("apple_tea") > 0;

        if (hasCup && marenTeaDialogue != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(marenTeaDialogue, DeliverTeaToMaren);
            return;
        }

        DeliverTeaToMaren();
    }

    public void CollectAppleTeaSeed() => CollectWorldItem(WorldItemIndex.AppleTeaSeed);

    public void CollectHistoryBook() => CollectWorldItem(WorldItemIndex.HistoryBook);

    public void CollectApple() => CollectWorldItem(WorldItemIndex.Apple);

    public void CollectCinnamon() => CollectWorldItem(WorldItemIndex.Cinnamon);

    private void CollectWorldItem(int index)
    {
        if (worldItems == null || index < 0 || index >= worldItems.Length || worldItems[index].item == null)
            return;

        if (worldItems[index].item.TryGetComponent<WorldItem>(out var worldItem))
            worldItem.Collect();
        else
            Destroy(worldItems[index].item);
    }

    public void OpenStove(int index)
    {
        SetBackground(index > 0 ? index : 17);
        TryBrewTeaAtStove();
    }

    private void TryBrewTeaAtStove()
    {
        const string questID = "q01_bir_fincan_huzur";
        const string brewObjectiveID = "q01_obj3";

        if (QuestManager.Instance == null || !QuestManager.Instance.IsQuestActive(questID))
            return;

        var objective = QuestManager.Instance.GetObjectiveState(questID, brewObjectiveID);

        if (objective == null || objective.isCompleted)
            return;

        var inv = InventoryManager.Instance;
        var db = ItemDatabase.Instance;
        ItemData apple = db != null ? db.GetByID("fresh_apple") : null;
        ItemData cinnamon = db != null ? db.GetByID("cinnamon") : null;

        if (inv == null || apple == null || cinnamon == null || inv.GetTotalQuantity("fresh_apple") <= 0 || inv.GetTotalQuantity("cinnamon") <= 0)
        {
            ShowForegroundMessage(Loc.T("You need a Fresh Apple and Cinnamon to brew the tea.", "Çayı demlemek için bir Taze Elma ve Tarçın gerekiyor."), 3f);
            return;
        }

        inv.RemoveItem(apple);
        inv.RemoveItem(cinnamon);
        QuestManager.Instance.UpdateObjectiveProgress(questID, brewObjectiveID, 1);
        ItemData tea = db.GetByID("apple_tea");

        if (tea != null)
            inv.AddItem(tea);

        ShowForegroundMessage(Loc.T("The tea is brewed. Use the cup from your inventory to give it to Maren.", "Çay demlendi. Maren'e vermek için fincanı envanterden kullan."), 3.5f);
    }

    void HandleItemUsed(ItemData item)
    {
        if (item != null && item.itemID == "apple_tea")
            DeliverTeaToMaren();
    }

    public void DeliverTeaToMaren()
    {
        const string questID = "q01_bir_fincan_huzur";
        const string deliverObjectiveID = "q01_obj4";

        if (QuestManager.Instance == null || !QuestManager.Instance.IsQuestActive(questID))
            return;

        var objective = QuestManager.Instance.GetObjectiveState(questID, deliverObjectiveID);

        if (objective == null || objective.isCompleted)
            return;

        ItemData tea = ItemDatabase.Instance != null ? ItemDatabase.Instance.GetByID("apple_tea") : null;

        if (tea == null || InventoryManager.Instance == null || InventoryManager.Instance.GetTotalQuantity("apple_tea") <= 0)
        {
            ShowForegroundMessage(Loc.T("You need to brew the tea at the stove first.", "Önce ocakta çayı demlemen gerek."), 2.5f);
            return;
        }

        InventoryManager.Instance.RemoveItem(tea);

        if (marenDeliveryNode != null && DialogueManager.Instance != null && !DialogueManager.Instance.IsInDialogue())
        {
            DialogueManager.Instance.StartDialogue(marenDeliveryNode, () => CompleteMarenDelivery(questID, deliverObjectiveID));
            return;
        }

        CompleteMarenDelivery(questID, deliverObjectiveID);
    }

    void CompleteMarenDelivery(string questID, string deliverObjectiveID)
    {
        if (QuestManager.Instance != null)
            QuestManager.Instance.UpdateObjectiveProgress(questID, deliverObjectiveID, 1);

        ShowForegroundMessage(Loc.T("You gave the cup to Maren.", "Fincanı Maren'e verdin."), 3f);
    }

    private void OnDoorClicked()
    {
        _doorClicked = true;

        if (Progress != SceneProgress.Scene1)
            Progress = SceneProgress.Scene1;

        TriggerScene2();
    }

    public void StartCashierDialogue() => TriggerMarketScene();

    public void StartNunDialogue() => TriggerChurchScene();

    public void StartCoachDialogue() => TriggerGymScene();

    public void StartStepSisterDialogue() => TriggerHomeScene();

    public void StartOfficerDialogue() => TriggerOfficeScene();

    public void TriggerHomeScene()
    {
        Progress = SceneProgress.SceneHome;
        TryStartDialogue(12);
    }

    public void TriggerMarketScene()
    {
        Progress = SceneProgress.SceneMarket;
        TryStartDialogue(9);
    }

    public void TriggerGymScene()
    {
        Progress = SceneProgress.SceneGym;
        TryStartDialogue(11);
    }

    public void TriggerOfficeScene()
    {
        Progress = SceneProgress.SceneOffice;
        TryStartDialogue(13);
    }

    public void TriggerChurchScene()
    {
        Progress = SceneProgress.SceneChurch;
        TryStartDialogue(10);
    }

    public void TriggerScene2()
    {
        if (Progress != SceneProgress.Scene1)
            return;

        Progress = SceneProgress.Scene2;
        SetBackground(1);
        SetCharacter(8);
        TryStartDialogue(1);
    }

    public void TriggerScene3()
    {
        if (Progress != SceneProgress.Scene2)
            return;

        Progress = SceneProgress.Scene3;
        SetBackground(2);
        SetCharacter(8);
        TryStartDialogue(2);
    }

    public void TriggerScene4()
    {
        if (Progress != SceneProgress.Scene3)
            return;

        Progress = SceneProgress.Scene4;
        SetBackground(3);
        TryStartDialogue(3);
    }

    private IEnumerator TriggerScene5()
    {
        if (Progress != SceneProgress.Scene4)
            yield break;

        yield return new WaitUntil(() => CombatUI.Instance == null || CombatUI.Instance.combatPanel == null || !CombatUI.Instance.combatPanel.activeSelf);
        Progress = SceneProgress.Scene5;
        SetCharacter(8);
        TryStartDialogue(4);
    }

    public void TriggerScene6()
    {
        if (Progress != SceneProgress.Scene5)
            return;

        Progress = SceneProgress.Scene6;
        SetBackground(4);
        SetCharacter(8);
        TryStartDialogue(5);
    }

    public void TriggerScene7()
    {
        if (Progress != SceneProgress.Scene6)
            return;

        Progress = SceneProgress.Scene7;
        SetBackground(5);
        SetCharacter(8);
        TryStartDialogue(6);
    }

    public void TriggerScene8()
    {
        if (Progress != SceneProgress.Scene7)
            return;

        Progress = SceneProgress.Scene8;
        SetBackground(6);
        SetCharacter(8);
        TryStartDialogue(7);
    }

    public void TriggerScene9()
    {
        if (Progress != SceneProgress.Scene8)
            return;

        Progress = SceneProgress.Scene9;
        SetBackground(7);
        SetCharacter(8);
        TryStartDialogue(8);
    }
}
