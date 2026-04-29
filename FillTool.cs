using System.Collections.Generic;
using UnityEngine;

public static class FillTool
{
    public static void FloodFill(Texture2D tex, Vector2 pos, Color fillColor)
    {
        int x = Mathf.RoundToInt(pos.x);
        int y = Mathf.RoundToInt(pos.y);
        int W = tex.width, H = tex.height;

        if (x < 0 || x >= W || y < 0 || y >= H)
            return;

        var pixels = tex.GetPixels32();
        var target = pixels[y * W + x];
        var fill = (Color32)fillColor;

        if (ColorMatch(target, fill))
            return;

        var stack = new Stack<Vector2Int>();
        stack.Push(new Vector2Int(x, y));

        while (stack.Count > 0)
        {
            var p = stack.Pop();
            int idx = p.y * W + p.x;

            if (p.x < 0 || p.x >= W || p.y < 0 || p.y >= H)
                continue;

            if (!ColorMatch(pixels[idx], target))
                continue;

            pixels[idx] = fill;
            stack.Push(new Vector2Int(p.x + 1, p.y));
            stack.Push(new Vector2Int(p.x - 1, p.y));
            stack.Push(new Vector2Int(p.x, p.y + 1));
            stack.Push(new Vector2Int(p.x, p.y - 1));
        }

        tex.SetPixels32(pixels);
    }

    static bool ColorMatch(Color32 a, Color32 b)
    {
        return Mathf.Abs(a.r - b.r) < 30 && Mathf.Abs(a.g - b.g) < 30 && Mathf.Abs(a.b - b.b) < 30 && Mathf.Abs(a.a - b.a) < 30;
    }
}
