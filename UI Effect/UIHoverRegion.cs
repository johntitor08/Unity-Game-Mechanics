using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeController))]
[RequireComponent(typeof(SpriteShapeRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class UIHoverRegion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerClickHandler
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
    private bool pressedDuringDialogue;
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

        if (TryGetComponent<UnityEngine.UI.Image>(out var square))
            square.raycastTarget = false;
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

    public void OnPointerDown(PointerEventData eventData)
    {
        pressedDuringDialogue = IsDialogueActive() || IsSceneDark();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (IsDialogueActive() || IsSceneDark() || pressedDuringDialogue)
            return;

        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }

        currentAlpha = 0f;
        SetAlpha(0f);

        if (GameAudioManager.Instance != null && GetComponent<UIClickSound>() != null)
            GameAudioManager.Instance.PlayUiClick();

        OnRegionClicked?.Invoke();
    }

    private void HandleDialogueStarted(DialogueNode node)
    {
        StartFade(0f);
    }

    private bool IsDialogueActive() => DialogueManager.WorldClicksBlocked;

    private static bool IsSceneDark() => SceneEvent.Instance != null && SceneEvent.Instance.IsSceneDarkened;

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
            bool dark = IsSceneDark();
            float finalTarget = IsDialogueActive() || dark ? 0f : targetAlpha;
            currentAlpha = Mathf.MoveTowards(currentAlpha, finalTarget, Time.deltaTime * fadeSpeed);
            SetAlpha(currentAlpha);

            if (Mathf.Approximately(currentAlpha, finalTarget) && !(dark && targetAlpha > 0f))
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
