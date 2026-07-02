using TMPro;
using UnityEngine;

public class ItemTooltip : MonoBehaviour
{
    public TextMeshProUGUI tooltipText;

    public void Show(ItemData item, int upgradeLevel = 0)
    {
        int sellPrice = item.GetSellPrice();
        string upgradeStr = upgradeLevel > 0 ? $"  <color=#FFD700>+{upgradeLevel}</color>" : "";
        tooltipText.text = $"{item.DisplayName}{upgradeStr}\n{Loc.T("Sell Price", "Satış Fiyatı")}: {sellPrice} {Loc.T("Gold", "Altın")}";
        gameObject.SetActive(true);
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
