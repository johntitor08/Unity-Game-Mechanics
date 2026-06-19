using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BuffUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Image icon;
    public TextMeshProUGUI timerText;
    public GameObject tooltip;
    public TextMeshProUGUI tooltipText;
    private string buffName;
    private float duration;

    public void Setup(Sprite buffIcon, string name, float duration)
    {
        icon.sprite = buffIcon;
        buffName = name;
        this.duration = duration;
        timerText.text = duration.ToString("F1") + "s";

        if (tooltip != null)
        {
            tooltip.SetActive(false);
            tooltipText.text = $"{buffName}\nDuration: {duration:F1}s";
        }
    }

    public void UpdateTimer(float timeLeft)
    {
        timerText.text = Mathf.Max(0f, timeLeft).ToString("F1") + "s";

        if (tooltip != null)
            tooltipText.text = $"{buffName}\nDuration: {Mathf.Max(0f, timeLeft):F1}s";
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltip != null)
            tooltip.SetActive(false);
    }
}
