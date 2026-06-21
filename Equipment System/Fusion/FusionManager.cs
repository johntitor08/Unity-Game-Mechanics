using System.Collections.Generic;
using UnityEngine;

public class FusionManager : MonoBehaviour
{
    public static FusionManager Instance;

    [Header("Recipes")]
    public List<FusionRecipe> recipes = new();

    [Header("Upgrade Cost")]
    public ItemData upgradeMaterialCommon;
    public ItemData upgradeMaterialRare;
    public ItemData upgradeMaterialEpic;
    public int baseGoldPerLevel = 50;

    void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public FusionRecipe FindRecipe(EquipmentData a, EquipmentData b)
    {
        foreach (var r in recipes)
        {
            bool match = (r.ingredientA.itemID == a.itemID && r.ingredientB.itemID == b.itemID) || (r.ingredientA.itemID == b.itemID && r.ingredientB.itemID == a.itemID);

            if (match)
                return r;
        }

        return null;
    }

    public bool CanFuse(EquipmentData a, EquipmentData b)
    {
        if (a == null || b == null)
            return false;

        var em = EquipmentManager.Instance;

        if (a.itemID == b.itemID)
            return false;

        bool aEquipped = em != null && em.IsEquipped(a);
        bool bEquipped = em != null && em.IsEquipped(b);

        if (aEquipped || bEquipped)
            return false;

        return InventoryManager.Instance.GetQuantity(a) > 0 && InventoryManager.Instance.GetQuantity(b) > 0 && FindRecipe(a, b) != null;
    }

    public EquipmentData Fuse(EquipmentData a, EquipmentData b)
    {
        if (!CanFuse(a, b))
            return null;

        var recipe = FindRecipe(a, b);
        InventoryManager.Instance.RemoveItem(a, 1);
        InventoryManager.Instance.RemoveItem(b, 1);

        if (Random.value > recipe.successChance)
            return null;

        InventoryManager.Instance.AddItem(recipe.result, 1);
        return recipe.result;
    }

    public struct UpgradeCost
    {
        public int gold;
        public ItemData material;
        public int materialQty;
    }

    public UpgradeCost GetUpgradeCost(EquipmentData data, int currentLevel)
    {
        ItemData mat = data.rarity switch
        {
            Rarity.Common => upgradeMaterialCommon,
            Rarity.Rare => upgradeMaterialRare,
            _ => upgradeMaterialEpic
        };

        int rarityFactor = data.rarity switch
        {
            Rarity.Common => 1,
            Rarity.Rare => 2,
            Rarity.Epic => 3,
            Rarity.Legendary => 5,
            _ => 6
        };

        return new UpgradeCost
        {
            gold = baseGoldPerLevel * (currentLevel + 1) * rarityFactor,
            material = mat,
            materialQty = currentLevel + 1
        };
    }

    public bool CanUpgradeFuse(EquipmentData data)
    {
        if (data == null)
            return false;

        if (CountAllCopies(data) < 1)
            return false;

        int maxLevel = GetHighestLevel(data);

        if (maxLevel + 1 > data.maxUpgradeLevel)
            return false;

        var cost = GetUpgradeCost(data, maxLevel);
        bool hasGold = CurrencyManager.Instance != null && CurrencyManager.Instance.Has(CurrencyType.Gold, cost.gold);
        bool hasMaterial = cost.material == null || (InventoryManager.Instance != null && InventoryManager.Instance.GetQuantity(cost.material) >= cost.materialQty);
        return hasGold && hasMaterial;
    }

    public bool OwnsUpgradeable(EquipmentData data)
    {
        if (data == null || CountAllCopies(data) < 1)
            return false;

        return GetHighestLevel(data) + 1 <= data.maxUpgradeLevel;
    }

    public EquipmentInstance UpgradeFuse(EquipmentData data)
    {
        if (!CanUpgradeFuse(data))
            return null;

        int currentLevel = GetHighestLevel(data);
        var cost = GetUpgradeCost(data, currentLevel);

        if (CurrencyManager.Instance != null)
            CurrencyManager.Instance.Spend(CurrencyType.Gold, cost.gold);

        if (cost.material != null && cost.materialQty > 0 && InventoryManager.Instance != null)
            InventoryManager.Instance.RemoveItem(cost.material, cost.materialQty);

        var em = EquipmentManager.Instance;
        var equipped = em != null ? em.GetEquipped(data.slot) : null;
        bool equippedIsBest = equipped != null && equipped.baseData.itemID == data.itemID && equipped.upgradeLevel == currentLevel;

        EquipmentInstance result;

        if (equippedIsBest)
        {
            em.UpgradeEquipped(data.slot);
            result = em.GetEquipped(data.slot);
        }
        else
        {
            InventoryManager.Instance.RemoveUpgradedItem(data, currentLevel, 1);
            result = new EquipmentInstance(data, currentLevel + 1);
            InventoryManager.Instance.AddInstance(result, 1);
        }

        SaveSystem.SaveGame();
        return result;
    }

    private int CountAllCopies(EquipmentData data)
    {
        int count = 0;
        var em = EquipmentManager.Instance;

        if (em != null)
        {
            var inst = em.GetEquipped(data.slot);

            if (inst != null && inst.baseData.itemID == data.itemID)
                count++;
        }

        if (InventoryManager.Instance != null)
            count += InventoryManager.Instance.GetTotalQuantity(data.itemID);

        return count;
    }

    private int GetHighestLevel(EquipmentData data)
    {
        int maxLevel = 0;
        var em = EquipmentManager.Instance;

        if (em != null)
        {
            var inst = em.GetEquipped(data.slot);

            if (inst != null && inst.baseData.itemID == data.itemID)
                maxLevel = Mathf.Max(maxLevel, inst.upgradeLevel);
        }

        if (InventoryManager.Instance != null)
        {
            foreach (var (inst, _) in InventoryManager.Instance.GetEquipmentInstances())
                if (inst.baseData.itemID == data.itemID)
                    maxLevel = Mathf.Max(maxLevel, inst.upgradeLevel);
        }

        return maxLevel;
    }
}
