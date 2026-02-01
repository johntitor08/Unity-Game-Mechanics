using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class ClickableIcon : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    [Header("Visual Feedback")]
    public bool enableHoverEffect = true;
    public bool enableClickEffect = true;
    public bool enablePulseEffect = false;

    [Header("Hover Settings")]
    public float hoverScale = 1.15f;
    public float hoverDuration = 0.2f;
    public Color hoverTintColor = new(1f, 1f, 0.8f);

    [Header("Click Settings")]
    public float clickScale = 0.9f;
    public float clickDuration = 0.1f;

    [Header("Pulse Settings")]
    public float pulseSpeed = 2f;
    public float pulseAmount = 0.1f;

    [Header("Audio")]
    public AudioClip hoverSound;
    public AudioClip clickSound;

    private Vector3 originalScale;
    private Image iconImage;
    private Color originalColor;
    private bool isHovering = false;

    void Awake()
    {
        originalScale = transform.localScale;
        iconImage = GetComponent<Image>();

        if (iconImage != null)
            originalColor = iconImage.color;
    }

    void Update()
    {
        if (enablePulseEffect && !isHovering)
        {
            float scale = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = originalScale * scale;
        }
    }

    void OnDisable()
    {
        ResetVisual();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;
        if (!enableHoverEffect) return;
        isHovering = true;
        StopAllCoroutines();
        StartCoroutine(ScaleEffect(hoverScale, hoverDuration));

        if (iconImage != null)
            iconImage.color = hoverTintColor;

        PlaySound(hoverSound);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;
        if (!enableHoverEffect) return;
        isHovering = false;
        StopAllCoroutines();
        StartCoroutine(ScaleEffect(1f, hoverDuration));

        if (iconImage != null)
            iconImage.color = originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isActiveAndEnabled) return;
        if (!enableClickEffect) return;
        StopAllCoroutines();
        StartCoroutine(ClickEffect());
        PlaySound(clickSound);
    }

    System.Collections.IEnumerator ScaleEffect(float targetScale, float duration)
    {
        if (gameObject == null) yield break;
        Vector3 startScale = transform.localScale;
        Vector3 endScale = originalScale * targetScale;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            transform.localScale = Vector3.Lerp(startScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
    }

    System.Collections.IEnumerator ClickEffect()
    {
        if (gameObject == null) yield break;
        Vector3 currentScale = transform.localScale;
        Vector3 clickedScale = originalScale * clickScale;
        float elapsed = 0f;
        
        while (elapsed < clickDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / clickDuration;
            transform.localScale = Vector3.Lerp(currentScale, clickedScale, t);
            yield return null;
        }

        float targetScale = isHovering ? hoverScale : 1f;
        Vector3 endScale = originalScale * targetScale;
        
        while (elapsed < clickDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / clickDuration;
            transform.localScale = Vector3.Lerp(clickedScale, endScale, t);
            yield return null;
        }

        transform.localScale = endScale;
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
        }
    }

    public void ResetVisual()
    {
        StopAllCoroutines();
        transform.localScale = originalScale;

        if (iconImage != null)
            iconImage.color = originalColor;
        
        isHovering = false;
    }
}
