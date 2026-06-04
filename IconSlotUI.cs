using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class IconSlotUI : MonoBehaviour
{
    public Image iconImage;
    public Image lockedOverlay;
    public Image selectedOutline;
    public TextMeshProUGUI costText;
    public Button button;
    private string iconID;
    private Action<string> onClicked;

    public void Setup(IconEntry entry, bool unlocked, bool selected, Action<string> onClick)
    {
        iconID = entry.id;
        onClicked = onClick;
        if (iconImage != null) iconImage.sprite = entry.sprite;
        if (lockedOverlay != null) lockedOverlay.gameObject.SetActive(!unlocked);
        if (selectedOutline != null) selectedOutline.gameObject.SetActive(selected);
        if (costText != null) costText.text = unlocked ? "" : $"{entry.cost}g";

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => onClicked?.Invoke(iconID));
            button.interactable = true;
        }
    }
}
