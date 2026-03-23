using System.Collections.Generic;
using UnityEngine;

public class FusionManager : MonoBehaviour
{
    public static FusionManager Instance;

    [Header("Recipes")]
    public List<FusionRecipe> recipes = new();

    void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
    }

    public FusionRecipe FindRecipe(EquipmentData a, EquipmentData b)
    {
        foreach (var r in recipes)
        {
            bool match = (r.ingredientA.itemID == a.itemID && r.ingredientB.itemID == b.itemID) || (r.ingredientA.itemID == b.itemID && r.ingredientB.itemID == a.itemID);
            if (match) return r;
        }

        return null;
    }

    public bool CanFuse(EquipmentData a, EquipmentData b)
    {
        if (a == null || b == null || a.itemID == b.itemID) return false;

        if (EquipmentManager.Instance != null)
        {
            if (EquipmentManager.Instance.IsEquipped(a) ||
                EquipmentManager.Instance.IsEquipped(b))
                return false;
        }

        if (InventoryManager.Instance.GetQuantity(a) <= 0 ||
            InventoryManager.Instance.GetQuantity(b) <= 0)
            return false;

        return FindRecipe(a, b) != null;
    }

    public EquipmentData Fuse(EquipmentData a, EquipmentData b)
    {
        if (!CanFuse(a, b)) return null;
        FusionRecipe recipe = FindRecipe(a, b);
        InventoryManager.Instance.RemoveItem(a, 1);
        InventoryManager.Instance.RemoveItem(b, 1);

        if (Random.value <= recipe.successChance)
        {
            InventoryManager.Instance.AddItem(recipe.result, 1);
            return recipe.result;
        }

        return null;
    }
}
