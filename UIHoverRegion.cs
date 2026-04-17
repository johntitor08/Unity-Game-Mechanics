using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

[RequireComponent(typeof(SpriteShapeRenderer))]
[RequireComponent(typeof(PolygonCollider2D))]
public class UIHoverRegion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SpriteShapeRenderer hoverShape;
    public float maxAlpha = 0.2f;
    public float fadeSpeed = 6f;
    private float currentAlpha = 0f;
    private Coroutine fadeRoutine;
    private bool isDialogueSubscribed;
    public event Action OnRegionClicked;

    void Start()
    {
        if (hoverShape == null)
            hoverShape = GetComponent<SpriteShapeRenderer>();

        SetAlpha(0);
    }

    void OnEnable()
    {
        StartCoroutine(TrySubscribe());
    }

    private IEnumerator TrySubscribe()
    {
        while (DialogueManager.Instance == null)
            yield return null;

        SubscribeDialogue();
    }

    void OnDisable() => UnsubscribeDialogue();

    void OnDestroy() => OnRegionClicked = null;

    private void SubscribeDialogue()
    {
        if (isDialogueSubscribed)
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
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue())
            return;

        StartFade(1f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StartFade(0f);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue())
            return;

        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        currentAlpha = 0f;
        SetAlpha(0f);
        OnRegionClicked?.Invoke();
    }

    private void HandleDialogueStarted(DialogueNode node)
    {
        StartFade(0f);
    }

    private void StartFade(float target)
    {
        if (fadeRoutine != null)
            StopCoroutine(fadeRoutine);

        fadeRoutine = StartCoroutine(FadeRoutine(target));
    }

    private IEnumerator FadeRoutine(float targetAlpha)
    {
        while (!Mathf.Approximately(currentAlpha, targetAlpha))
        {
            float finalTarget = (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue()) ? 0f : targetAlpha;
            currentAlpha = Mathf.MoveTowards(currentAlpha, finalTarget, Time.deltaTime * fadeSpeed);
            SetAlpha(currentAlpha);
            yield return null;
        }
    }

    private void SetAlpha(float a)
    {
        Color c = hoverShape.color;
        c.a = a * maxAlpha;
        hoverShape.color = c;
    }
}
