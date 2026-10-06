using UnityEngine;
using UnityEngine.UI;
[DisallowMultipleComponent]

[RequireComponent(typeof(CanvasGroup))]
public class PanelDim : MonoBehaviour
{
    [Range(0f, 1f)]
    public float darkness = 0.7f;
    Image _dim;
    CanvasGroup _group;

    void LateUpdate()
    {
        if (_dim == null)
            return;

        if (_group == null)
            _group = GetComponent<CanvasGroup>();

        float a = _group != null ? _group.alpha : 1f;
        var c = _dim.color;

        if (!Mathf.Approximately(c.a, darkness * a))
            _dim.color = new Color(c.r, c.g, c.b, darkness * a);
    }

    void OnEnable()
    {
        EnsureDim();

        if (_dim == null)
            return;

        _dim.gameObject.SetActive(true);

        if (_group == null)
            _group = GetComponent<CanvasGroup>();

        _dim.color = new Color(0f, 0f, 0f, darkness * (_group != null ? _group.alpha : 1f));
        _dim.transform.SetAsLastSibling();
        transform.SetAsLastSibling();
    }

    void OnDisable()
    {
        if (_dim != null)
            _dim.gameObject.SetActive(false);
    }

    void EnsureDim()
    {
        if (_dim != null)
            return;

        var canvas = GetComponentInParent<Canvas>();

        if (canvas == null)
            return;

        var go = new GameObject("Dim", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        go.transform.SetParent(canvas.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _dim = go.GetComponent<Image>();
        _dim.color = new Color(0f, 0f, 0f, darkness);
        _dim.raycastTarget = true;
    }
}
