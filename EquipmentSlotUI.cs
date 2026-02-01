using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EquipmentSlotUI : MonoBehaviour
{
    [Header("UI References")]
    public Image iconImage;
    public Image backgroundImage;
    public TextMeshProUGUI slotNameText;
    public GameObject emptyIndicator;
    public Button slotButton;

    [Header("Visual Feedback")]
    public Color emptyColor = new(0.3f, 0.3f, 0.3f, 0.5f);
    public Color filledColor = new(1f, 1f, 1f, 0.8f);
    public Sprite emptySlotIcon;

    private EquipmentSlot slotType;
    private EquipmentData currentEquipment;
    public event Action<EquipmentData> OnItemClicked;
    public event Action<EquipmentData> OnDetailClicked;

    private void OnEnable()
    {
        slotButton.onClick.AddListener(ShowDetailPanel);

        if (iconImage.TryGetComponent<Button>(out var iconButton))
            iconButton.onClick.AddListener(ShowEquipmentPanel);

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += Refresh;
    }

    private void OnDisable()
    {
        slotButton.onClick.RemoveListener(ShowDetailPanel);
        
        if (iconImage.TryGetComponent<Button>(out var iconButton))
            iconButton.onClick.RemoveListener(ShowEquipmentPanel);

        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= Refresh;
    }

    public void Setup(EquipmentSlot slot)
    {
        slotType = slot;

        if (slotNameText != null)
            slotNameText.text = GetSlotDisplayName(slot);

        Refresh();
    }

    public void Refresh()
    {
        if (EquipmentManager.Instance == null) return;
        currentEquipment = EquipmentManager.Instance.GetEquipped(slotType);

        if (currentEquipment != null)
        {
            ShowEquipped(currentEquipment);
        }
        else
        {
            ShowEmpty();
        }
    }

    private void ShowEquipped(EquipmentData equipment)
    {
        if (iconImage != null)
        {
            iconImage.sprite = equipment.icon;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }

        if (backgroundImage != null)
        {
            Color rarityColor = equipment.GetRarityColor();
            rarityColor.a = 0.7f;
            backgroundImage.color = rarityColor;
        }

        if (emptyIndicator != null)
            emptyIndicator.SetActive(false);
    }

    private void ShowEmpty()
    {
        if (iconImage != null)
        {
            if (emptySlotIcon != null)
            {
                iconImage.sprite = emptySlotIcon;
                iconImage.color = emptyColor;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.sprite = null;
                iconImage.enabled = false;
            }
        }

        if (backgroundImage != null)
        {
            backgroundImage.color = emptyColor;
        }

        if (emptyIndicator != null)
            emptyIndicator.SetActive(true);
    }

    public void ShowEquipmentPanel()
    {
        if (currentEquipment == null || EquipmentUI.Instance == null)
            return;

        EquipmentUI.Instance.OpenItemPanel(currentEquipment);
        OnItemClicked?.Invoke(currentEquipment);
    }

    public void ShowDetailPanel()
    {
        if (currentEquipment == null || EquipmentUI.Instance == null)
            return;

        EquipmentUI.Instance.OpenDetailPanel(currentEquipment);
        OnDetailClicked?.Invoke(currentEquipment);
    }

    private string GetSlotDisplayName(EquipmentSlot slot)
    {
        return slot switch
        {
            EquipmentSlot.Weapon => "Weapon",
            EquipmentSlot.Armor => "Armor",
            EquipmentSlot.Helmet => "Helmet",
            EquipmentSlot.Accessory => "Accessory",
            EquipmentSlot.Shield => "Shield",
            EquipmentSlot.Boots => "Boots",
            _ => "Unknown"
        };
    }

    public EquipmentData GetEquippedItem()
    {
        return currentEquipment;
    }

    public EquipmentSlot GetSlotType()
    {
        return slotType;
    }
}
