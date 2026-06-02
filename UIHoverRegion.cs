using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
[RequireComponent(typeof(SpriteShapeRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class UIHoverRegion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SpriteShapeRenderer hoverShape;
    public SpriteShapeController spriteShapeController;
    public PolygonCollider2D hoverCollider;
    public float maxAlpha = 0.2f;
    public float fadeSpeed = 6f;
    private float currentAlpha = 0f;
    private Coroutine fadeRoutine;
    private Coroutine subscribeRoutine;
    private bool isDialogueSubscribed;
    public event Action OnRegionClicked;

    private void Awake()
    {
        CacheComponents();
        UpdateSpriteShapeCollider();
        SetAlpha(0f);
    }

    private void Reset()
    {
        CacheComponents();
        UpdateSpriteShapeCollider();
    }

    private void OnEnable()
    {
        subscribeRoutine = StartCoroutine(TrySubscribe());
    }

    private void OnDisable()
    {
        if (subscribeRoutine != null)
        {
            StopCoroutine(subscribeRoutine);
            subscribeRoutine = null;
        }

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        UnsubscribeDialogue();
    }

    private void OnDestroy()
    {
        UnsubscribeDialogue();
        OnRegionClicked = null;
    }

    private void CacheComponents()
    {
        if (hoverShape == null)
            hoverShape = GetComponent<SpriteShapeRenderer>();

        if (spriteShapeController == null)
            spriteShapeController = GetComponent<SpriteShapeController>();

        if (hoverCollider == null)
            hoverCollider = GetComponent<PolygonCollider2D>();

        if (hoverCollider != null)
            hoverCollider.isTrigger = true;
    }

    private void UpdateSpriteShapeCollider()
    {
        if (spriteShapeController == null)
            return;

        spriteShapeController.BakeCollider();
    }

    private IEnumerator TrySubscribe()
    {
        while (DialogueManager.Instance == null)
            yield return null;

        SubscribeDialogue();
        subscribeRoutine = null;
    }

    private void SubscribeDialogue()
    {
        if (isDialogueSubscribed || DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.OnDialogueStart += HandleDialogueStarted;
        isDialogueSubscribed = true;
    }

    private void UnsubscribeDialogue()
    {
        if (!isDialogueSubscribed || DialogueManager.Instance == null)
            return;

        DialogueManager.Instance.OnDialogueStart -= HandleDialogueStarted;
        isDialogueSubscribed = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (IsDialogueActive())
            return;

        StartFade(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsDialogueActive())
            return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        currentAlpha = 0f;
        SetAlpha(0f);
        OnRegionClicked?.Invoke();
    }

    private void HandleDialogueStarted(DialogueNode node)
    {
        StartFade(0f);
    }

    private bool IsDialogueActive()
    {
        return DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue();
    }

    private void StartFade(float targetAlpha)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        while (true)
        {
            float finalTarget = IsDialogueActive() ? 0f : targetAlpha;
            currentAlpha = Mathf.MoveTowards(currentAlpha, finalTarget, Time.deltaTime * fadeSpeed);
            SetAlpha(currentAlpha);

            if (Mathf.Approximately(currentAlpha, finalTarget))
                break;

            yield return null;
        }

        fadeRoutine = null;
    }

    private void SetAlpha(float alpha)
    {
        if (hoverShape == null)
            return;

        Color color = hoverShape.color;
        color.a = alpha * maxAlpha;
        hoverShape.color = color;
    }
}
