using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class RewardItemUI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI amountText;
    public Button selectButton;
    public GameObject selectedIndicator;

    public void Setup(string itemName, string amount, Sprite itemIcon, Action onClicked = null, bool isSelected = false)
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

        if (selectButton == null)
            selectButton = GetComponent<Button>();

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.interactable = onClicked != null;

            if (onClicked != null)
                selectButton.onClick.AddListener(() => onClicked());
        }

        if (selectedIndicator != null)
            selectedIndicator.SetActive(isSelected);
    }
}
