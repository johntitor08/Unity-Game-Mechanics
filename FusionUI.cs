using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class FusionUI : MonoBehaviour
{
    public static FusionUI Instance;
    private EquipmentData selectedA;
    private EquipmentData selectedB;

    [Header("Slots")]
    public Button slotA;
    public Button slotB;
    public Image iconA;
    public Image iconB;
    public TextMeshProUGUI nameA;
    public TextMeshProUGUI nameB;

    [Header("Result Preview")]
    public Image resultIcon;
    public TextMeshProUGUI resultName;
    public TextMeshProUGUI successChanceText;

    [Header("Controls")]
    public Button fuseButton;
    public TextMeshProUGUI statusText;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    void OnEnable()
    {
        fuseButton.onClick.RemoveAllListeners();
        fuseButton.onClick.AddListener(OnFuseClicked);
        ClearAll();
    }

    public void SelectItem(EquipmentData equipment)
    {
        if (equipment == null)
            return;

        if (selectedA == null)
            selectedA = equipment;
        else if (selectedB == null)
            selectedB = equipment;
        else
        {
            selectedA = selectedB;
            selectedB = equipment;
        }

        RefreshSlots();
        RefreshPreview();
    }

    void RefreshSlots()
    {
        SetSlot(iconA, nameA, selectedA);
        SetSlot(iconB, nameB, selectedB);
    }

    void SetSlot(Image icon, TextMeshProUGUI label, EquipmentData eq)
    {
        if (eq != null)
        {
            icon.sprite = eq.icon;
            icon.enabled = true;
            label.text = eq.itemName;
        }
        else
        {
            icon.enabled = false;
            label.text = "—";
        }
    }

    void RefreshPreview()
    {
        if (selectedA == null || selectedB == null)
        {
            resultIcon.enabled = false;
            resultName.text = "";
            successChanceText.text = "";
            fuseButton.interactable = false;
            return;
        }

        var recipe = FusionManager.Instance.FindRecipe(selectedA, selectedB);

        if (recipe != null)
        {
            resultIcon.sprite = recipe.result.icon;
            resultIcon.enabled = true;
            resultName.text = recipe.result.itemName;
            successChanceText.text = $"Success: {recipe.successChance * 100:0}%";
            fuseButton.interactable = FusionManager.Instance.CanFuse(selectedA, selectedB);
            statusText.text = "";
        }
        else
        {
            resultIcon.enabled = false;
            resultName.text = "No recipe found";
            successChanceText.text = "";
            fuseButton.interactable = false;
        }
    }

    void OnFuseClicked()
    {
        var result = FusionManager.Instance.Fuse(selectedA, selectedB);
        ClearAll();

        if (result != null)
            statusText.text = $"Created: {result.itemName}!";
        else
            statusText.text = "Fusion failed — items lost.";
    }

    void ClearAll()
    {
        selectedA = null;
        selectedB = null;
        RefreshSlots();
        RefreshPreview();
    }
}
