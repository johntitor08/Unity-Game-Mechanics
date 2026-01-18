using UnityEngine;
using UnityEngine.UI;

public class DamageFlash : MonoBehaviour
{
    [Header("Flash Settings")]
    public Image flashImage;
    public Color damageColor = new(1f, 0f, 0f, 0.3f);
    public Color healColor = new(0f, 1f, 0f, 0.3f);
    public float flashDuration = 0.2f;

    void Start()
    {
        if (flashImage != null)
        {
            Color c = flashImage.color;
            c.a = 0f;
            flashImage.color = c;
        }

        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatChanged += OnStatChanged;
        }
    }

    void OnDestroy()
    {
        if (PlayerStats.Instance != null)
        {
            PlayerStats.Instance.OnStatChanged -= OnStatChanged;
        }
    }

    void OnStatChanged(StatType type, int oldValue, int newValue)
    {
        if (type == StatType.Health)
        {
            int change = newValue - oldValue;

            if (change < 0)
            {
                // Damage taken
                StartCoroutine(Flash(damageColor));
            }
            else if (change > 0)
            {
                // Healing
                StartCoroutine(Flash(healColor));
            }
        }
    }

    System.Collections.IEnumerator Flash(Color color)
    {
        if (flashImage == null) yield break;

        float elapsed = 0f;
        Color startColor = color;
        Color endColor = color;
        endColor.a = 0f;

        while (elapsed < flashDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / flashDuration;
            flashImage.color = Color.Lerp(startColor, endColor, t);
            yield return null;
        }

        flashImage.color = endColor;
    }
}
