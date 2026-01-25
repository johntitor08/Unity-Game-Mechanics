using TMPro;
using UnityEngine;

public class ItemTooltip : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;

    public void Show(ItemData item)
    {
        int sellPrice = item.GetSellPrice();

        tooltipText.text =
            $@"<b>{item.itemName}</b>
            <color=#{ColorUtility.ToHtmlStringRGB(item.GetRarityColor())}>
            {item.rarity}
            </color>
            Sell Price: {sellPrice} Gold";

        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
