using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RewardItemUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI amountText;

    public void Setup(string itemName, string amount, Sprite itemIcon)
    {
        if (nameText != null)
            nameText.text = itemName;

        if (amountText != null)
            amountText.text = amount;

        if (icon != null && itemIcon != null)
        {
            icon.sprite = itemIcon;
            icon.enabled = true;
        }
        else if (icon != null)
        {
            icon.enabled = false;
        }
    }
}
