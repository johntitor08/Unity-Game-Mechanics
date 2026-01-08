using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public Button slotButton;
    public GameObject emptyIndicator;
    public TextMeshProUGUI slotNameText;
    public Image rarityBorder;

    private EquipmentSlot slot;

    public void Setup(EquipmentSlot equipmentSlot)
    {
        slot = equipmentSlot;

        if (slotNameText != null)
            slotNameText.text = slot.ToString();

        if (slotButton != null)
            slotButton.onClick.AddListener(OnSlotClicked);

        Refresh();
    }

    public void Refresh()
    {
        var equipped = EquipmentManager.Instance.GetEquipped(slot);

        if (equipped != null)
        {
            // Show equipped item
            if (iconImage != null)
            {
                iconImage.sprite = equipped.icon;
                iconImage.enabled = true;
            }

            if (emptyIndicator != null)
                emptyIndicator.SetActive(false);

            if (rarityBorder != null)
            {
                rarityBorder.color = equipped.GetRarityColor();
                rarityBorder.enabled = true;
            }
        }
        else
        {
            // Show empty slot
            if (iconImage != null)
                iconImage.enabled = false;

            if (emptyIndicator != null)
                emptyIndicator.SetActive(true);

            if (rarityBorder != null)
                rarityBorder.enabled = false;
        }
    }

    void OnSlotClicked()
    {
        var equipped = EquipmentManager.Instance.GetEquipped(slot);

        if (equipped != null)
        {
            EquipmentDetailPanel.Instance.Show(equipped, slot);
        }
        else
        {
            Debug.Log($"No {slot} equipped. Open inventory to equip.");
            // Could open inventory filtered to this slot type
        }
    }
}
