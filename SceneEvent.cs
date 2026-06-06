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
    public static SceneEvent Instance { get; private set; }
    private SceneProgress progress = SceneProgress.Scene1;
    private int currentMapIndex = 0;
    private int _validMapCursor = 0;
    private static readonly int[] validMapIndices = { 6, 8, 13 };
    public ItemDatabase itemDatabase;
    private bool isDialogueSubscribed;
    private bool isHoverEffectsSubscribed;
    public GameObject[] hoverEffects;
    public GameObject[] items;
    private readonly List<System.Action> _hoverHideActions = new();
    private Action<EnemyData> _currentVictoryHandler;
    private Action _currentDefeatHandler;
    private Vector2 _officeIconDefaultPos;
    public TMP_Text mapTitleText;

    public SceneProgress Progress
    {
        get => progress;

        private set
        {
            if (progress == value)
                return;

            progress = value;
            SaveSystem.SaveGame();
        }
    }

    [Header("Backgrounds")]
    public Image backgroundImage;
    public Sprite[] bgs;

    [Header("Characters")]
    public Image charImage;
    public Sprite[] chars;

    [Header("Dialogues")]
    public DialogueNode[] sceneStartDialogueNodes;

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

    [Header("UI Icons")]
    public GameObject settingsIcon;
    public GameObject profileIcon;
    public GameObject inventoryIcon;
    public GameObject mapIcon;
    public GameObject coinIcon;
    public GameObject questIcon;
    public GameObject combatIcon;
    public GameObject homeIcon;
    public GameObject marketIcon;
    public GameObject gymIcon;
    public GameObject officeIcon;
    public GameObject churchIcon;
    public GameObject townIcon;
    public GameObject libraryIcon;
    public GameObject dungeonIcon;
    public GameObject castleIcon;
    public GameObject arenaIcon;

    [Header("House Icons")]
    public GameObject livingRoomIcon;
    public GameObject bedroomIcon;
    public GameObject tvRoomIcon;
    public GameObject kitchenIcon;
    public GameObject bathroomIcon;
    public GameObject gardenIcon;

    [Header("UI Buttons")]
    public GameObject closeMapButton;
    public GameObject closeCombatMapButton;

    [Header("Animation")]
    public Animator timePanelAnimator;
    public Animator iconPanelAnimator;
    public Animator dialoguePanelAnimator;
    public string timePanelOpenTrigger = "TimePanelOpened";
    public string timePanelCloseTrigger = "TimePanelClosed";
    public string iconPanelOpenTrigger = "IconPanelOpened";
    public string iconPanelCloseTrigger = "IconPanelClosed";
    public string dialoguePanelOpenTrigger = "DialoguePanelOpened";
    public string dialoguePanelCloseTrigger = "DialoguePanelClosed";
    public float dialogueCloseFallbackDuration = 0.25f;

    [Header("Icon Settings")]
    public bool closeOtherPanelsOnOpen = true;
    public bool allowMultiplePanels = false;

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

        if (officeIcon != null)
            _officeIconDefaultPos = officeIcon.GetComponent<RectTransform>().anchoredPosition;
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

        if (charImage != null)
            charImage.gameObject.SetActive(false);
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
    }

    void OnDisable()
    {
        UnsubscribeDialogue();
        UnsubscribeHoverEffects();

        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= OnPhaseChanged;
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

        if (combatIcon != null)
            combatIcon.GetComponentInChildren<Button>().onClick.AddListener(() => TogglePanel(combatMapPanel, "Combat Map"));

        if (closeMapButton != null)
            closeMapButton.GetComponent<Button>().onClick.AddListener(HideAllPanels);

        if (closeCombatMapButton != null)
            closeCombatMapButton.GetComponent<Button>().onClick.AddListener(HideAllPanels);

        SetActive(homeIcon, false);
        SetActive(combatIcon, false);
        SetActive(marketIcon, false);
        SetActive(gymIcon, false);
        SetActive(officeIcon, false);
        SetActive(churchIcon, false);
        SetActive(townIcon, false);
        SetActive(livingRoomIcon, false);
        SetActive(bedroomIcon, false);
        SetActive(tvRoomIcon, false);
        SetActive(kitchenIcon, false);
        SetActive(bathroomIcon, false);
        SetActive(gardenIcon, false);
        SetActive(libraryIcon, false);
        SetActive(dungeonIcon, false);
        SetActive(castleIcon, false);
        SetActive(arenaIcon, false);
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

        panel.SetActive(true);
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

    public void HideAllPanels()
    {
        SetActive(settingsPanel, false);
        SetActive(savePanel, false);
        SetActive(profilePanel, false);
        SetActive(inventoryPanel, false);
        SetActive(mapPanel, false);
        SetActive(equipmentPanel, false);
        SetActive(coinPanel, false);
        SetActive(questPanel, false);
        SetActive(combatMapPanel, false);

        if (MarketUI.Instance != null)
            MarketUI.Instance.CloseAll();
        else
            SetActive(shopPanel, false);
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

        panel.SetActive(!isActive);

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
        if (inventoryPanel != null)
            inventoryPanel.SetActive(!inventoryPanel.activeSelf);
    }

    public void SetCharacter(int index)
    {
        if (charImage != null && index >= 0 && index < chars.Length && chars[index] != null)
            charImage.sprite = chars[index];
    }

    public void SetBackground(int index)
    {
        if (backgroundImage != null && index >= 0 && index < bgs.Length && bgs[index] != null)
            backgroundImage.sprite = bgs[index];

        if (hoverEffects != null && hoverEffects.Length >= 6)
        {
            SetActive(hoverEffects[0], index == 0 && Progress == SceneProgress.Scene1);
            SetActive(hoverEffects[1], index == 12);
            SetActive(hoverEffects[2], index == 10);
            SetActive(hoverEffects[3], index == 11);
            SetActive(hoverEffects[4], index == 9);
            SetActive(hoverEffects[5], index == 8);
            SetActive(hoverEffects[6], index == 13);
            SetActive(hoverEffects[7], index == 15);
            SetActive(items[0], index == 38);
            SetActive(hoverEffects[9], index == 15);
        }

        bool isHouse = index == 11 || index == 13 || index == 14 || index == 15 || index == 16 || index == 17 || index == 20 || index == 38 || index == 39 || index == 40;
        SetActive(houseIconsPanel, isHouse);
        SetActive(livingRoomIcon, isHouse);
        SetActive(bedroomIcon, isHouse);
        SetActive(tvRoomIcon, isHouse);
        SetActive(kitchenIcon, isHouse);
        SetActive(bathroomIcon, isHouse);
        SetActive(gardenIcon, isHouse);
        bool showChar = index >= 8 && index <= 12;
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
                SetCharacter(23);
            }
            else if (index == 11)
            {
                charRt.anchorMin = new Vector2(0.5f, 0);
                charRt.anchorMax = new Vector2(0.5f, 0);
                charRt.pivot = new Vector2(0.5f, 0.5f);
                charRt.anchoredPosition = new Vector2(375f, 375f);
                charRt.sizeDelta = new Vector2(75f, 75f);
                charRt.localScale = new Vector3(10f, 10f, 10f);
                SetCharacter(26);
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
                    SetCharacter(21);
                else if (index == 10)
                    SetCharacter(25);
                else if (index == 12)
                    SetCharacter(28);
            }
        }
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
        SetActive(homeIcon, isModern);
        SetActive(marketIcon, isModern);
        SetActive(gymIcon, isModern);
        SetActive(officeIcon, isModern);
        SetActive(churchIcon, isModern);
        SetActive(townIcon, isFantasy);
        SetActive(libraryIcon, isFantasy);
        SetActive(dungeonIcon, isFantasy);
        SetActive(castleIcon, isFantasy);
        SetActive(arenaIcon, isFantasy);
        SetActive(combatIcon, isCombat);

        if (isModern)
        {
            TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;
            UpdateModernIconStates(phase);
            mapTitleText.text = "Ashenveil Town";
        }
        else if (isFantasy)
        {
            mapTitleText.text = "Neighborhood";
        }
        else if (isCombat)
        {
            mapTitleText.text = "Combat Region";
        }
    }

    public void SetCombatMap(int index)
    {
        if (combatMapImage == null || (index < 0 || index >= maps.Length || maps[index] == null))
            return;

        combatMapImage.sprite = maps[index];
    }

    private void UpdateModernIconStates(TimePhase phase)
    {
        SetInteractable(homeIcon, true);
        SetInteractable(marketIcon, phase == TimePhase.Morning || phase == TimePhase.Noon || phase == TimePhase.Evening);
        SetInteractable(gymIcon, phase == TimePhase.Morning || phase == TimePhase.Noon || phase == TimePhase.Evening);
        SetInteractable(officeIcon, phase == TimePhase.Noon || phase == TimePhase.Evening);
        SetInteractable(churchIcon, phase == TimePhase.Morning || phase == TimePhase.Noon);

        if (officeIcon != null && officeIcon.TryGetComponent<RectTransform>(out var rt))
        {
            bool isShifted = phase == TimePhase.Noon || phase == TimePhase.Evening;
            rt.anchoredPosition = isShifted ? new Vector2(_officeIconDefaultPos.x - 75f, _officeIconDefaultPos.y) : _officeIconDefaultPos;
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
        isDialogueSubscribed = true;
    }

    public void UnsubscribeDialogue()
    {
        if (!isDialogueSubscribed || DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.OnDialogueStart -= HandleDialogueStart;
        DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        DialogueManager.Instance.OnLineShown -= HandleLineShown;
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

        if (sceneStartDialogueNodes == null || nodeIndex >= sceneStartDialogueNodes.Length || Progress == SceneProgress.SceneHome || Progress == SceneProgress.SceneMarket || Progress == SceneProgress.SceneGym || Progress == SceneProgress.SceneOffice || Progress == SceneProgress.SceneChurch)
            yield break;

        if (DialogueManager.Instance == null)
        {
            Debug.LogWarning("[SceneEvent] StartDialogueAfterLoad: DialogueManager is null.");
            yield break;
        }

        DialogueNode node = sceneStartDialogueNodes[nodeIndex];

        if (node != null)
            DialogueManager.Instance.StartDialogue(node);
    }

    void HandleDialogueStart(DialogueNode node)
    {
        if (timePanelAnimator != null)
            timePanelAnimator.SetTrigger(timePanelCloseTrigger);

        if (iconPanelAnimator != null)
            iconPanelAnimator.SetTrigger(iconPanelCloseTrigger);

        if (charImage != null)
            charImage.gameObject.SetActive(true);
    }

    bool IsInDialogue() => DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue();

    void HandleDialogueEnd(DialogueNode endedNode)
    {
        if (endedNode == null || !endedNode.isFinalNode)
            return;

        SetActive(settingsIconPanel, true);
        SetActive(timePanel, true);
        SetActive(iconPanel, true);

        if (timePanelAnimator != null)
            timePanelAnimator.SetTrigger(timePanelOpenTrigger);

        if (iconPanelAnimator != null)
            iconPanelAnimator.SetTrigger(iconPanelOpenTrigger);

        if (charImage != null)
            StartCoroutine(FadeOutCharacter(0.5f, endedNode));
        else
            HandleSceneTransition(endedNode);

        if (Progress == SceneProgress.SceneMarket && sceneStartDialogueNodes != null && sceneStartDialogueNodes.Length > 8 && sceneStartDialogueNodes[8] != null && endedNode == sceneStartDialogueNodes[8] && MarketUI.Instance != null)
            MarketUI.Instance.OpenMarket();
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

    public void OpenDialoguePanel()
    {
        if (dialoguePanelAnimator != null)
            dialoguePanelAnimator.SetTrigger(dialoguePanelOpenTrigger);
    }

    public void CloseDialoguePanel()
    {
        if (dialoguePanelAnimator != null)
            dialoguePanelAnimator.SetTrigger(dialoguePanelCloseTrigger);
    }

    public float DialogueOpenAnimationDuration() => 0f;

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

    void HandleLineShown(DialogueNode node, int lineIndex)
    {
        if (node == sceneStartDialogueNodes[0] && lineIndex == 1)
            SetCharacter(8);

        if (node == sceneStartDialogueNodes[3] && lineIndex == 0)
            SetCharacter(14);
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
                break;

            case SceneProgress.Scene8:
                SetBackground(6);
                break;

            case SceneProgress.Scene9:
                SetBackground(7);
                SetCharacter(15);
                break;

            case SceneProgress.SceneHome:
                SetBackground(11);
                SetCharacter(26);
                break;

            case SceneProgress.SceneMarket:
                SetBackground(12);
                SetCharacter(28);
                break;

            case SceneProgress.SceneGym:
                SetBackground(8);
                SetCharacter(21);
                break;

            case SceneProgress.SceneOffice:
                SetBackground(9);
                SetCharacter(23);
                break;

            case SceneProgress.SceneChurch:
                SetBackground(10);
                SetCharacter(25);
                break;
        }
    }

    private void SubscribeHoverEffects()
    {
        if (hoverEffects == null || hoverEffects.Length == 0 || isHoverEffectsSubscribed)
            return;

        _hoverHideActions.Clear();

        foreach (var effect in hoverEffects)
        {
            if (effect == null)
                continue;

            if (effect.TryGetComponent<UIHoverRegion>(out var hover))
            {
                var captured = effect;
                void hideAction() => SetActive(captured, false);
                _hoverHideActions.Add(hideAction);
                hover.OnRegionClicked += hideAction;
            }
        }

        if (hoverEffects[0] != null)
            hoverEffects[0].GetComponent<UIHoverRegion>().OnRegionClicked += TriggerScene2;

        if (hoverEffects[1] != null)
            hoverEffects[1].GetComponent<UIHoverRegion>().OnRegionClicked += StartCashierDialogue;

        if (hoverEffects[2] != null)
            hoverEffects[2].GetComponent<UIHoverRegion>().OnRegionClicked += StartNunDialogue;

        if (hoverEffects[3] != null)
            hoverEffects[3].GetComponent<UIHoverRegion>().OnRegionClicked += StartStepSisterDialogue;

        if (hoverEffects[4] != null)
            hoverEffects[4].GetComponent<UIHoverRegion>().OnRegionClicked += StartOfficerDialogue;

        if (hoverEffects[5] != null)
            hoverEffects[5].GetComponent<UIHoverRegion>().OnRegionClicked += StartCoachDialogue;

        if (hoverEffects[6] != null)
            hoverEffects[6].GetComponent<UIHoverRegion>().OnRegionClicked += SkipToNextDay;

        if (hoverEffects[7] != null)
            hoverEffects[7].GetComponent<UIHoverRegion>().OnRegionClicked += ShowGardenHole;

        if (hoverEffects[8] != null)
            hoverEffects[8].GetComponent<UIHoverRegion>().OnRegionClicked += CollectAppleTeaSeed;

        if (hoverEffects[9] != null)
            hoverEffects[9].GetComponent<UIHoverRegion>().OnRegionClicked += ShowPool;

        isHoverEffectsSubscribed = true;
    }

    public void UnsubscribeHoverEffects()
    {
        if (hoverEffects == null || hoverEffects.Length == 0 || !isHoverEffectsSubscribed)
            return;

        for (int i = 0; i < hoverEffects.Length && i < _hoverHideActions.Count; i++)
        {
            if (hoverEffects[i] == null)
                continue;

            if (hoverEffects[i].TryGetComponent<UIHoverRegion>(out var hover))
                hover.OnRegionClicked -= _hoverHideActions[i];
        }

        _hoverHideActions.Clear();

        if (hoverEffects[0] != null)
            hoverEffects[0].GetComponent<UIHoverRegion>().OnRegionClicked -= TriggerScene2;

        if (hoverEffects[1] != null)
            hoverEffects[1].GetComponent<UIHoverRegion>().OnRegionClicked -= StartCashierDialogue;

        if (hoverEffects[2] != null)
            hoverEffects[2].GetComponent<UIHoverRegion>().OnRegionClicked -= StartNunDialogue;

        if (hoverEffects[3] != null)
            hoverEffects[3].GetComponent<UIHoverRegion>().OnRegionClicked -= StartStepSisterDialogue;

        if (hoverEffects[4] != null)
            hoverEffects[4].GetComponent<UIHoverRegion>().OnRegionClicked -= StartOfficerDialogue;

        if (hoverEffects[5] != null)
            hoverEffects[5].GetComponent<UIHoverRegion>().OnRegionClicked -= StartCoachDialogue;

        if (hoverEffects[6] != null)
            hoverEffects[6].GetComponent<UIHoverRegion>().OnRegionClicked -= SkipToNextDay;

        if (hoverEffects[7] != null)
            hoverEffects[7].GetComponent<UIHoverRegion>().OnRegionClicked -= ShowGardenHole;

        if (hoverEffects[8] != null)
            hoverEffects[8].GetComponent<UIHoverRegion>().OnRegionClicked -= CollectAppleTeaSeed;

        if (hoverEffects[9] != null)
            hoverEffects[9].GetComponent<UIHoverRegion>().OnRegionClicked -= ShowPool;

        isHoverEffectsSubscribed = false;
    }

    public void ResetSceneProgress()
    {
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

    static void SetInteractable(GameObject go, bool interactable)
    {
        if (go == null)
            return;

        Button btn = go.GetComponentInChildren<Button>();

        if (btn != null)
            btn.interactable = interactable;
    }

    private void OnPhaseChanged(TimePhase _)
    {
        if (mapPanel != null && mapPanel.activeSelf)
            SetMap(currentMapIndex);
    }

    IEnumerator FadeOutCharacter(float duration, DialogueNode endedNode = null)
    {
        if (charImage == null)
            yield break;

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

        if (endedNode != null)
            HandleSceneTransition(endedNode);
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

    private void StartCombatForScene(int enemyIndex, Action<EnemyData> onVictory, Action onDefeat)
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
        cm.OnCombatVictory += _currentVictoryHandler;
        cm.OnCombatDefeat += _currentDefeatHandler;
        SetFleeDisabled(true);
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
    }

    private void StartCombatForScene4()
    {
        StartCombatForScene(
            enemyIndex: 0,
            onVictory: (enemy) =>
            {
                SetFleeDisabled(false);
                UnsubscribeSceneCombat();
                StartCoroutine(TriggerScene5());
            },
            onDefeat: () =>
            {
                SetFleeDisabled(false);
                UnsubscribeSceneCombat();
                SetCharacter(8);
            }
        );
    }

    public void SkipToNextDay()
    {
        if (TimePhaseManager.Instance == null)
            return;

        StartCoroutine(ShowSleepingPanelAfterDelay(3f));

        while (TimePhaseManager.Instance.currentPhase != TimePhase.Night)
            TimePhaseManager.Instance.NextPhase();

        TimePhaseManager.Instance.NextPhase();

        if (PlayerStats.Instance != null)
            PlayerStats.Instance.FullRestore();
    }

    IEnumerator ShowSleepingPanelAfterDelay(float delay)
    {
        SetActive(sleepingPanel, true);
        yield return new WaitForSeconds(delay);
        SetActive(sleepingPanel, false);
    }

    public void ShowGardenHole() => SetBackground(38);

    public void CollectAppleTeaSeed()
    {
        Destroy(items[0]);
        Debug.Log("Apple Tea Seed is collected.");
    }

    public void ShowPool() => SetBackground(39);

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
        TryStartDialogue(1);
    }

    public void TriggerScene3()
    {
        if (Progress != SceneProgress.Scene2)
            return;

        Progress = SceneProgress.Scene3;
        SetBackground(2);
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
        TryStartDialogue(5);
    }

    public void TriggerScene7()
    {
        if (Progress != SceneProgress.Scene6)
            return;

        Progress = SceneProgress.Scene7;
        SetBackground(5);
        TryStartDialogue(6);
    }

    public void TriggerScene8()
    {
        if (Progress != SceneProgress.Scene7)
            return;

        Progress = SceneProgress.Scene8;
        SetBackground(6);
        TryStartDialogue(7);
    }

    public void TriggerScene9()
    {
        if (Progress != SceneProgress.Scene8)
            return;

        Progress = SceneProgress.Scene9;
        SetBackground(7);
        SetCharacter(15);
        TryStartDialogue(8);
    }
}
