using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DamageFlash : MonoBehaviour
{
    [Header("Target Stats")]
    public StatsBase statsTarget;

    [Header("Flash Settings")]
    public Image flashImage;
    public Color damageColor = new(1f, 0f, 0f, 0.5f);
    public Color healColor = new(0f, 1f, 0f, 0.5f);
    public float flashDuration = 0.2f;

    [Header("Shake Settings")]
    public bool enableShake = false;
    public RectTransform shakeTarget;
    public float shakeMagnitude = 5f;
    public float shakeDuration = 0.1f;

    [Header("Health Bar")]
    public Image healthBarFill;

    private Coroutine flashRoutine;
    private Vector2 originalPosition;

    void Awake()
    {
        if (flashImage != null)
            flashImage.color = new Color(0f, 0f, 0f, 0f);

        if (shakeTarget == null && flashImage != null)
            shakeTarget = flashImage.rectTransform;

        if (shakeTarget != null)
            originalPosition = shakeTarget.anchoredPosition;
    }

    void OnEnable()
    {
        if (statsTarget != null)
        {
            statsTarget.OnStatChanged += HandleStatChanged;

            if (healthBarFill != null)
            {
                int health = statsTarget.Get(StatType.Health);
                int maxHealth = statsTarget.Get(StatType.MaxHealth);
                healthBarFill.fillAmount = Mathf.Clamp01((float)health / maxHealth);
            }
        }
    }

    void OnDisable()
    {
        if (statsTarget != null)
            statsTarget.OnStatChanged -= HandleStatChanged;
    }

    private void HandleStatChanged(StatType type, int oldValue, int newValue)
    {
        if (type != StatType.Health) return;
        int delta = newValue - oldValue;
        if (delta == 0) return;
        Color color = delta < 0 ? damageColor : healColor;
        
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash(color));

        // Shake
        if (enableShake && delta < 0)
            StartCoroutine(Shake());

        // Health bar
        if (healthBarFill != null)
        {
            int maxHealth = statsTarget.Get(StatType.MaxHealth);
            healthBarFill.fillAmount = Mathf.Clamp01((float)newValue / maxHealth);
        }
    }

    IEnumerator Flash(Color color)
    {
        if (flashImage == null) yield break;
        float t = 0f;
        Color start = color;
        Color end = color;
        end.a = 0f;

        while (t < flashDuration)
        {
            t += Time.deltaTime;
            flashImage.color = Color.Lerp(start, end, t / flashDuration);
            yield return null;
        }

        flashImage.color = end;
    }

    IEnumerator Shake()
    {
        if (shakeTarget == null) yield break;
        float elapsed = 0f;
        Vector2 startPos = originalPosition;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            float damper = 1f - progress; // linear damping (can also use Mathf.Pow(progress, 2) for stronger easing)
            float x = Mathf.Sin(elapsed * 40f) * shakeMagnitude * damper * Random.Range(0.8f, 1.2f);
            float y = Mathf.Sin(elapsed * 50f) * shakeMagnitude * damper * Random.Range(0.8f, 1.2f);
            shakeTarget.anchoredPosition = startPos + new Vector2(x, y);
            yield return null;
        }

        // Restore original position
        shakeTarget.anchoredPosition = startPos;
    }

    public void TriggerFlash(Color color)
    {
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(Flash(color));
    }
}
