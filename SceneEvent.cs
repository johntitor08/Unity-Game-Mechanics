using System.Collections;
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
}

public class SceneEvent : MonoBehaviour
{
    public static SceneEvent Instance { get; private set; }
    private SceneProgress progress = SceneProgress.Scene1;
    private int currentMapIndex = 0;
    public ItemDatabase itemDatabase;
    public bool subscribed;
    public GameObject hoverEffect;

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

    [Header("Dialogues")]
    public DialogueNode[] sceneStartDialogueNodes;

    [Header("Backgrounds")]
    public Image backgroundImage;
    public Sprite[] bgs;

    [Header("Characters")]
    public Image charImage;
    public Sprite[] chars;

    [Header("Enemies")]
    public EnemyData[] enemies;

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

    [Header("UI Panels")]
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
        if (!SaveSystem.IsLoading)
        {
            SetBackground(0);
            SetCharacter(0);
        }

        SetupIconButtons();
        HideAllPanels();
        SubscribeDialogue();

        if (hoverEffect != null)
            hoverEffect.GetComponent<UIHoverRegion>().OnRegionClicked += TriggerScene2;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
            TogglePanel(mapPanel, "Map");
    }

    void OnEnable()
    {
        SubscribeDialogue();
    }

    void OnDisable()
    {
        UnsubscribeDialogue();
    }

    void SetupIconButtons()
    {
        if (profileIcon != null)
            profileIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(profilePanel, "Profile"));

        if (inventoryIcon != null)
            inventoryIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(inventoryPanel, "Inventory"));

        if (mapIcon != null)
            mapIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(mapPanel, "Map"));

        if (combatIcon != null)
            combatIcon.GetComponent<Button>().onClick.AddListener(() => TogglePanel(combatMapPanel, "Combat"));

        if (coinButton != null)
            coinButton.GetComponent<Button>().onClick.AddListener(() => TogglePanel(coinPanel, "Coin"));

        if (closeMapButton != null)
            closeMapButton.GetComponent<Button>().onClick.AddListener(HideAllPanels);

        if (marketIcon != null)
        {
            marketIcon.GetComponent<Button>().onClick.AddListener(() =>
            {
                if (MarketUI.Instance != null)
                    MarketUI.Instance.OpenMarket();
            });

            marketIcon.transform.parent.parent.gameObject.SetActive(false);
        }

        SetActive(homeIcon, false);
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

    void TogglePanel(GameObject panel, string panelName)
    {
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

    void OpenPanel(GameObject panel, string panelName)
    {
        if (panel == null)
        {
            Debug.LogWarning($"[SceneEvent] {panelName} panel reference is null.");
            return;
        }

        if (closeOtherPanelsOnOpen && !allowMultiplePanels)
            HideAllPanels();

        panel.SetActive(true);
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

    public void SetCharacter(int index)
    {
        if (charImage != null && index >= 0 && index < chars.Length && chars[index] != null)
            charImage.sprite = chars[index];
    }

    public void SetBackground(int index)
    {
        if (backgroundImage != null && index >= 0 && index < bgs.Length && bgs[index] != null)
            backgroundImage.sprite = bgs[index];

        SetActive(hoverEffect, index == 0);
        bool isHouse = index == 11 || index == 13 || index == 14 || index == 15;
        SetActive(houseIconsPanel, isHouse);
        SetActive(livingRoomIcon, isHouse);
        SetActive(bedroomIcon, isHouse);
        SetActive(tvRoomIcon, isHouse);
        SetActive(kitchenIcon, isHouse);
        SetActive(bathroomIcon, isHouse);
        SetActive(gardenIcon, isHouse);
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
        SetActive(gymIcon, isModern);
        SetActive(officeIcon, isModern);
        SetActive(churchIcon, isModern);

        if (marketIcon != null)
            marketIcon.transform.parent.parent.gameObject.SetActive(isModern);

        SetActive(townIcon, isFantasy);
        SetActive(libraryIcon, isFantasy);
        SetActive(dungeonIcon, isFantasy);
        SetActive(castleIcon, isFantasy);
        SetActive(arenaIcon, isFantasy);

        if (combatIcon != null)
            combatIcon.transform.parent.parent.gameObject.SetActive(isCombat);
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
        if (subscribed || DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        DialogueManager.Instance.OnLineShown += HandleLineShown;
        subscribed = true;
    }

    public void UnsubscribeDialogue()
    {
        if (!subscribed || DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        DialogueManager.Instance.OnLineShown -= HandleLineShown;
        subscribed = false;
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

        if (sceneStartDialogueNodes == null || nodeIndex >= sceneStartDialogueNodes.Length)
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

    void HandleDialogueEnd(DialogueNode endedNode)
    {
        if (endedNode == null || !endedNode.isFinalNode)
            return;

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
                TriggerScene6();
                break;
        }
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
        }
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

        CombatManager.Instance.OnCombatVictory += HandleScene4CombatVictory;
        CombatManager.Instance.OnCombatDefeat += HandleScene4CombatDefeat;
        CombatManager.Instance.StartCombat(enemies[0]);
    }

    private void HandleScene4CombatVictory()
    {
        UnsubscribeSceneCombat();
        TriggerScene5();
    }

    private void HandleScene4CombatDefeat()
    {
        UnsubscribeSceneCombat();
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
}
