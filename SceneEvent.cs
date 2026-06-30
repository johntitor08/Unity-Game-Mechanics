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
    private static readonly int DialoguePanelCloseHash = Animator.StringToHash("DialoguePanelClose");
    private static readonly WaitForSecondsRealtime _waitForSecondsRealtime0_12 = new(0.12f);
    private static readonly WaitForSecondsRealtime _bgSwapPause = new(0.2f);
    public static SceneEvent Instance { get; private set; }
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

    public bool DayScenarioPending
    {
        get => dayScenarioPending;
        set => dayScenarioPending = value;
    }

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

            return s != null ? s : morning;
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
        public Sprite eveningSprite;

        public readonly Sprite Resolve(TimePhase phase)
        {
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

        if (index == 0)
            TryStartDayScenario();

        OnBackgroundChanged?.Invoke(index);
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
                mapTitleText.text = "Neighborhood";
            else if (isFantasy)
                mapTitleText.text = "Ashenveil Town";
            else if (isCombat)
                mapTitleText.text = "Combat Region";
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
                visible = visible && IsQuestIconActive(loc) && !IsNight();

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

            _charFadingOut = false;

            if (charImage.gameObject.activeSelf)
            {
                Color cc = charImage.color;
                charImage.color = new Color(cc.r, cc.g, cc.b, 1f);
            }

            if (node != null && node.characterImage != null)
            {
                _dialogueBgPending = node.backgroundImage;
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

        _dialogueBgPending = node.backgroundImage;
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
            charImage.gameObject.SetActive(true);
            Color c = charImage.color;
            charImage.color = new Color(c.r, c.g, c.b, 1f);
            ApplyDialogueCharacterLayout();

            bool bgChanged = _dialogueBgPending != null && _dialogueBgPending != _dialogueBgCurrent;

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
        else
        {
            if (_dialogueBgPending != null)
            {
                _dialogueBgCurrent = _dialogueBgPending;

                if (DialogueManager.Instance != null && DialogueManager.Instance.backgroundImage != null)
                {
                    DialogueManager.Instance.backgroundImage.sprite = _dialogueBgPending;
                    DialogueManager.Instance.backgroundImage.enabled = true;
                }
            }

            charFadeCoroutine = StartCoroutine(FadeInCharacter(sprite));
        }
    }

    IEnumerator SwapBackgroundOnly(Sprite newBg)
    {
        yield return _bgSwapPause;

        if (newBg != null && DialogueManager.Instance != null && DialogueManager.Instance.backgroundImage != null)
        {
            DialogueManager.Instance.backgroundImage.sprite = newBg;
            DialogueManager.Instance.backgroundImage.enabled = true;
        }

        _dialogueBgCurrent = newBg;
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
        yield return _bgSwapPause;

        if (_dialogueBgPending != null && DialogueManager.Instance != null && DialogueManager.Instance.backgroundImage != null)
        {
            DialogueManager.Instance.backgroundImage.sprite = _dialogueBgPending;
            DialogueManager.Instance.backgroundImage.enabled = true;
        }

        if (_dialogueBgPending != null)
            _dialogueBgCurrent = _dialogueBgPending;

        charImage.sprite = newSprite;
        ApplyDialogueCharacterLayout();
        charImage.gameObject.SetActive(true);
        charImage.color = new Color(col.r, col.g, col.b, 0f);

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
        {
            HandleSceneTransition(endedNode);
            ClearDialogueBackground();
        }
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
        rt.anchoredPosition = dialogueCharacterPosition;
        rt.sizeDelta = dialogueCharacterSize;
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

            charFadeCoroutine = StartCoroutine(DeferredFadeOut(0.5f, isFinal ? endedNode : null));
        }
        else if (isFinal)
        {
            HandleSceneTransition(endedNode);
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

    IEnumerator DeferredFadeOut(float duration, DialogueNode endedNode)
    {
        _charFadingOut = true;
        yield return _waitForSecondsRealtime0_12;
        yield return FadeOutCharacter(duration, endedNode);
    }

    void OnScenarioStarted(ScenarioData scenario) => ForceHideSceneCharacter();

    void OnScenarioCompleted(ScenarioData scenario)
    {
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
                if (endedNode == sceneStartDialogueNodes[4])
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
        if (dialoguePanelAnimator == null)
            return;

        dialoguePanelAnimator.ResetTrigger(dialoguePanelOpenTrigger);
        dialoguePanelAnimator.SetTrigger(dialoguePanelCloseTrigger);

        if (dialoguePanelAnimator.HasState(0, Animator.StringToHash("DialoguePanelClose")))
            dialoguePanelAnimator.Play(DialoguePanelCloseHash, 0, 0f);
    }

    public float DialogueOpenAnimationDuration() => 0f;

    public bool IsSceneTransitionActive() => charFadeCoroutine != null;

    public float DialogueCloseAnimationDuration()
    {
        if (dialoguePanelAnimator != null)
        {
            AnimatorStateInfo info = dialoguePanelAnimator.GetCurrentAnimatorStateInfo(0);

            if (info.length > 0f)
                return info.length;
        }

        return dialogueCloseFallbackDuration;
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
            node.speakerName = ProfileManager.Instance != null && ProfileManager.Instance.profile != null ? ProfileManager.Instance.profile.playerName : "You";
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
            Action<EnemyData> onVictory = null;

            void Cleanup()
            {
                CombatManager.Instance.OnCombatDefeat -= onDefeat;
                CombatManager.Instance.OnCombatVictory -= onVictory;
            }

            onDefeat = () =>
            {
                Cleanup();

                if (region != null)
                    region.SetActive(true);
            };

            onVictory = (_) => Cleanup();

            CombatManager.Instance.OnCombatDefeat += onDefeat;
            CombatManager.Instance.OnCombatVictory += onVictory;
            CombatManager.Instance.StartCombat(entry.questEnemy);
        });
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
                charImage.sprite = s;
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
        StartCombatForScene(
            enemyIndex: 0,
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

    public void SkipToNextDay()
    {
        if (TimePhaseManager.Instance == null)
            return;

        if (TimePhaseManager.Instance.currentPhase < earliestSleepPhase)
        {
            ShowForegroundMessage("It's too early to sleep.", 2f);
            return;
        }

        if (!IsCurrentDayContentComplete())
        {
            ShowForegroundMessage("You should finish today's events before resting.", 2.5f);
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

        return sm.IsScenarioCompleted(scenarioID);
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
            SceneProgress.Scene6 => TimePhase.Evening,
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

        dayScenarioPending = false;
        ScenarioManager.Instance.StartScenario(scenario);
    }

    private void TryPlayFinaleCutscene()
    {
        const string finaleFlag = "ashenveil_finale_shown";

        if (finaleCutsceneNode == null || DialogueManager.Instance == null || StoryFlags.Has(finaleFlag) || !AllDayScenariosCompleted())
            return;

        dayScenarioPending = false;
        StoryFlags.Add(finaleFlag);
        DialogueManager.Instance.StartDialogue(finaleCutsceneNode);
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

    public void ShowQuestLocation(string bgName)
    {
        if (string.IsNullOrEmpty(bgName) || backgroundImage == null)
            return;

        Sprite spr = null;
        TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;

        if (questLocationBackgrounds != null)
            foreach (var qb in questLocationBackgrounds)
                if (qb.name == bgName)
                {
                    spr = qb.Resolve(phase);
                    break;
                }

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
            ShowForegroundMessage("You need a Fresh Apple and Cinnamon to brew the tea.", 3f);
            return;
        }

        inv.RemoveItem(apple);
        inv.RemoveItem(cinnamon);
        QuestManager.Instance.UpdateObjectiveProgress(questID, brewObjectiveID, 1);
        ItemData tea = db.GetByID("apple_tea");

        if (tea != null)
            inv.AddItem(tea);

        ShowForegroundMessage("The tea is brewed — you got a cup of apple tea.", 3f);
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
            ShowForegroundMessage("You need to brew the tea at the stove first.", 2.5f);
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

        ShowForegroundMessage("You gave the cup to Maren.", 3f);
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
