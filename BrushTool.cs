using UnityEngine;

public static class BrushTool
{
    public static void DrawLine(Texture2D tex, Vector2 from, Vector2 to, int size, Color color, float hardness, bool eraser)
    {
        float dist = Vector2.Distance(from, to);
        int steps = Mathf.Max(1, Mathf.CeilToInt(dist * 2f));

        for (int i = 0; i <= steps; i++)
        {
            float t = (float)i / steps;
            PaintCircle(tex, Vector2.Lerp(from, to, t), size, color, hardness, eraser);
        }
    }

    static void PaintCircle(Texture2D tex, Vector2 center, int radius, Color color, float hardness, bool eraser)
    {
        int cx = Mathf.RoundToInt(center.x);
        int cy = Mathf.RoundToInt(center.y);
        int W = tex.width, H = tex.height;

        for (int x = cx - radius; x <= cx + radius; x++)
        {
            for (int y = cy - radius; y <= cy + radius; y++)
            {
                if (x < 0 || x >= W || y < 0 || y >= H)
                continue;

                float dx = x - cx, dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > radius)
                    continue;

                float alpha = hardness >= 1f ? 1f : Mathf.Clamp01(1f - (dist / radius - hardness) / (1f - hardness + 0.001f));

                if (eraser)
                {
                    var existing = tex.GetPixel(x, y);
                    existing.a = Mathf.Max(0, existing.a - alpha);
                    tex.SetPixel(x, y, existing);
                }
                else
                {
                    var existing = tex.GetPixel(x, y);
                    var blended = Color.Lerp(existing, color, color.a * alpha);
                    blended.a = Mathf.Max(existing.a, color.a * alpha);
                    tex.SetPixel(x, y, blended);
                }
            }
        }
    }
}
