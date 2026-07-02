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
            label.text = eq.DisplayName;
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
            resultName.text = recipe.result.DisplayName;
            successChanceText.text = $"{Loc.T("Success", "Başarı")}: {recipe.successChance * 100:0}%";
            fuseButton.interactable = FusionManager.Instance.CanFuse(selectedA, selectedB);
            statusText.text = "";
        }
        else
        {
            resultIcon.enabled = false;
            resultName.text = Loc.T("No recipe found", "Tarif bulunamadı");
            successChanceText.text = "";
            fuseButton.interactable = false;
        }
    }

    void OnFuseClicked()
    {
        if (selectedA == null || selectedB == null || FusionManager.Instance == null)
        {
            statusText.text = Loc.T("Select two items to fuse.", "Birleştirmek için iki eşya seç.");
            return;
        }

        var result = FusionManager.Instance.Fuse(selectedA, selectedB);
        ClearAll();

        if (result != null)
            statusText.text = $"{Loc.T("Created", "Oluşturuldu")}: {result.DisplayName}!";
        else
            statusText.text = Loc.T("Fusion failed — items were lost.", "Birleştirme başarısız — eşyalar kayboldu.");
    }

    void ClearAll()
    {
        selectedA = null;
        selectedB = null;
        RefreshSlots();
        RefreshPreview();
    }
}
