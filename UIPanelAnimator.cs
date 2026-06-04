using System.Collections;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
[DisallowMultipleComponent]
public class UIPanelAnimator : MonoBehaviour
{
    public float duration = 0.14f;
    public bool animateScale = true;
    [Range(0.5f, 1f)] public float startScale = 0.94f;

    private CanvasGroup _cg;
    private RectTransform _rt;
    private Coroutine _routine;

    void Awake()
    {
        _cg = GetComponent<CanvasGroup>();
        _rt = transform as RectTransform;
    }

    void OnEnable()
    {
        PlayOpen();
    }

    void OnDisable()
    {
        _routine = null;

        if (_cg != null)
            _cg.alpha = 1f;

        if (_rt != null)
            _rt.localScale = Vector3.one;
    }

    public void PlayOpen()
    {
        if (_cg == null)
            _cg = GetComponent<CanvasGroup>();

        if (!gameObject.activeInHierarchy)
            return;

        StopRoutine();
        _cg.alpha = 0f;
        _routine = StartCoroutine(Animate(0f, 1f, true, null));
    }

    public void PlayClose(System.Action onComplete)
    {
        if (_cg == null)
            _cg = GetComponent<CanvasGroup>();

        if (!gameObject.activeInHierarchy)
        {
            onComplete?.Invoke();
            return;
        }

        StopRoutine();
        _routine = StartCoroutine(Animate(_cg.alpha, 0f, false, onComplete));
    }

    IEnumerator Animate(float from, float to, bool opening, System.Action onComplete)
    {
        if (_rt == null)
            _rt = transform as RectTransform;

        _cg.blocksRaycasts = true;
        _cg.interactable = false;
        float t = 0f;
        float dur = Mathf.Max(0.0001f, duration);

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / dur);
            float eased = opening ? 1f - (1f - k) * (1f - k) : k * k;
            _cg.alpha = Mathf.Lerp(from, to, eased);

            if (animateScale && _rt != null)
            {
                float s = opening ? Mathf.Lerp(startScale, 1f, eased) : Mathf.Lerp(1f, startScale, eased);
                _rt.localScale = new Vector3(s, s, 1f);
            }

            yield return null;
        }

        if (animateScale && _rt != null)
            _rt.localScale = Vector3.one;

        _cg.alpha = to;
        _cg.interactable = opening;
        _cg.blocksRaycasts = opening;
        _routine = null;
        onComplete?.Invoke();
    }

    void StopRoutine()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }
    }
}
