using UnityEngine;
using UnityEngine.Events;

[CreateAssetMenu(fileName = "ItemData", menuName = "Inventory/ItemData")]
public class ItemData : ScriptableObject
{
    [Header("Identity")]
    public string itemID;
    public string itemName;
    public Sprite icon;

    [TextArea]
    public string description;

    [Header("Stacking")]
    public bool stackable = true;
    public int maxStack = 99;

    [Header("Usage")]
    public bool useable;
    public StatType affectedStat;
    public int statAmount;
    public UnityEvent onUse;

    [Header("Economy")]
    public int basePrice = 10;

    [Header("Rarity")]
    public Rarity rarity = Rarity.Common;

    public Color GetRarityColor()
    {
        return rarity switch
        {
            Rarity.Common => new Color(0.8f, 0.8f, 0.8f),
            Rarity.Rare => new Color(0.2f, 0.5f, 1f),
            Rarity.Epic => new Color(0.8f, 0.2f, 0.8f),
            Rarity.Legendary => new Color(1f, 0.6f, 0f),
            _ => Color.white
        };
    }

    public int GetSellPrice(float sellRatio = 0.5f)
    {
        return Mathf.RoundToInt(basePrice * GetRarityMultiplier() * sellRatio);
    }

    public float GetRarityMultiplier()
    {
        return rarity switch
        {
            Rarity.Common => 1f,
            Rarity.Rare => 1.5f,
            Rarity.Epic => 2.5f,
            Rarity.Legendary => 5f,
            _ => 1f
        };
    }
}

public enum Rarity
{
    Common,
    Rare,
    Epic,
    Legendary
}
