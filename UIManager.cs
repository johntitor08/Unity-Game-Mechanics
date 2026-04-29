using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIManager : MonoBehaviour
{
    [Header("Tool Buttons")]
    public Button brushBtn;
    public Button eraserBtn;
    public Button fillBtn;
    public Button eyedropperBtn;

    [Header("Shape Buttons")]
    public Button lineBtn;
    public Button rectBtn;
    public Button circleBtn;

    [Header("Brush Controls")]
    public Slider sizeSlider;
    public Slider hardnessSlider;
    public Slider opacitySlider;
    public TextMeshProUGUI sizeLabel;

    [Header("Color")]
    public Image colorPreview;
    public Button[] paletteButtons;

    [Header("Layer")]
    public Button addLayerBtn;
    public Button removeLayerBtn;
    public Button mergeDownBtn;

    [Header("File")]
    public Button savePNGBtn;
    public Button saveNativeBtn;
    public Button undoBtn;
    public Button redoBtn;
    public Button clearBtn;

    void Start()
    {
        var bs = BrushSettings.Instance;

        if (brushBtn != null)
            brushBtn.onClick.AddListener(() => SetTool(ToolType.Brush));

        if (eraserBtn != null)
            eraserBtn.onClick.AddListener(() => SetTool(ToolType.Eraser));

        if (fillBtn != null)
            fillBtn.onClick.AddListener(() => SetTool(ToolType.Fill));

        if (eyedropperBtn != null)
            eyedropperBtn.onClick.AddListener(() => SetTool(ToolType.Eyedropper));

        if (lineBtn != null)
            lineBtn.onClick.AddListener(() =>
            {
                SetTool(ToolType.Shape);
                bs.activeShape = ShapeType.Line;
            });

        if (rectBtn != null)
            rectBtn.onClick.AddListener(() =>
            {
                SetTool(ToolType.Shape);
                bs.activeShape = ShapeType.Rectangle;
            });

        if (circleBtn != null)
            circleBtn.onClick.AddListener(() =>
            {
                SetTool(ToolType.Shape);
                bs.activeShape = ShapeType.Circle;
            });

        if (sizeSlider != null)
        {
            sizeSlider.onValueChanged.AddListener(v =>
            {
                bs.size = (int)v;
                if (sizeLabel != null) sizeLabel.text = $"{(int)v}px";
            });
        }

        if (hardnessSlider != null)
            hardnessSlider.onValueChanged.AddListener(v => bs.hardness = v);

        if (opacitySlider != null)
            opacitySlider.onValueChanged.AddListener(v => bs.opacity = v);

        foreach (var btn in paletteButtons)
        {
            if (btn == null)
                continue;

            var img = btn.GetComponent<Image>();
            var col = img != null ? img.color : Color.white;

            btn.onClick.AddListener(() =>
            {
                bs.color = col;

                if (colorPreview != null)
                    colorPreview.color = col;

                SetTool(ToolType.Brush);
            });
        }

        if (addLayerBtn != null)
            addLayerBtn.onClick.AddListener(() => LayerManager.Instance.AddLayer());

        if (removeLayerBtn != null)
            removeLayerBtn.onClick.AddListener(() => LayerManager.Instance.RemoveLayer(LayerManager.Instance.ActiveIndex));

        if (mergeDownBtn != null)
            mergeDownBtn.onClick.AddListener(() => LayerManager.Instance.MergeDown(LayerManager.Instance.ActiveIndex));

        if (savePNGBtn != null)
            savePNGBtn.onClick.AddListener(() =>
            {
                SaveManager.Instance.SavePNG();
                var am = AudioManager.Instance;

                if (am != null)
                    am.PlaySave();
            });

        if (saveNativeBtn != null) saveNativeBtn.onClick.AddListener(() =>
        {
            SaveManager.Instance.SaveNative();
            var am = AudioManager.Instance;

            if (am != null)
                am.PlaySave();
        });

        if (undoBtn != null)
            undoBtn.onClick.AddListener(() => DrawingCanvas.Instance.Undo());

        if (redoBtn != null)
            redoBtn.onClick.AddListener(() => DrawingCanvas.Instance.Redo());

        if (clearBtn != null)
            clearBtn.onClick.AddListener(() =>
            {
                var layer = LayerManager.Instance.ActiveLayer;
                layer?.Clear();
            });
    }

    static void SetTool(ToolType t) => BrushSettings.Instance.activeTool = t;
}
