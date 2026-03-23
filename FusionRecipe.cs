using UnityEngine;

[CreateAssetMenu(fileName = "FusionRecipe", menuName = "Inventory/FusionRecipe")]
public class FusionRecipe : ScriptableObject
{
    public EquipmentData ingredientA;
    public EquipmentData ingredientB;
    public EquipmentData result;
    [Range(0f, 1f)] public float successChance = 1f;
}
