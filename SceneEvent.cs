using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum SceneProgress
{
    Scene1,
    Scene2,
    Scene3,
    Scene4
}

public class SceneEvent : MonoBehaviour
{
    private static readonly WaitForSeconds WAIT_HALF_SEC = new(0.5f);
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
            SaveSceneProgress();
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
    public GameObject libraryIcon;
    public GameObject dungeonIcon;
    public GameObject castleIcon;
    public GameObject arenaIcon;

    [Header("UI Panels")]
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject mapPanel;
    public GameObject combatMapPanel;
    public GameObject equipmentPanel;
    public GameObject coinPanel;

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
        DontDestroyOnLoad(gameObject);

        if (itemDatabase != null)
            itemDatabase.SetInstance();
    }

    void Start()
    {
        SetBackground(0);
        SetCharacter(0);

        if (SaveSystem.CachedData != null)
            ApplySceneProgress((SceneProgress)SaveSystem.CachedData.sceneProgress);

        SetupIconButtons();
        HideAllPanels();
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
            closeMapButton.GetComponent<Button>().onClick.AddListener(() => HideAllPanels());

        if (homeIcon != null)
            homeIcon.SetActive(false);

        if (marketIcon != null)
            marketIcon.SetActive(false);

        if (gymIcon != null)
            gymIcon.SetActive(false);

        if (officeIcon != null)
            officeIcon.SetActive(false);

        if (churchIcon != null)
            churchIcon.SetActive(false);

        if (libraryIcon != null)
            libraryIcon.SetActive(false);

        if (dungeonIcon != null)
            dungeonIcon.SetActive(false);

        if (castleIcon != null)
            castleIcon.SetActive(false);

        if (arenaIcon != null)
            arenaIcon.SetActive(false);
    }

    void TogglePanel(GameObject panel, string panelName)
    {
        if (panel == null)
        {
            Debug.LogWarning($"{panelName} panel not found!");
            return;
        }

        bool isActive = panel.activeSelf;

        if (closeOtherPanelsOnOpen && !allowMultiplePanels && !isActive)
            HideAllPanels();

        panel.SetActive(!isActive);

        if (!isActive && panelName == "Map")
            SetMap(currentMapIndex);
    }

    public void OpenProfile()
    {
        OpenPanel(profilePanel, "Profile");

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }
    }

    public void OpenInventory()
    {
        OpenPanel(inventoryPanel, "Inventory");

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }
    }

    public void OpenShop()
    {
        OpenPanel(shopPanel, "Shop");

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }
    }

    public void OpenMap()
    {
        OpenPanel(mapPanel, "Map");

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }
    }

    public void OpenCombat()
    {
        OpenPanel(combatMapPanel, "Combat");

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }
    }

    public void OpenEquipment()
    {
        OpenPanel(equipmentPanel, "Equipment");

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }
    }

    public void OpenCoin()
    {
        OpenPanel(coinPanel, "Coin");

        if (ProfileUI.Instance != null)
        {
            ProfileUI.Instance.RefreshAll();
        }
    }

    void OpenPanel(GameObject panel, string panelName)
    {
        if (panel == null)
        {
            Debug.LogWarning($"{panelName} panel not found!");
            return;
        }

        if (closeOtherPanelsOnOpen && !allowMultiplePanels)
        {
            HideAllPanels();
        }

        panel.SetActive(true);
    }

    public void TriggerScene2()
    {
        if (Progress != SceneProgress.Scene1)
            return;

        Progress = SceneProgress.Scene2;
        StartCoroutine(Scene2Routine());
    }

    IEnumerator Scene2Routine()
    {
        yield return WAIT_HALF_SEC;
        SetBackground(1);

        if (sceneStartDialogueNodes[0] != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(sceneStartDialogueNodes[0]);
        }
    }

    public void TriggerScene3()
    {
        if (Progress != SceneProgress.Scene2)
            return;

        Progress = SceneProgress.Scene3;
        SetBackground(2);

        if (sceneStartDialogueNodes[1] != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(sceneStartDialogueNodes[1]);
        }
    }

    public void TriggerScene4()
    {
        if (Progress != SceneProgress.Scene3)
            return;

        Progress = SceneProgress.Scene4;
        SetBackground(3);

        if (sceneStartDialogueNodes[2] != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(sceneStartDialogueNodes[2]);
        }
    }

    public void SaveSceneProgress()
    {
        if (SaveSystem.CachedData != null)
        {
            SaveSystem.CachedData.sceneProgress = (int)Progress;
        }
    }

    public void ResetSceneProgress()
    {
        Progress = SceneProgress.Scene1;
        SetBackground(0);
        SetCharacter(0);
    }

    public void RestartSceneProgress()
    {
        Progress = SceneProgress.Scene1;
        SetBackground(0);

        if (sceneStartDialogueNodes[0] != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(sceneStartDialogueNodes[0]);
        }
    }

    public void TriggerCombat()
    {
        if (CombatManager.Instance != null && Progress == SceneProgress.Scene4)
            CombatManager.Instance.StartCombat(enemies[0]);
    }

    void OnEnable()
    {
        SubscribeDialogue();
    }

    void OnDisable()
    {
        UnsubscribeDialogue();
    }

    void SubscribeDialogue()
    {
        if (subscribed)
            return;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
            subscribed = true;
        }
    }

    void UnsubscribeDialogue()
    {
        if (!subscribed)
            return;

        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
            subscribed = false;
        }
    }

    void HandleDialogueEnd(DialogueNode endedNode)
    {
        if (endedNode == null)
            return;

        if (endedNode.name.Contains("S2") && endedNode.isFinalNode)
        {
            TriggerScene3();
        }
        else if (endedNode.name.Contains("S3") && endedNode.isFinalNode)
        {
            TriggerScene4();
        }
        else if (endedNode.name.Contains("S4") && endedNode.isFinalNode)
        {
            TriggerCombat();
        }
    }

    public void ApplySceneProgress(SceneProgress progress)
    {
        Progress = progress;

        switch (Progress)
        {
            case SceneProgress.Scene1:
                SetBackground(0);
                break;

            case SceneProgress.Scene2:
                SetBackground(1);
                break;

            case SceneProgress.Scene3:
                SetBackground(2);
                break;

            case SceneProgress.Scene4:
                SetBackground(3);
                break;
        }
    }

    public void SetCharacter(int index)
    {
        if (charImage != null && index < chars.Length && chars[index] != null)
            charImage.sprite = chars[index];
    }

    public void SetBackground(int index)
    {
        if (backgroundImage != null && index < bgs.Length && bgs[index] != null)
            backgroundImage.sprite = bgs[index];

        if (hoverEffect != null)
            hoverEffect.SetActive(index == 0);
    }

    public void HideAllPanels()
    {
        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (combatMapPanel != null)
            combatMapPanel.SetActive(false);

        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

        if (coinPanel != null)
            coinPanel.SetActive(false);
    }

    public void SetMap(int index)
    {
        if (mapImage == null || maps[index] == null)
            return;

        mapImage.sprite = maps[index];
        bool isModern = index == 0;
        bool isFantasy = index == 1;
        bool isCombat = index == 2;

        if (homeIcon != null)
            homeIcon.SetActive(isModern);

        if (marketIcon != null)
            marketIcon.SetActive(isModern);

        if (gymIcon != null)
            gymIcon.SetActive(isModern);

        if (officeIcon != null)
            officeIcon.SetActive(isModern);

        if (churchIcon != null)
            churchIcon.SetActive(isModern);

        if (libraryIcon != null)
            libraryIcon.SetActive(isFantasy);

        if (dungeonIcon != null)
            dungeonIcon.SetActive(isFantasy);

        if (castleIcon != null)
            castleIcon.SetActive(isFantasy);

        if (arenaIcon != null)
            arenaIcon.SetActive(isFantasy);

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
}
