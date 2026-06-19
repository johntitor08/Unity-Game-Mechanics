using UnityEngine;

[System.Serializable]
public class CurrencyShopItem : MonoBehaviour
{
    public string itemName;
    public string description;
    public Sprite icon;
    public CurrencyType costType;
    public int cost;
    public MultiCurrencyCost[] costs;
    public CurrencyType rewardType;
    public int rewardAmount;
}
