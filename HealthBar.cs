using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private Gradient gradient;
    [SerializeField] private Image fill;

    public void SetMaxHealth(int health)
    {
        slider.maxValue = health;
        slider.value = health;
        fill.color = gradient.Evaluate(1);

    }

    public void SetHealth(int health)
    {
        slider.value = health;
        fill.color = gradient.Evaluate(slider.normalizedValue);

    }

    public void FlipHealthBar()
    {
        Vector3 healthBarlocalScale = slider.transform.localScale;
        healthBarlocalScale.x *= -1;
        slider.transform.localScale = healthBarlocalScale;

    }

    public void FlipHealthBar(int sign)
    {
        slider.transform.localScale = new Vector2(sign * Mathf.Abs(slider.transform.localScale.x), slider.transform.localScale.y);

    }

}