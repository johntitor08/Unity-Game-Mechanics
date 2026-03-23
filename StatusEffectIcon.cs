using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectIcon : MonoBehaviour
{
    [Header("UI Elements")]
    public Image iconImage;
    public Image backgroundImage;
    public TextMeshProUGUI durationText;
    public TextMeshProUGUI stackText;
    public Image fillImage;

    [HideInInspector]
    public StatusEffectType effectType;

    private ActiveStatusEffect currentEffect;

    public void Setup(ActiveStatusEffect effect)
    {
        currentEffect = effect;
        effectType = effect.data.effectType;

        if (iconImage != null && effect.data.icon != null)
            iconImage.sprite = effect.data.icon;

        if (backgroundImage != null)
        {
            Color bgColor = effect.data.effectColor;
            bgColor.a = 0.5f;
            backgroundImage.color = bgColor;
        }

        if (fillImage != null)
            fillImage.color = effect.data.effectColor;

        UpdateEffect(effect);
    }

    public void UpdateEffect(ActiveStatusEffect effect)
    {
        currentEffect = effect;

        if (durationText != null)
        {
            if (effect.data.isPermanent)
            {
                durationText.text = "∞";
            }
            else
            {
                durationText.text = Mathf.Ceil(effect.remainingDuration).ToString();
            }
        }

        if (stackText != null)
        {
            if (effect.data.canStack && effect.currentStacks > 1)
            {
                stackText.gameObject.SetActive(true);
                stackText.text = $"x{effect.currentStacks}";
            }
            else
            {
                stackText.gameObject.SetActive(false);
            }
        }

        if (fillImage != null && !effect.data.isPermanent)
        {
            float fillAmount = effect.remainingDuration / effect.data.duration;
            fillImage.fillAmount = fillAmount;
        }
    }
}
