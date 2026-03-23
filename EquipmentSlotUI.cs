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

    private Sprite emptySprite;
    private Sprite commonSprite;
    private Sprite rareSprite;
    private Sprite epicSprite;
    private Sprite legendarySprite;
    private Sprite godlySprite;
    private EquipmentSlot slotType;
    private EquipmentData currentEquipment;
    private Button iconButton;
    public event Action<EquipmentData> OnItemClicked;
    public event Action<EquipmentData> OnDetailClicked;

    void Awake()
    {
        if (slotButton != null)
            slotButton.onClick.AddListener(ShowDetailPanel);

        if (iconImage != null && iconImage.TryGetComponent<Button>(out var ib))
        {
            iconButton = ib;
            iconButton.onClick.AddListener(ShowEquipmentPanel);
        }
    }

    private void OnEnable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += Refresh;
    }

    private void OnDisable()
    {
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

    public void SetRaritySprites(Sprite empty, Sprite common, Sprite rare, Sprite epic, Sprite legendary, Sprite godly)
    {
        emptySprite = empty;
        commonSprite = common;
        rareSprite = rare;
        epicSprite = epic;
        legendarySprite = legendary;
        godlySprite = godly;
    }

    public void Refresh()
    {
        if (EquipmentManager.Instance == null)
            return;

        currentEquipment = EquipmentManager.Instance.GetEquipped(slotType);

        if (currentEquipment != null)
            ShowEquipped(currentEquipment);
        else
            ShowEmpty();
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
            Sprite s = GetRaritySprite(equipment.equipmentRarity);

            if (s != null)
            {
                backgroundImage.sprite = s;
                backgroundImage.color = Color.white;
            }
            else
            {
                backgroundImage.sprite = null;
                Color c = equipment.GetRarityColor();
                c.a = 0.7f;
                backgroundImage.color = c;
            }
        }

        if (emptyIndicator != null)
            emptyIndicator.SetActive(false);

        if (slotButton != null)
            slotButton.interactable = true;

        if (iconButton != null)
            iconButton.interactable = true;
    }

    private void ShowEmpty()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.color = emptyColor;
            iconImage.enabled = false;
        }

        if (backgroundImage != null)
        {
            if (emptySprite != null)
            {
                backgroundImage.sprite = emptySprite;
                backgroundImage.color = Color.white;
            }
            else
            {
                backgroundImage.sprite = null;
                backgroundImage.color = emptyColor;
            }
        }

        if (emptyIndicator != null)
            emptyIndicator.SetActive(true);

        if (slotButton != null)
            slotButton.interactable = false;

        if (iconButton != null)
            iconButton.interactable = false;
    }

    private Sprite GetRaritySprite(EquipmentRarity rarity) => rarity switch
    {
        EquipmentRarity.Common => commonSprite,
        EquipmentRarity.Rare => rareSprite,
        EquipmentRarity.Epic => epicSprite,
        EquipmentRarity.Legendary => legendarySprite,
        EquipmentRarity.Godly => godlySprite,
        _ => null
    };

    public void ShowEquipmentPanel()
    {
        if (currentEquipment == null)
            return;

        if (EquipmentUI.Instance != null)
            EquipmentUI.Instance.OpenItemPanel(currentEquipment);

        OnItemClicked?.Invoke(currentEquipment);
    }

    public void ShowDetailPanel()
    {
        if (currentEquipment == null)
            return;

        if (EquipmentUI.Instance != null)
            EquipmentUI.Instance.OpenDetailPanel(currentEquipment);

        OnDetailClicked?.Invoke(currentEquipment);
    }

    private static string GetSlotDisplayName(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => "Weapon",
        EquipmentSlot.Armor => "Armor",
        EquipmentSlot.Helmet => "Helmet",
        EquipmentSlot.Accessory => "Accessory",
        EquipmentSlot.Shield => "Shield",
        EquipmentSlot.Boots => "Boots",
        _ => "Unknown"
    };

    public EquipmentData GetEquippedItem() => currentEquipment;

    public EquipmentSlot GetSlotType() => slotType;
}
