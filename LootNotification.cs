using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LootNotification : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup;
    private float duration;
    private float elapsed;

    public void Setup(string message, Color color, Sprite icon, float dur)
    {
        if (messageText != null)
        {
            messageText.text = message;
            messageText.color = color;
        }

        if (iconImage != null && icon != null)
        {
            iconImage.sprite = icon;
            iconImage.enabled = true;
        }
        else if (iconImage != null)
        {
            iconImage.enabled = false;
        }

        duration = dur;
        elapsed = 0f;

        if (canvasGroup != null)
            canvasGroup.alpha = 1f;
    }

    void Update()
    {
        elapsed += Time.deltaTime;

        // Fade out son 1 saniye
        if (elapsed > duration - 1f && canvasGroup != null)
        {
            canvasGroup.alpha = 1f - ((elapsed - (duration - 1f)) / 1f);
        }

        if (elapsed >= duration)
        {
            if (LootNotificationUI.Instance != null)
            {
                LootNotificationUI.Instance.ReturnToPool(gameObject);
            }
        }
    }
}
