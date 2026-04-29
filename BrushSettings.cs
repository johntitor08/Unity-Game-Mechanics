using UnityEngine;

public enum ToolType
{
    Brush,
    Eraser,
    Fill,
    Shape,
    Eyedropper
}

public enum ShapeType
{
    Line,
    Rectangle,
    Circle
}

public enum BlendMode
{
    Normal,
    Multiply,
    Screen,
    Overlay
}

[CreateAssetMenu(fileName = "BrushSettings", menuName = "Drawing/BrushSettings")]
public class BrushSettings : ScriptableObject
{
    public static BrushSettings Instance { get; private set; }

    [Header("Tool")]
    public ToolType activeTool = ToolType.Brush;
    public ShapeType activeShape = ShapeType.Line;

    [Header("Brush")]
    [Range(1, 200)] public int size = 8;
    [Range(0f, 1f)] public float opacity = 1f;
    [Range(0f, 1f)] public float hardness = 1f;
    public Color color = Color.black;
    public BlendMode blendMode = BlendMode.Normal;

    [Header("Eraser")]
    [Range(1, 200)] public int eraserSize = 16;

    void OnEnable()
    {
        Instance = this;
    }

    public Color GetActiveColor()
    {
        if (activeTool == ToolType.Eraser)
            return Color.clear;
            
        var c = color;
        c.a *= opacity;
        return c;
    }
}
