using UnityEngine;
using UnityEngine.UI;

public class ColorWheelUI : MonoBehaviour
{
    [Header("Preview")]
    public Image colorPreview;

    [Header("HSV Sliders")]
    public Slider hueSlider;
    public Slider satSlider;
    public Slider valSlider;

    [Header("Hex Input")]
    public TMPro.TMP_InputField hexInput;

    void Start()
    {
        if (hueSlider != null)
            hueSlider.onValueChanged.AddListener(_ => OnHSVChanged());

        if (satSlider != null)
            satSlider.onValueChanged.AddListener(_ => OnHSVChanged());

        if (valSlider != null)
            valSlider.onValueChanged.AddListener(_ => OnHSVChanged());

        if (hexInput != null)
            hexInput.onEndEdit.AddListener(OnHexInput);

        SyncFromBrush();
    }

    void OnHSVChanged()
    {
        float h = hueSlider != null ? hueSlider.value : 0f;
        float s = satSlider != null ? satSlider.value : 1f;
        float v = valSlider != null ? valSlider.value : 1f;
        var color = Color.HSVToRGB(h, s, v);
        BrushSettings.Instance.color = color;

        if (colorPreview != null)
            colorPreview.color = color;

        if (hexInput != null)
            hexInput.text = ColorUtility.ToHtmlStringRGB(color);
    }

    void OnHexInput(string hex)
    {
        if (ColorUtility.TryParseHtmlString("#" + hex, out var color))
        {
            BrushSettings.Instance.color = color;
            if (colorPreview != null) colorPreview.color = color;
            Color.RGBToHSV(color, out float h, out float s, out float v);

            if (hueSlider != null)
                hueSlider.value = h;

            if (satSlider != null)
                satSlider.value = s;

            if (valSlider != null)
                valSlider.value = v;
        }
    }

    void SyncFromBrush()
    {
        var c = BrushSettings.Instance.color;
        Color.RGBToHSV(c, out float h, out float s, out float v);

        if (hueSlider != null)
            hueSlider.value = h;

        if (satSlider != null)
            satSlider.value = s;

        if (valSlider != null)
            valSlider.value = v;

        if (colorPreview != null)
            colorPreview.color = c;
    }
}
