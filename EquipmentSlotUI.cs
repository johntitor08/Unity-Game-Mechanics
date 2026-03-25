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
    public TextMeshProUGUI upgradeBadgeText;
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
    private EquipmentInstance currentInstance;
    private Button iconButton;
    public event Action<EquipmentInstance> OnItemClicked;
    public event Action<EquipmentInstance> OnDetailClicked;

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

    void OnEnable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += Refresh;
    }

    void OnDisable()
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

        currentInstance = EquipmentManager.Instance.GetEquipped(slotType);

        if (currentInstance != null)
            ShowEquipped(currentInstance);
        else
            ShowEmpty();
    }

    void ShowEquipped(EquipmentInstance instance)
    {
        var data = instance.baseData;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.color = Color.white;
            iconImage.enabled = true;
        }

        if (backgroundImage != null)
        {
            Sprite s = GetRaritySprite(data.rarity);

            if (s != null)
            {
                backgroundImage.sprite = s;
                backgroundImage.color = Color.white;
            }
            else
            {
                backgroundImage.sprite = null;
                Color c = data.GetRarityColor();
                c.a = 0.7f;
                backgroundImage.color = c;
            }
        }

        if (upgradeBadgeText != null)
        {
            upgradeBadgeText.gameObject.SetActive(instance.upgradeLevel > 0);
            upgradeBadgeText.text = $"+{instance.upgradeLevel}";
        }

        if (emptyIndicator != null)
            emptyIndicator.SetActive(false);

        if (slotButton != null)
            slotButton.interactable = true;

        if (iconButton != null)
            iconButton.interactable = true;
    }

    void ShowEmpty()
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

        if (upgradeBadgeText != null)
            upgradeBadgeText.gameObject.SetActive(false);

        if (emptyIndicator != null)
            emptyIndicator.SetActive(true);

        if (slotButton != null)
            slotButton.interactable = false;

        if (iconButton != null)
            iconButton.interactable = false;
    }

    Sprite GetRaritySprite(Rarity rarity) => rarity switch
    {
        Rarity.Common => commonSprite,
        Rarity.Rare => rareSprite,
        Rarity.Epic => epicSprite,
        Rarity.Legendary => legendarySprite,
        Rarity.Godly => godlySprite,
        _ => null
    };

    public void ShowEquipmentPanel()
    {
        if (currentInstance == null)
            return;

        if (EquipmentUI.Instance != null)
            EquipmentUI.Instance.OpenItemPanel(currentInstance);

        OnItemClicked?.Invoke(currentInstance);
    }

    public void ShowDetailPanel()
    {
        if (currentInstance == null)
            return;

        if (EquipmentUI.Instance != null)
            EquipmentUI.Instance.OpenDetailPanel(currentInstance);

        OnDetailClicked?.Invoke(currentInstance);
    }

    static string GetSlotDisplayName(EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => "Weapon",
        EquipmentSlot.Armor => "Armor",
        EquipmentSlot.Helmet => "Helmet",
        EquipmentSlot.Accessory => "Accessory",
        EquipmentSlot.Shield => "Shield",
        EquipmentSlot.Boots => "Boots",
        _ => "Unknown"
    };

    public EquipmentData GetEquippedItem() => currentInstance?.baseData;

    public EquipmentInstance GetEquippedInstance() => currentInstance;

    public EquipmentSlot GetSlotType() => slotType;
}
