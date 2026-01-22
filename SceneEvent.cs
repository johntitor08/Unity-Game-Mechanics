using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SceneEvent : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds0_5 = new(0.5f);
    bool hasTriggered = false;

    public static SceneEvent Instance { get; private set; }

    [Header("Dialogues")]
    public DialogueNode scene2AStartDialogueNode;

    [Header("Backgrounds")]
    public GameObject bg1;
    public GameObject bg2;

    [Header("Characters")]
    public GameObject char1;

    [Header("Icon Buttons")]
    public Button profileIcon;
    public Button inventoryIcon;
    public Button shopIcon;
    public Button mapIcon;
    public Button equipmentIcon;
    public Button statsIcon;

    [Header("Close Buttons")]
    public Button closeMapButton;

    [Header("UI Panels (Auto-detect)")]
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject mapPanel;
    public GameObject equipmentPanel;
    public GameObject statsPanel;

    [Header("Icon Settings")]
    public bool closeOtherPanelsOnOpen = true;
    public bool allowMultiplePanels = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // Setup backgrounds
        if (bg1 != null)
            bg1.SetActive(true);

        if (bg2 != null)
            bg2.SetActive(false);

        // Setup character
        if (char1 != null)
            char1.SetActive(true);

        // Setup icon buttons
        SetupIconButtons();

        // Initially hide all panels
        HideAllPanels();
    }

    void SetupIconButtons()
    {
        if (profileIcon != null)
            profileIcon.onClick.AddListener(() => TogglePanel(profilePanel, "Profile"));

        if (inventoryIcon != null)
            inventoryIcon.onClick.AddListener(() => TogglePanel(inventoryPanel, "Inventory"));

        if (shopIcon != null)
            shopIcon.onClick.AddListener(() => TogglePanel(shopPanel, "Shop"));

        if (mapIcon != null)
            mapIcon.onClick.AddListener(() => TogglePanel(mapPanel, "Map"));

        if (equipmentIcon != null)
            equipmentIcon.onClick.AddListener(() => TogglePanel(equipmentPanel, "Equipment"));

        if (statsIcon != null)
            statsIcon.onClick.AddListener(() => TogglePanel(statsPanel, "Stats"));

        if (closeMapButton != null)
            closeMapButton.onClick.AddListener(() => HideAllPanels());
    }

    void TogglePanel(GameObject panel, string panelName)
    {
        if (panel == null)
        {
            Debug.LogWarning($"{panelName} panel not found!");
            return;
        }

        bool isActive = panel.activeSelf;

        // Close other panels if setting is enabled
        if (closeOtherPanelsOnOpen && !allowMultiplePanels && !isActive)
        {
            HideAllPanels();
        }

        // Toggle the panel
        panel.SetActive(!isActive);

        Debug.Log($"{panelName} panel: {(panel.activeSelf ? "Opened" : "Closed")}");
    }

    void HideAllPanels()
    {
        if (profilePanel != null)
            profilePanel.SetActive(false);

        if (inventoryPanel != null)
            inventoryPanel.SetActive(false);

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (mapPanel != null)
            mapPanel.SetActive(false);

        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

        if (statsPanel != null)
            statsPanel.SetActive(false);
    }

    public void CloseAllPanels()
    {
        HideAllPanels();
    }

    public void Trigger()
    {
        HandleDialogueEnd();
    }

    IEnumerator TriggerDelayed()
    {
        yield return _waitForSeconds0_5;

        if (bg1 != null)
            bg1.SetActive(false);

        if (bg2 != null)
            bg2.SetActive(true);

        if (char1 != null)
            char1.SetActive(false);

        DialogueNode nextNode = scene2AStartDialogueNode;

        if (nextNode != null && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.StartDialogue(nextNode);
        }
    }

    // Public methods to programmatically control panels
    public void OpenProfile() => OpenPanel(profilePanel, "Profile");
    public void OpenInventory() => OpenPanel(inventoryPanel, "Inventory");
    public void OpenShop() => OpenPanel(shopPanel, "Shop");
    public void OpenMap() => OpenPanel(mapPanel, "Map");
    public void OpenEquipment() => OpenPanel(equipmentPanel, "Equipment");
    public void OpenStats() => OpenPanel(statsPanel, "Stats");

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

    private void OnEnable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd += HandleDialogueEnd;
        }
    }

    private void OnDisable()
    {
        if (DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueEnd -= HandleDialogueEnd;
        }
    }

    void HandleDialogueEnd()
    {
        if (hasTriggered) return;
        hasTriggered = true;

        StartCoroutine(TriggerDelayed());
    }

}
