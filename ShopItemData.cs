using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "Shop/ShopItemData")]
public class ShopItemData : ScriptableObject
{
    public ItemData item;
    public int price;
    public bool unlimitedStock = true;
    public int stockAmount = 1;

    [Header("Sell Settings")]
    [Range(0f, 1f)]
    public float sellRatio = 0.5f;

    [Header("Requirements")]
    public int requiredLevel = 1;
    public bool requiresFlag;
    public string requiredFlag;
}
