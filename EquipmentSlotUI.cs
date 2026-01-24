using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class EquipmentSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Elements")]
    public Image iconImage;
    public Button slotButton;
    public GameObject emptyIndicator;
    public TextMeshProUGUI slotNameText;
    public Image rarityBorder;
    public EquipmentSlot slot;
    private EquipmentData currentItem;

    public void Setup(EquipmentSlot equipmentSlot)
    {
        slot = equipmentSlot;

        if (slotNameText != null)
            slotNameText.text = slot.ToString();

        if (slotButton != null)
        {
            slotButton.onClick.RemoveAllListeners();
            slotButton.onClick.AddListener(OnSlotClicked);
        }

        Refresh();
    }

    public void Refresh()
    {
        currentItem = EquipmentManager.Instance.GetEquipped(slot);

        if (currentItem != null)
        {
            // Show equipped item
            if (iconImage != null)
            {
                iconImage.sprite = currentItem.icon;
                iconImage.enabled = true;
            }

            if (emptyIndicator != null)
                emptyIndicator.SetActive(false);

            if (rarityBorder != null)
            {
                rarityBorder.color = currentItem.GetRarityColor();
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
        if (currentItem != null)
        {
            if (EquipmentDetailPanel.Instance != null)
                EquipmentDetailPanel.Instance.Show(currentItem, slot);
        }
        else
        {
            Debug.Log($"No {slot} equipped. Open inventory to equip.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && EquipmentTooltip.Instance != null)
        {
            EquipmentTooltip.Instance.Show(currentItem);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EquipmentTooltip.Instance?.Hide();
    }
}
