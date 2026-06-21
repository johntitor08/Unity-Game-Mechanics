using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpgradeFusionButton : MonoBehaviour
{
    [Header("References")]
    public Button upgradeButton;
    public TextMeshProUGUI upgradeButtonText;

    private EquipmentData watchedItem;

    void Awake()
    {
        if (upgradeButton != null)
            upgradeButton.onClick.AddListener(OnUpgradeClicked);
    }

    void OnEnable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged += Refresh;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += Refresh;
    }

    void OnDisable()
    {
        if (EquipmentManager.Instance != null)
            EquipmentManager.Instance.OnEquipmentChanged -= Refresh;

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
    }

    public void SetItem(EquipmentData item)
    {
        watchedItem = item;
        Refresh();
    }

    void Refresh()
    {
        if (upgradeButton == null)
            return;

        if (watchedItem == null)
        {
            upgradeButton.gameObject.SetActive(false);
            return;
        }

        var fm = FusionManager.Instance;
        bool show = fm != null && fm.OwnsUpgradeable(watchedItem);
        upgradeButton.gameObject.SetActive(show);

        if (!show)
            return;

        bool canAfford = fm.CanUpgradeFuse(watchedItem);
        upgradeButton.interactable = canAfford;

        if (upgradeButtonText == null)
            return;

        int bestLevel = 0;
        var em = EquipmentManager.Instance;

        if (em != null)
        {
            var equipped = em.GetEquipped(watchedItem.slot);

            if (equipped != null && equipped.baseData.itemID == watchedItem.itemID)
                bestLevel = Mathf.Max(bestLevel, equipped.upgradeLevel);
        }

        if (InventoryManager.Instance != null)
        {
            foreach (var (inst, _) in InventoryManager.Instance.GetEquipmentInstances())
                if (inst.baseData.itemID == watchedItem.itemID)
                    bestLevel = Mathf.Max(bestLevel, inst.upgradeLevel);
        }

        var cost = fm.GetUpgradeCost(watchedItem, bestLevel);
        string matText = cost.material != null ? $" + {cost.materialQty} {cost.material.itemName}" : "";
        upgradeButtonText.text = $"Upgrade +{bestLevel}→+{bestLevel + 1}  ({cost.gold}g{matText})";
    }

    void OnUpgradeClicked()
    {
        if (watchedItem == null || FusionManager.Instance == null)
            return;

        var result = FusionManager.Instance.UpgradeFuse(watchedItem);

        if (result != null)
        {
            if (ItemDetailPanel.Instance != null)
                ItemDetailPanel.Instance.ShowItemDetail(result.baseData, -1, result.upgradeLevel);
        }

        Refresh();
    }
}
