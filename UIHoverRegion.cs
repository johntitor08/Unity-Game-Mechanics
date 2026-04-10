using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class UIHoverRegion : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public SpriteShapeRenderer hoverShape;
    public float maxAlpha = 0.2f;
    public float fadeSpeed = 6f;
    float alpha = 0f;
    bool hovering = false;
    public event Action OnRegionClicked;

    void Start()
    {
        SetAlpha(0);
    }

    void Update()
    {
        float target = hovering ? 1f : 0f;
        alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * fadeSpeed);
        SetAlpha(alpha);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (DialogueManager.Instance != null && DialogueManager.Instance.IsInDialogue())
            return;

        OnRegionClicked?.Invoke();
    }

    void SetAlpha(float a)
    {
        Color c = hoverShape.color;
        c.a = a * maxAlpha;
        hoverShape.color = c;
    }
}
