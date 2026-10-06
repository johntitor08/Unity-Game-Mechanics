using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ForegroundNotifier : MonoBehaviour
{
    public static ForegroundNotifier Instance;
    private static readonly Color MessageSurface = new(0.118f, 0.100f, 0.086f, 0.95f);
    private static readonly Color ToastSurface = new(0.200f, 0.170f, 0.146f, 0.96f);
    private const float MessageMaxTextWidth = 1400f;
    private const float MessagePadX = 56f;
    private const float MessagePadY = 28f;
    private const float ToastMaxTextWidth = 400f;
    private Canvas _canvas;
    private RectTransform _toastRoot;
    public TMP_FontAsset headerFont;
    public TMP_FontAsset bodyFont;

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
        var backdrop = go.AddComponent<Image>();
        backdrop.sprite = UISprites.Rounded(22);
        backdrop.type = Image.Type.Sliced;
        backdrop.color = MessageSurface;
        backdrop.raycastTarget = false;
        var textGo = new GameObject("Text", typeof(RectTransform));
        textGo.transform.SetParent(go.transform, false);
        var trt = (RectTransform)textGo.transform;
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = new Vector2(MessagePadX, MessagePadY);
        trt.offsetMax = new Vector2(-MessagePadX, -MessagePadY);
        var txt = textGo.AddComponent<TextMeshProUGUI>();
        txt.text = message;

        if (headerFont != null)
            txt.font = headerFont;

        txt.fontSize = 64;
        txt.alignment = TextAlignmentOptions.Center;
        txt.color = UIPalette.Gold;
        txt.raycastTarget = false;
        Vector2 pref = txt.GetPreferredValues(message, MessageMaxTextWidth, 0f);
        rt.sizeDelta = new Vector2(Mathf.Min(pref.x, MessageMaxTextWidth) + 2f * MessagePadX, pref.y + 2f * MessagePadY);
        go.transform.SetAsLastSibling();
        yield return new WaitForSeconds(seconds);

        if (go != null)
            Destroy(go);
    }

    public void ShowLoot(ItemData item, int quantity = 1)
    {
        if (item == null)
            return;

        Color c = item is EquipmentData eq ? eq.GetRarityColor() : UIPalette.Cream;
        ShowToast($"+{quantity} {item.DisplayName}", item.icon, c);
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
        vlg.childForceExpandWidth = false;
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
        bg.sprite = UISprites.Rounded(12);
        bg.type = Image.Type.Sliced;
        bg.color = ToastSurface;
        bg.raycastTarget = false;
        var le = go.AddComponent<LayoutElement>();
        le.minHeight = 64f;
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

        if (bodyFont != null)
            txt.font = bodyFont;

        txt.fontSize = 28;
        txt.alignment = TextAlignmentOptions.MidlineLeft;
        txt.color = tint;
        txt.raycastTarget = false;
        txt.overflowMode = TextOverflowModes.Overflow;
        var txtLe = txtGo.AddComponent<LayoutElement>();
        txtLe.preferredWidth = Mathf.Min(txt.GetPreferredValues(message).x, ToastMaxTextWidth);
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
