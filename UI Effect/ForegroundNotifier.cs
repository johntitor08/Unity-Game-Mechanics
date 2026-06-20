using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForegroundNotifier : MonoBehaviour
{
    public static ForegroundNotifier Instance;
    static readonly Color Cream = new(0.937f, 0.886f, 0.761f);
    Canvas _canvas;
    RectTransform _toastRoot;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        _canvas = GetComponentInParent<Canvas>();
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void ShowMessage(string message, float seconds)
    {
        if (_canvas == null)
            return;

        StartCoroutine(MessageRoutine(message, seconds));
    }

    IEnumerator MessageRoutine(string message, float seconds)
    {
        var go = new GameObject("ForegroundMessage", typeof(RectTransform));
        go.transform.SetParent(_canvas.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(1500f, 260f);
        var backdrop = go.AddComponent<Image>();
        backdrop.color = new Color(0f, 0f, 0f, 0.6f);
        backdrop.raycastTarget = false;
        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var trt = (RectTransform)textGo.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(40f, 0f);
        trt.offsetMax = new Vector2(-40f, 0f);
        var txt = textGo.AddComponent<TextMeshProUGUI>();
        txt.text = message;
        txt.fontSize = 80;
        txt.fontStyle = FontStyles.Bold;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = Cream;
        txt.raycastTarget = false;
        go.transform.SetAsLastSibling();
        yield return new WaitForSeconds(seconds);

        if (go != null)
            Destroy(go);
    }

    public void ShowLoot(ItemData item, int quantity = 1)
    {
        if (item == null)
            return;

        Color c = item is EquipmentData eq ? eq.GetRarityColor() : Cream;
        ShowToast($"+{quantity} {item.itemName}", item.icon, c);
    }

    public void ShowToast(string message, Sprite icon, Color tint, float seconds = 3f)
    {
        var root = ToastRoot();

        if (root == null)
            return;

        StartCoroutine(ToastRoutine(root, message, icon, tint, seconds));
    }

    RectTransform ToastRoot()
    {
        if (_toastRoot != null)
            return _toastRoot;

        if (_canvas == null)
            return null;

        var go = new GameObject("ToastStack", typeof(RectTransform));
        go.transform.SetParent(_canvas.transform, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = new Vector2(1f, 0f);
        rt.anchorMax = new Vector2(1f, 0f);
        rt.pivot = new Vector2(1f, 0f);
        rt.anchoredPosition = new Vector2(-30f, 30f);
        rt.sizeDelta = new Vector2(460f, 0f);
        var vlg = go.AddComponent<VerticalLayoutGroup>();
        vlg.childAlignment = TextAnchor.LowerRight;
        vlg.spacing = 10f;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        var fitter = go.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        _toastRoot = rt;
        return _toastRoot;
    }

    IEnumerator ToastRoutine(RectTransform root, string message, Sprite icon, Color tint, float seconds)
    {
        var go = new GameObject("Toast", typeof(RectTransform));
        go.transform.SetParent(root, false);
        var bg = go.AddComponent<Image>();
        bg.color = new Color(0.10f, 0.07f, 0.04f, 0.85f);
        bg.raycastTarget = false;
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 64f;
        le.preferredHeight = 64f;
        var hlg = go.AddComponent<HorizontalLayoutGroup>();
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.padding = new RectOffset(12, 16, 8, 8);
        hlg.spacing = 12f;
        hlg.childControlWidth = true;
        hlg.childControlHeight = true;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = false;
        var cg = go.AddComponent<CanvasGroup>();

        if (icon != null)
        {
            var iconGo = new GameObject("Icon", typeof(RectTransform));
            iconGo.transform.SetParent(go.transform, false);
            var iconImg = iconGo.AddComponent<Image>();
            iconImg.sprite = icon;
            iconImg.preserveAspect = true;
            iconImg.raycastTarget = false;
            var iconLe = iconGo.AddComponent<LayoutElement>();
            iconLe.minWidth = 48f; iconLe.preferredWidth = 48f;
            iconLe.minHeight = 48f; iconLe.preferredHeight = 48f;
        }

        var txtGo = new GameObject("Text", typeof(RectTransform));
        txtGo.transform.SetParent(go.transform, false);
        var txt = txtGo.AddComponent<TextMeshProUGUI>();
        txt.text = message;
        txt.fontSize = 30;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        txt.color = tint == Cream ? Cream : tint;
        txt.raycastTarget = false;
        txt.overflowMode = TextOverflowModes.Overflow;
        var txtLe = txtGo.AddComponent<LayoutElement>();
        txtLe.flexibleWidth = 1f;
        float t = 0f;
        cg.alpha = 0f;

        while (t < 0.18f)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = Mathf.Clamp01(t / 0.18f);
            yield return null;
        }

        cg.alpha = 1f;
        yield return new WaitForSecondsRealtime(seconds);
        t = 0f;

        while (t < 0.3f)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = 1f - Mathf.Clamp01(t / 0.3f);
            yield return null;
        }

        if (go != null)
            Destroy(go);
    }
}
