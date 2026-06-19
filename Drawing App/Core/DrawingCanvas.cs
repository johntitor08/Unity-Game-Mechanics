using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RawImage))]
[RequireComponent(typeof(RectTransform))]
public class DrawingCanvas : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerMoveHandler
{
    public static DrawingCanvas Instance { get; private set; }
    private RawImage _rawImage;
    private RectTransform _rt;
    private bool _drawing = false;
    private Vector2 _lastPos;
    private bool _strokeStarted = false;
    private Vector2 _shapeStart;
    private Texture2D _shapePreviewSnap;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
        
        _rawImage = GetComponent<RawImage>();
        _rt = GetComponent<RectTransform>();
    }

    void Start()
    {
        RefreshDisplay();
        LayerManager.Instance.OnLayersChanged += RefreshDisplay;
    }

    void RefreshDisplay()
    {
        _rawImage.texture = LayerManager.Instance.Flatten();
    }

    public void OnPointerDown(PointerEventData e)
    {
        var pos = GetCanvasPos(e);

        if (pos == null)
            return;

        var tool = BrushSettings.Instance.activeTool;

        if (tool == ToolType.Fill)
        {
            UndoRedoManager.Instance.Push(UndoRedoManager.TakeSnapshot(LayerManager.Instance.Layers));
            FillTool.FloodFill(LayerManager.Instance.ActiveLayer.texture, pos.Value, BrushSettings.Instance.color);
            LayerManager.Instance.ActiveLayer.texture.Apply();
            var audioFill = AudioManager.Instance;
            if (audioFill != null) audioFill.PlayFill();
            RefreshDisplay();
            return;
        }

        if (tool == ToolType.Eyedropper)
        {
            EyedropperTool.Sample(LayerManager.Instance.ActiveLayer.texture, pos.Value);
            return;
        }

        if (tool == ToolType.Shape)
        {
            _shapeStart = pos.Value;
            var t = LayerManager.Instance.ActiveLayer.texture;
            _shapePreviewSnap = new Texture2D(t.width, t.height, TextureFormat.RGBA32, false);
            _shapePreviewSnap.SetPixels32(t.GetPixels32());
            _shapePreviewSnap.Apply();
            UndoRedoManager.Instance.Push(UndoRedoManager.TakeSnapshot(LayerManager.Instance.Layers));
            return;
        }

        _drawing = true;
        _lastPos = pos.Value;
        _strokeStarted = false;
    }

    public void OnDrag(PointerEventData e)
    {
        var pos = GetCanvasPos(e);

        if (pos == null)
            return;

        var tool = BrushSettings.Instance.activeTool;

        if (tool == ToolType.Shape && _shapePreviewSnap != null)
        {
            var t = LayerManager.Instance.ActiveLayer.texture;
            t.SetPixels32(_shapePreviewSnap.GetPixels32());
            ShapeTool.DrawShape(t, _shapeStart, pos.Value, BrushSettings.Instance.activeShape, BrushSettings.Instance.color, BrushSettings.Instance.size);
            t.Apply();
            RefreshDisplay();
            return;
        }

        if (!_drawing)
            return;

        if (!_strokeStarted)
        {
            UndoRedoManager.Instance.Push(UndoRedoManager.TakeSnapshot(LayerManager.Instance.Layers));
            _strokeStarted = true;
            var audio = AudioManager.Instance;

            if (tool == ToolType.Eraser)
            {
                if (audio != null)
                    audio.PlayEraser();
            }
            else
            {
                if (audio != null)
                    audio.PlayBrush();
            }
        }

        BrushTool.DrawLine(LayerManager.Instance.ActiveLayer.texture, _lastPos, pos.Value, BrushSettings.Instance.size, BrushSettings.Instance.GetActiveColor(), BrushSettings.Instance.hardness, BrushSettings.Instance.activeTool == ToolType.Eraser);
        LayerManager.Instance.ActiveLayer.texture.Apply();
        _lastPos = pos.Value;
        RefreshDisplay();
    }

    public void OnPointerMove(PointerEventData e) { }

    public void OnPointerUp(PointerEventData e)
    {
        _drawing = false;
        _strokeStarted = false;
        _shapePreviewSnap = null;
    }

    Vector2? GetCanvasPos(PointerEventData e)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(_rt, e.position, e.pressEventCamera, out var local))
            return null;

        var rect = _rt.rect;
        var u = (local.x - rect.x) / rect.width;
        var v = (local.y - rect.y) / rect.height;

        if (u < 0 || u > 1 || v < 0 || v > 1)
            return null;

        var lm = LayerManager.Instance;
        return new Vector2(u * lm.canvasWidth, v * lm.canvasHeight);
    }

    void Update()
    {
        bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.LeftCommand);

        if (Input.GetKeyDown(KeyCode.Z) && ctrl)
            Undo();

        if (Input.GetKeyDown(KeyCode.Y) && ctrl)
            Redo();

        if (Input.GetKeyDown(KeyCode.E))
            BrushSettings.Instance.activeTool = ToolType.Eraser;

        if (Input.GetKeyDown(KeyCode.B))
            BrushSettings.Instance.activeTool = ToolType.Brush;

        if (Input.GetKeyDown(KeyCode.F))
            BrushSettings.Instance.activeTool = ToolType.Fill;

        if (Input.GetKeyDown(KeyCode.I))
            BrushSettings.Instance.activeTool = ToolType.Eyedropper;
    }

    public void Undo()
    {
        var state = UndoRedoManager.Instance.Undo();
        
        if (state == null)
            return;
        
        RestoreState(state);
        var audio = AudioManager.Instance;

        if (audio != null)
            audio.PlayUndo();
    }

    public void Redo()
    {
        var state = UndoRedoManager.Instance.Redo();
        
        if (state == null)
            return;
        
        RestoreState(state);
    }

    void RestoreState(Texture2D[] snaps)
    {
        var layers = LayerManager.Instance.Layers;
        
        for (int i = 0; i < Mathf.Min(snaps.Length, layers.Count); i++)
            layers[i].RestoreFrom(snaps[i]);
            
        RefreshDisplay();
    }
}
