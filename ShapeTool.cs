using UnityEngine;

public static class ShapeTool
{
    public static void DrawShape(Texture2D tex, Vector2 from, Vector2 to, ShapeType shape, Color color, int thickness)
    {
        switch (shape)
        {
            case ShapeType.Line:
                DrawLine(tex, from, to, color, thickness);
                break;

            case ShapeType.Rectangle:
                DrawRect(tex, from, to, color, thickness);
                break;

            case ShapeType.Circle:
                DrawEllipse(tex, from, to, color, thickness);
                break;
        }
    }

    static void DrawLine(Texture2D tex, Vector2 a, Vector2 b, Color color, int thick)
    {
        BrushTool.DrawLine(tex, a, b, thick, color, 1f, false);
    }

    static void DrawRect(Texture2D tex, Vector2 a, Vector2 b, Color color, int thick)
    {
        BrushTool.DrawLine(tex, new Vector2(a.x, a.y), new Vector2(b.x, a.y), thick, color, 1f, false);
        BrushTool.DrawLine(tex, new Vector2(b.x, a.y), new Vector2(b.x, b.y), thick, color, 1f, false);
        BrushTool.DrawLine(tex, new Vector2(b.x, b.y), new Vector2(a.x, b.y), thick, color, 1f, false);
        BrushTool.DrawLine(tex, new Vector2(a.x, b.y), new Vector2(a.x, a.y), thick, color, 1f, false);
    }

    static void DrawEllipse(Texture2D tex, Vector2 a, Vector2 b, Color color, int thick)
    {
        Vector2 center = (a + b) / 2f;
        float rx = Mathf.Abs(b.x - a.x) / 2f;
        float ry = Mathf.Abs(b.y - a.y) / 2f;
        int steps = Mathf.Max(64, (int)(2 * Mathf.PI * Mathf.Max(rx, ry)));
        Vector2 prev = Vector2.zero;

        for (int i = 0; i <= steps; i++)
        {
            float angle = 2 * Mathf.PI * i / steps;
            var curr = new Vector2(center.x + rx * Mathf.Cos(angle), center.y + ry * Mathf.Sin(angle));

            if (i > 0)
                BrushTool.DrawLine(tex, prev, curr, thick, color, 1f, false);

            prev = curr;
        }
    }
}
