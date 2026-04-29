using System.Collections.Generic;
using UnityEngine;

public class LayerManager : MonoBehaviour
{
    public static LayerManager Instance { get; private set; }
    public List<Layer> Layers { get; private set; } = new List<Layer>();
    public int ActiveIndex { get; private set; } = 0;
    public Layer ActiveLayer => Layers.Count > 0 ? Layers[ActiveIndex] : null;
    private int _nextId = 0;
    public event System.Action OnLayersChanged;

    public class Layer
    {
        public string name;
        public Texture2D texture;
        public bool visible = true;
        public float opacity = 1f;
        public bool locked = false;
        public int id;

        public Layer(int id, string name, int w, int h)
        {
            this.id = id;
            this.name = name;

            texture = new Texture2D(w, h, TextureFormat.RGBA32, false)
            {
                filterMode = FilterMode.Point
            };

            Clear();
        }

        public void Clear()
        {
            var pixels = new Color32[texture.width * texture.height];
            texture.SetPixels32(pixels);
            texture.Apply();
        }

        public void RestoreFrom(Texture2D snap)
        {
            texture.SetPixels32(snap.GetPixels32());
            texture.Apply();
        }
    }

    [Header("Canvas")]
    public int canvasWidth = 1920;
    public int canvasHeight = 1080;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        AddLayer("Background");
        var bg = Layers[0].texture;
        var white = new Color32[canvasWidth * canvasHeight];
        for (int i = 0; i < white.Length; i++) white[i] = new Color32(255, 255, 255, 255);
        bg.SetPixels32(white);
        bg.Apply();
    }

    public Layer AddLayer(string name = "")
    {
        var n = string.IsNullOrEmpty(name) ? $"Layer {_nextId}" : name;
        var layer = new Layer(_nextId++, n, canvasWidth, canvasHeight);
        Layers.Add(layer);
        ActiveIndex = Layers.Count - 1;
        OnLayersChanged?.Invoke();
        return layer;
    }

    public void RemoveLayer(int index)
    {
        if (Layers.Count <= 1)
            return;

        Layers.RemoveAt(index);
        ActiveIndex = Mathf.Clamp(ActiveIndex, 0, Layers.Count - 1);
        OnLayersChanged?.Invoke();
    }

    public void SetActive(int index)
    {
        ActiveIndex = Mathf.Clamp(index, 0, Layers.Count - 1);
    }

    public void MoveLayer(int from, int to)
    {
        if (from < 0 || from >= Layers.Count || to < 0 || to >= Layers.Count)
            return;

        var layer = Layers[from];
        Layers.RemoveAt(from);
        Layers.Insert(to, layer);
        OnLayersChanged?.Invoke();
    }

    public void MergeDown(int index)
    {
        if (index <= 0 || index >= Layers.Count)
            return;

        var top = Layers[index];
        var bottom = Layers[index - 1];
        UndoRedoManager.Instance.Push(UndoRedoManager.TakeSnapshot(Layers));
        var topPixels = top.texture.GetPixels32();
        var botPixels = bottom.texture.GetPixels32();

        for (int i = 0; i < topPixels.Length; i++)
        {
            var src = (Color)topPixels[i];
            var dst = (Color)botPixels[i];
            src.a *= top.opacity;
            var blended = Color.Lerp(dst, src, src.a);
            botPixels[i] = blended;
        }

        bottom.texture.SetPixels32(botPixels);
        bottom.texture.Apply();
        RemoveLayer(index);
    }

    public Texture2D Flatten()
    {
        var result = new Texture2D(canvasWidth, canvasHeight, TextureFormat.RGBA32, false);
        var pixels = new Color32[canvasWidth * canvasHeight];

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = new Color32(255, 255, 255, 255);

        foreach (var layer in Layers)
        {
            if (!layer.visible)
                continue;

            var lp = layer.texture.GetPixels32();

            for (int i = 0; i < lp.Length; i++)
            {
                var src = (Color)lp[i];
                var dst = (Color)pixels[i];
                src.a *= layer.opacity;
                var blended = Color.Lerp(dst, src, src.a);
                pixels[i] = (Color32)blended;
            }
        }

        result.SetPixels32(pixels);
        result.Apply();
        return result;
    }
}
