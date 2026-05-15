using UnityEngine;

public static class EyedropperTool
{
    public static void Sample(Texture2D tex, Vector2 pos)
    {
        int x = Mathf.Clamp(Mathf.RoundToInt(pos.x), 0, tex.width - 1);
        int y = Mathf.Clamp(Mathf.RoundToInt(pos.y), 0, tex.height - 1);
        var sampled = tex.GetPixel(x, y);
        sampled.a = 1f;
        BrushSettings.Instance.color = sampled;
        BrushSettings.Instance.activeTool = ToolType.Brush;
        Debug.Log($"Renk örneklendi: {sampled}");
    }
}
