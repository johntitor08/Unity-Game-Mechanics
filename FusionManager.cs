using System.Collections.Generic;
using UnityEngine;

public class FusionManager : MonoBehaviour
{
    public static FusionManager Instance;

    [Header("Recipes")]
    public List<FusionRecipe> recipes = new();

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

        if (Random.value <= recipe.successChance)
        {
            InventoryManager.Instance.AddItem(recipe.result, 1);
            return recipe.result;
        }

        return null;
    }

    public bool CanUpgradeFuse(EquipmentData data)
    {
        if (data == null)
            return false;

        int total = CountAllCopies(data);

        if (total < 2)
            return false;

        int maxLevel = GetHighestLevel(data);
        return maxLevel + 1 <= data.maxUpgradeLevel;
    }

    public EquipmentInstance UpgradeFuse(EquipmentData data)
    {
        if (!CanUpgradeFuse(data))
            return null;

        var (copies, equippedInst) = GatherCopies(data);

        copies.Sort((a, b) =>
        {
            if (a.upgradeLevel != b.upgradeLevel)
                return b.upgradeLevel - a.upgradeLevel;

            bool aEq = ReferenceEquals(a, equippedInst);
            bool bEq = ReferenceEquals(b, equippedInst);

            if (aEq && !bEq)
                return -1;

            if (bEq && !aEq)
                return 1;

            return 0;
        });

        var keep = copies[0];
        var consume = copies[1];
        int resultLevel = keep.upgradeLevel + 1;
        bool consumeIsEquipped = ReferenceEquals(consume, equippedInst);

        if (consumeIsEquipped)
            EquipmentManager.Instance.Unequip(data.slot, returnToInventory: false, save: false);
        else
            InventoryManager.Instance.RemoveUpgradedItem(data, consume.upgradeLevel, 1);

        bool keepIsEquipped = ReferenceEquals(keep, equippedInst);
        EquipmentInstance result;

        if (keepIsEquipped)
        {
            EquipmentManager.Instance.UpgradeEquipped(data.slot);
            result = EquipmentManager.Instance.GetEquipped(data.slot);
        }
        else
        {
            InventoryManager.Instance.RemoveUpgradedItem(data, keep.upgradeLevel, 1);
            result = new EquipmentInstance(data, resultLevel);
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

    private (List<EquipmentInstance> copies, EquipmentInstance equipped) GatherCopies(EquipmentData data)
    {
        var copies = new List<EquipmentInstance>();
        EquipmentInstance equippedInst = null;
        var em = EquipmentManager.Instance;

        if (em != null)
        {
            var inst = em.GetEquipped(data.slot);

            if (inst != null && inst.baseData.itemID == data.itemID)
            {
                equippedInst = inst;
                copies.Add(inst);
            }
        }

        if (InventoryManager.Instance != null)
        {
            foreach (var (inst, qty) in InventoryManager.Instance.GetEquipmentInstances())
            {
                if (inst.baseData.itemID != data.itemID) continue;

                for (int i = 0; i < qty; i++)
                    copies.Add(new EquipmentInstance(data, inst.upgradeLevel));
            }
        }

        return (copies, equippedInst);
    }
}
