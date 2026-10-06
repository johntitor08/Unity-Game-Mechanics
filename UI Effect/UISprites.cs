using System;
using System.Collections.Generic;
using UnityEngine;

public static class UISprites
{
    private static readonly Dictionary<string, Sprite> Cache = new();

    public static Sprite Rounded(int radius)
    {
        return Get("round" + radius, () =>
        {
            const int S = 96;
            int r = Mathf.Clamp(radius, 1, S / 2);
            var tex = NewTex(S, S);
            var px = new Color32[S * S];

            for (int y = 0; y < S; y++)
            {
                for (int x = 0; x < S; x++)
                {
                    float dx = Mathf.Max(r - (x + 0.5f), (x + 0.5f) - (S - r));
                    float dy = Mathf.Max(r - (y + 0.5f), (y + 0.5f) - (S - r));
                    float a = 1f;

                    if (dx > 0f && dy > 0f)
                    {
                        float d = Mathf.Sqrt(dx * dx + dy * dy);
                        a = Mathf.Clamp01(r - d + 0.5f);
                    }

                    px[y * S + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels32(px);
            tex.Apply();

            return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        });
    }

    public static Sprite RoundedRing(int radius, int width)
    {
        return Get("ring" + radius + "_" + width, () =>
        {
            const int S = 96;
            int r = Mathf.Clamp(radius, 1, S / 2);
            var tex = NewTex(S, S);
            var px = new Color32[S * S];

            for (int y = 0; y < S; y++)
            {
                for (int x = 0; x < S; x++)
                {
                    float dx = Mathf.Max(r - (x + 0.5f), (x + 0.5f) - (S - r));
                    float dy = Mathf.Max(r - (y + 0.5f), (y + 0.5f) - (S - r));
                    float inside = (dx > 0f && dy > 0f) ? r - Mathf.Sqrt(dx * dx + dy * dy) : Mathf.Min(Mathf.Min(x + 0.5f, S - x - 0.5f), Mathf.Min(y + 0.5f, S - y - 0.5f));
                    float a = Mathf.Clamp01(inside + 0.5f) * Mathf.Clamp01(width - inside + 0.5f);
                    px[y * S + x] = new Color(1f, 1f, 1f, a);
                }
            }

            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect, new Vector4(r, r, r, r));
        });
    }

    public static Sprite Circle()
    {
        return Get("circle", () =>
        {
            const int S = 256;
            var tex = NewTex(S, S);
            var px = new Color32[S * S];
            float c = S * 0.5f;

            for (int y = 0; y < S; y++)
            {
                for (int x = 0; x < S; x++)
                {
                    float d = Mathf.Sqrt((x + 0.5f - c) * (x + 0.5f - c) + (y + 0.5f - c) * (y + 0.5f - c));
                    px[y * S + x] = new Color(1f, 1f, 1f, Mathf.Clamp01(c - d));
                }
            }

            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, S, S), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        });
    }

    public static Sprite Solid()
    {
        return Get("solid", () =>
        {
            var tex = NewTex(4, 4);
            var px = new Color32[16];

            for (int i = 0; i < 16; i++)
                px[i] = Color.white;

            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        });
    }

    public static Sprite BottomFade()
    {
        return Get("bfade", () =>
        {
            const int H = 128;
            var tex = NewTex(4, H);
            var px = new Color32[4 * H];

            for (int y = 0; y < H; y++)
            {
                float a = 1f - Mathf.Clamp01(y / (float)(H - 1));
                a = a * a;

                for (int x = 0; x < 4; x++)
                    px[y * 4 + x] = new Color(1f, 1f, 1f, a);
            }

            tex.SetPixels32(px);
            tex.Apply();
            return Sprite.Create(tex, new Rect(0, 0, 4, H), new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
        });
    }

    private static Texture2D NewTex(int w, int h)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false)
        {
            wrapMode = TextureWrapMode.Clamp,
            filterMode = FilterMode.Bilinear,
            hideFlags = HideFlags.HideAndDontSave
        };

        return tex;
    }

    private static Sprite Get(string key, Func<Sprite> make)
    {
        if (Cache.TryGetValue(key, out var cached) && cached != null)
            return cached;

        var sprite = make();
        sprite.hideFlags = HideFlags.HideAndDontSave;
        Cache[key] = sprite;
        return sprite;
    }
}
