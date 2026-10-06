using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[DisallowMultipleComponent]
[ExecuteAlways]
public class UIButtonStyle : MonoBehaviour
{
    public enum Kind
    {
        Action,
        Compact,
        Icon
    }

    public UITheme theme;
    public Kind kind = Kind.Action;
    public bool primary = false;
    public bool applyOnEnable = true;
    public bool resizeToMetrics = true;
    public bool styleLabel = true;
    [Range(1, 3)]
    public int maxLines = 1;
    public bool overrideLabelColor;
    public Color labelColor = Color.white;
    public float labelFontSize = 0f;
    public bool overrideButtonColor;
    public Color buttonColor = Color.white;

    void OnEnable()
    {
        if (applyOnEnable)
            Apply();
    }

    [ContextMenu("Apply Button Style")]
    public void Apply()
    {
        if (theme == null || !TryGetComponent<Button>(out var button))
            return;

        var image = button.targetGraphic as Image;

        if (image != null)
        {
            image.sprite = UISprites.Rounded(kind == Kind.Icon ? theme.radiusSlot : theme.radiusButton);
            image.type = Image.Type.Sliced;
            image.color = overrideButtonColor ? buttonColor : primary ? theme.accent : theme.buttonSecondary;
        }

        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = new Color(1.28f, 1.28f, 1.28f, 1f);
        colors.pressedColor = new Color(0.82f, 0.82f, 0.82f, 1f);
        colors.selectedColor = Color.white;
        colors.disabledColor = new Color(1f, 1f, 1f, 0.35f);
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.08f;
        button.colors = colors;
        button.transition = Selectable.Transition.ColorTint;
        var label = button.GetComponentInChildren<TMP_Text>();

        if (label != null && styleLabel)
        {
            label.color = overrideLabelColor ? labelColor : primary ? theme.primaryButtonText : theme.buttonText;
            label.fontSize = labelFontSize > 0f ? labelFontSize : kind == Kind.Action ? theme.sizeBody : theme.sizeLabel;
        }

        if (label != null)
            FitLabel(label, maxLines);

        if (resizeToMetrics)
            Resize(button);
    }

    static void FitLabel(TMP_Text label, int maxLines)
    {
        float max = label.enableAutoSizing ? Mathf.Max(label.fontSizeMax, label.fontSize) : label.fontSize;
        label.textWrappingMode = maxLines > 1 ? TextWrappingModes.Normal : TextWrappingModes.NoWrap;
        label.enableAutoSizing = true;
        label.fontSizeMax = max;
        label.fontSizeMin = Mathf.Min(maxLines > 1 ? 20f : 14f, max);
        var rt = label.rectTransform;
        bool fillsButton = rt.anchorMin == Vector2.zero && rt.anchorMax == Vector2.one && rt.offsetMin == Vector2.zero && rt.offsetMax == Vector2.zero;

        if (fillsButton && label.margin == Vector4.zero && rt.rect.width >= 100f)
            label.margin = new Vector4(12f, 2f, 12f, 2f);
    }

    void Resize(Button button)
    {
        var rt = button.transform as RectTransform;

        if (rt == null)
            return;

        var size = rt.sizeDelta;

        switch (kind)
        {
            case Kind.Action:
                size.y = theme.buttonHeight;
                size.x = Mathf.Max(size.x, theme.buttonMinWidth);
                break;

            case Kind.Compact:
                size.y = theme.buttonHeightCompact;
                size.x = Mathf.Max(size.x, theme.buttonMinWidth);
                break;

            case Kind.Icon:
                size.x = theme.iconButtonSize;
                size.y = theme.iconButtonSize;
                break;
        }

        rt.sizeDelta = size;
    }
}
