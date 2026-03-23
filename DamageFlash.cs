using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class DamageFlash : MonoBehaviour
{
    [Header("Target Stats")]
    [SerializeField] private StatsBase _statsTarget;
    public StatsBase StatsTarget
    {
        get => _statsTarget;
        set
        {
            if (_statsTarget != null)
                _statsTarget.OnStatChanged -= HandleStatChanged;

            _statsTarget = value;
            isSubscribed = false;
            TrySubscribe();
        }
    }

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
    private bool isSubscribed = false;
    private StatsBase subscribedTarget;

    void Awake()
    {
        if (_statsTarget == null)
        {
            StatsBase localStats = GetComponentInParent<StatsBase>();
            _statsTarget = localStats != null ? localStats : PlayerStats.Instance;
        }

        if (flashImage != null)
            flashImage.color = new Color(0f, 0f, 0f, 0f);

        if (shakeTarget == null && flashImage != null)
            shakeTarget = flashImage.rectTransform;

        if (shakeTarget != null)
            originalPosition = shakeTarget.anchoredPosition;
    }

    void OnEnable()
    {
        TrySubscribe();
    }

    void OnDisable()
    {
        if (isSubscribed && subscribedTarget != null)
        {
            subscribedTarget.OnStatChanged -= HandleStatChanged;
            isSubscribed = false;
            subscribedTarget = null;
        }

        if (flashRoutine != null)
        {
            StopCoroutine(flashRoutine);
            flashRoutine = null;
        }

        if (flashImage != null)
            flashImage.color = new Color(0f, 0f, 0f, 0f);
    }

    void TrySubscribe()
    {
        if (subscribedTarget != null && subscribedTarget != _statsTarget)
        {
            subscribedTarget.OnStatChanged -= HandleStatChanged;
            isSubscribed = false;
            subscribedTarget = null;
        }

        if (_statsTarget != null && !isSubscribed)
        {
            _statsTarget.OnStatChanged += HandleStatChanged;
            isSubscribed = true;
            subscribedTarget = _statsTarget;

            if (healthBarFill != null)
            {
                int health = _statsTarget.Get(StatType.Health);
                int maxHealth = _statsTarget.Get(StatType.MaxHealth);
                healthBarFill.fillAmount = Mathf.Clamp01((float)health / maxHealth);
            }
        }
    }

    private void HandleStatChanged(StatType type, int oldValue, int newValue)
    {
        if (type != StatType.Health)
            return;

        int delta = newValue - oldValue;

        if (delta == 0)
            return;

        Color color = delta < 0 ? damageColor : healColor;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(Flash(color));

        if (enableShake && delta < 0)
            StartCoroutine(Shake());

        if (healthBarFill != null)
        {
            int maxHealth = _statsTarget.Get(StatType.MaxHealth);
            healthBarFill.fillAmount = Mathf.Clamp01((float)newValue / maxHealth);
        }
    }

    IEnumerator Flash(Color color)
    {
        if (flashImage == null)
            yield break;

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
        if (shakeTarget == null)
            yield break;

        float elapsed = 0f;
        Vector2 startPos = originalPosition;

        while (elapsed < shakeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = elapsed / shakeDuration;
            float damper = 1f - progress;
            float x = Mathf.Sin(elapsed * 40f) * shakeMagnitude * damper * Random.Range(0.8f, 1.2f);
            float y = Mathf.Sin(elapsed * 50f) * shakeMagnitude * damper * Random.Range(0.8f, 1.2f);
            shakeTarget.anchoredPosition = startPos + new Vector2(x, y);
            yield return null;
        }

        shakeTarget.anchoredPosition = startPos;
    }

    public void TriggerFlash(Color color)
    {
        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(Flash(color));
    }
}
