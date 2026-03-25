using TMPro;
using UnityEngine;

public class ItemTooltip : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;

    public void Show(ItemData item, int upgradeLevel = 0)
    {
        int sellPrice = item.GetSellPrice();
        string upgradeStr = upgradeLevel > 0 ? $"  <color=#FFD700>+{upgradeLevel}</color>" : "";
        tooltipText.text = $"{item.itemName}{upgradeStr}\nSell Price: {sellPrice} Gold";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
