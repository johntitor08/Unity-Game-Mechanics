using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.U2D;

public class UIHoverRegion : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IPointerClickHandler
{
    [Header("References")]
    public RectTransform rect;
    public SpriteShapeRenderer hoverShape;
    public LineRenderer outline;
    public Camera uiCamera;

    [Header("Hover Effect")]
    public float maxAlpha = 0.2f;
    public float fadeSpeed = 6f;
    public float drawSpeed = 3f;

    float alpha = 0f;
    float drawProgress = 0f;
    bool hovering = false;
    Vector2[] shapePoints;

    void Start()
    {
        var controller = hoverShape.GetComponent<SpriteShapeController>();
        int count = controller.spline.GetPointCount();
        shapePoints = new Vector2[count];

        for (int i = 0; i < count; i++)
            shapePoints[i] = controller.spline.GetPosition(i);

        outline.positionCount = count + 1;
        outline.useWorldSpace = false;
        outline.gameObject.SetActive(false);
        SetAlpha(0);
    }

    void Update()
    {
        float target = hovering ? 1f : 0f;
        alpha = Mathf.MoveTowards(alpha, target, Time.deltaTime * fadeSpeed);
        SetAlpha(alpha);

        if (!hovering)
        {
            drawProgress = 0;
            outline.gameObject.SetActive(false);
            return;
        }

        drawProgress = Mathf.MoveTowards(drawProgress, 1f, Time.deltaTime * drawSpeed);
        DrawOutline(drawProgress);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        hovering = true;
        outline.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        hovering = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (DialogueManager.Instance.IsInDialogue())
            return;

        if (SceneEvent.Instance.Progress == SceneProgress.Scene1)
            SceneEvent.Instance.TriggerScene2();
    }

    void SetAlpha(float a)
    {
        Color c = hoverShape.color;
        c.a = a * maxAlpha;
        hoverShape.color = c;
        outline.startColor = c;
        outline.endColor = c;
    }

    void DrawOutline(float t)
    {
        int visible = Mathf.FloorToInt(shapePoints.Length * t);

        for (int i = 0; i <= visible; i++)
        {
            Vector3 p = shapePoints[i % shapePoints.Length];
            outline.SetPosition(i, p);
        }

        if (visible >= shapePoints.Length - 1)
            outline.SetPosition(shapePoints.Length, outline.GetPosition(0));
    }
}
