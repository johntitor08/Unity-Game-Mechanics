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
    public SceneProgress Progress { get; private set; } = SceneProgress.Scene1;
    public ItemDatabase itemDatabase;
    bool subscribed;

    [Header("Dialogues")]
    public DialogueNode[] sceneStartDialogueNodes;

    [Header("Backgrounds")]
    public GameObject[] bgs;

    [Header("Characters")]
    public GameObject[] chars;

    [Header("UI Icons")]
    public Button profileIcon;
    public Button inventoryIcon;
    public Button shopIcon;
    public Button mapIcon;
    public Button combatIcon;
    public Button equipmentIcon;
    public Button coinButton;
    public Button closeMapButton;

    [Header("UI Panels")]
    public GameObject profilePanel;
    public GameObject inventoryPanel;
    public GameObject shopPanel;
    public GameObject mapPanel;
    public GameObject combatMapPanel;
    public GameObject equipmentPanel;
    public GameObject coinPanel;

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

        if (SaveSystem.CachedData != null)
        {
            ApplySceneProgress(
                (SceneProgress)SaveSystem.CachedData.sceneProgress
            );
        }

        if (chars.Length > 0 && chars[0] != null)
            chars[0].SetActive(true);
        
        SetupIconButtons();
        HideAllPanels();

        if (SaveSystem.CachedData != null)
            SaveSystem.ApplyLoadedData(SaveSystem.CachedData);
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

        if (combatIcon != null)
            combatIcon.onClick.AddListener(() => TogglePanel(combatMapPanel, "Combat"));

        if (equipmentIcon != null)
            equipmentIcon.onClick.AddListener(() => TogglePanel(equipmentPanel, "Equipment"));

        if (coinButton != null)
            coinButton.onClick.AddListener(() => TogglePanel(coinPanel, "Coin"));

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

        if (closeOtherPanelsOnOpen && !allowMultiplePanels && !isActive)
        {
            HideAllPanels();
        }

        panel.SetActive(!isActive);
    }

    public void CloseAllPanels()
    {
        HideAllPanels();
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
        }
    }

    void SetBackground(int index)
    {
        for (int i = 0; i < bgs.Length; i++)
        {
            if (bgs[i] != null)
                bgs[i].SetActive(i == index);
        }
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

        if (combatMapPanel != null)
            combatMapPanel.SetActive(false);

        if (equipmentPanel != null)
            equipmentPanel.SetActive(false);

        if (coinButton != null)
            coinPanel.SetActive(false);
    }
}
