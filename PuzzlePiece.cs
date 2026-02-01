using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class PuzzlePiece : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Vector2Int gridPos;
    private PuzzleManager manager;
    private RectTransform rt;
    private Canvas canvas;
    private bool placed;
    private Vector2 dragOffset;
    private Camera uiCamera;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        uiCamera = canvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : canvas.worldCamera;
    }

    public void Initialize(Vector2Int gp, PuzzleManager pm)
    {
        gridPos = gp;
        manager = pm;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (placed) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt.parent as RectTransform,
            e.position,
            uiCamera,
            out Vector2 localMousePos
        );

        dragOffset = rt.anchoredPosition - localMousePos;
        PuzzleEvents.OnMoveMade?.Invoke();
    }

    public void OnDrag(PointerEventData e)
    {
        if (placed) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rt.parent as RectTransform,
            e.position,
            uiCamera,
            out Vector2 localMousePos
        );

        rt.anchoredPosition = localMousePos + dragOffset;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (placed) return;
        Vector2 correct = manager.GetCorrectPosition(gridPos);

        if (Vector2.Distance(rt.anchoredPosition, correct) < manager.GetSnapDistance())
        {
            rt.anchoredPosition = correct;
            placed = true;
            GetComponent<Image>().raycastTarget = false;
            rt.SetAsLastSibling();
            manager.PlaySnapSound();
            manager.CheckWin();
        }
    }

    public bool IsInCorrectPosition() => placed;

    public void ResetPiece()
    {
        placed = false;
        GetComponent<Image>().raycastTarget = true;
    }
}
