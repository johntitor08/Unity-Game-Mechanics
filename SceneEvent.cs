using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public ItemDatabase itemDatabase;
    private bool isDialogueSubscribed;
    private bool isHoverEffectsSubscribed;
    public GameObject[] hoverEffects;
    private readonly List<System.Action> _hoverHideActions = new();

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

    [Header("UI Panels")]
    public GameObject timePanel;
    public GameObject iconPanel;
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject mapPanel;
    public GameObject combatMapPanel;
    public GameObject equipmentPanel;
    public GameObject coinPanel;
    public GameObject houseIconsPanel;

    [Header("UI Maps")]
    public Image mapImage;
    public Sprite[] maps;

    [Header("UI Icons")]
    public GameObject saveIcon;
    public GameObject loadIcon;
    public GameObject resetIcon;
    public GameObject profileIcon;
    public GameObject inventoryIcon;
    public GameObject mapIcon;
    public GameObject coinButton;
    public GameObject combatIcon;
    public GameObject closeMapButton;
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
    }

    void Start()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.PanelAnimator = this;

        if (!SaveSystem.IsLoading)
        {
            SetBackground(0);
            SetCharacter(0);
        }

        SetupIconButtons();
        HideAllPanels();
        SubscribeDialogue();
        SubscribeHoverEffects();

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
    }

    void OnDisable()
    {
        UnsubscribeDialogue();
        UnsubscribeHoverEffects();
    }

    void SetupIconButtons()
    {
        if (profileIcon != null)
            profileIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(profilePanel, "Profile"));

        if (inventoryIcon != null)
            inventoryIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(inventoryPanel, "Inventory"));

        if (mapIcon != null)
            mapIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(mapPanel, "Map"));

        if (coinButton != null)
            coinButton.GetComponent<Button>().onClick.AddListener(() => TogglePanel(coinPanel, "Coin"));

        if (closeMapButton != null)
            closeMapButton.GetComponent<Button>().onClick.AddListener(HideAllPanels);

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

    public void OpenCombat()
    {
        OpenPanel(combatMapPanel, "Combat");

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

    public void HideAllPanels()
    {
        SetActive(profilePanel, false);
        SetActive(inventoryPanel, false);

        if (MarketUI.Instance != null)
            MarketUI.Instance.CloseAll();
        else
            SetActive(shopPanel, false);

        SetActive(mapPanel, false);
        SetActive(combatMapPanel, false);
        SetActive(equipmentPanel, false);
        SetActive(coinPanel, false);
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

        if (!isActive && panelName == "Map")
            SetMap(currentMapIndex);
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
        }

        bool isHouse = index == 11 || index == 13 || index == 14 || index == 15 || index == 16 || index == 17 || index == 20;
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
            else if(index == 11)
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
        if (mapImage == null || index < 0 || index >= maps.Length || maps[index] == null)
            return;

        mapImage.sprite = maps[index];
        bool isModern = index == 0;
        bool isFantasy = index == 1;
        bool isCombat = index == 2;
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
    }

    public void NextMap()
    {
        currentMapIndex = (currentMapIndex + 1) % maps.Length;
        SetMap(currentMapIndex);
    }

    public void PreviousMap()
    {
        currentMapIndex = (currentMapIndex - 1 + maps.Length) % maps.Length;
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

        if (sceneStartDialogueNodes == null || nodeIndex >= sceneStartDialogueNodes.Length || Progress == SceneProgress.SceneHome || Progress == SceneProgress.SceneMarket || Progress == SceneProgress.SceneGym || Progress == SceneProgress.SceneOffice || Progress == SceneProgress.SceneChurch  )
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

        if (timePanelAnimator != null)
            timePanelAnimator.SetTrigger(timePanelOpenTrigger);

        if (iconPanelAnimator != null)
            iconPanelAnimator.SetTrigger(iconPanelOpenTrigger);

        if (charImage != null)
            StartCoroutine(FadeOutCharacter(0.5f, endedNode));
        else
            HandleSceneTransition(endedNode);

        if (Progress == SceneProgress.SceneMarket && sceneStartDialogueNodes != null && sceneStartDialogueNodes.Length > 8 && sceneStartDialogueNodes[8] != null && endedNode == sceneStartDialogueNodes[8] && MarketUI.Instance != null)
        {
            MarketUI.Instance.OpenMarket();
        }
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

    public void ResetSceneProgress()
    {
        Progress = SceneProgress.Scene1;
        SetBackground(0);
        SetCharacter(0);
        TryStartDialogue(0);
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

        isHoverEffectsSubscribed = false;
    }

    static void SetActive(GameObject go, bool active)
    {
        if (go != null)
            go.SetActive(active);
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

    public void StartCashierDialogue()
    {
        TriggerMarketScene();
    }

    public void StartNunDialogue()
    {
        TriggerChurchScene();
    }

    public void StartCoachDialogue()
    {
        TriggerGymScene();
    }

    public void StartStepSisterDialogue()
    {
        TriggerHomeScene();
    }

    public void StartOfficerDialogue()
    {
        TriggerOfficeScene();
    }

    public void TriggerHomeScene()
    {
        Progress = SceneProgress.SceneHome;
        TryStartDialogue(11);
    }

    public void TriggerMarketScene()
    {
        Progress = SceneProgress.SceneMarket;
        TryStartDialogue(8);
    }

    public void TriggerGymScene()
    {
        Progress = SceneProgress.SceneGym;
        TryStartDialogue(10);
    }

    public void TriggerOfficeScene()
    {
        Progress = SceneProgress.SceneOffice;
        TryStartDialogue(12);
    }

    public void TriggerChurchScene()
    {
        Progress = SceneProgress.SceneChurch;
        TryStartDialogue(9);
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

    private void StartCombatForScene4()
    {
        if (CombatManager.Instance == null)
            return;

        SetFleeDisabled(true);
        CombatManager.Instance.OnCombatVictory -= HandleScene4CombatVictory;
        CombatManager.Instance.OnCombatDefeat -= HandleScene4CombatDefeat;
        CombatManager.Instance.OnCombatVictory += HandleScene4CombatVictory;
        CombatManager.Instance.OnCombatDefeat += HandleScene4CombatDefeat;
        CombatManager.Instance.StartCombat(enemies[0]);
    }

    private void HandleScene4CombatVictory()
    {
        SetFleeDisabled(false);
        UnsubscribeSceneCombat();
        TriggerScene5();
    }

    private void HandleScene4CombatDefeat()
    {
        SetFleeDisabled(false);
        UnsubscribeSceneCombat();
        SetCharacter(8);
    }

    private void UnsubscribeSceneCombat()
    {
        if (CombatManager.Instance == null)
            return;

        CombatManager.Instance.OnCombatVictory -= HandleScene4CombatVictory;
        CombatManager.Instance.OnCombatDefeat -= HandleScene4CombatDefeat;
    }

    public void TriggerScene5()
    {
        if (Progress != SceneProgress.Scene4)
            return;

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
        SetCharacter(15);
        TryStartDialogue(7);
    }
}
