using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [SerializeField] private string defaultFileName = "drawing";

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    public void SavePNG(string filename = "")
    {
        var flat = LayerManager.Instance.Flatten();
        var bytes = flat.EncodeToPNG();
        var name = string.IsNullOrEmpty(filename) ? defaultFileName : filename;
        var path = Path.Combine(Application.persistentDataPath, name + ".png");
        File.WriteAllBytes(path, bytes);
        Debug.Log($"PNG kaydedildi: {path}");
    }

    public void SaveNative(string filename = "")
    {
        var layers = LayerManager.Instance.Layers;
        var name = string.IsNullOrEmpty(filename) ? defaultFileName : filename;
        var dir = Path.Combine(Application.persistentDataPath, name);
        Directory.CreateDirectory(dir);

        for (int i = 0; i < layers.Count; i++)
        {
            var bytes = layers[i].texture.EncodeToPNG();
            File.WriteAllBytes(Path.Combine(dir, $"layer_{i}_{layers[i].name}.png"), bytes);
        }

        var meta = new LayerMeta { layerCount = layers.Count };
        File.WriteAllText(Path.Combine(dir, "meta.json"), JsonUtility.ToJson(meta));
        Debug.Log($"Native kayıt: {dir}");
    }

    public void LoadNative(string filename)
    {
        var dir = Path.Combine(Application.persistentDataPath, filename);

        if (!Directory.Exists(dir))
        {
            Debug.LogWarning("Kayıt bulunamadı.");
            return;
        }

        var metaPath = Path.Combine(dir, "meta.json");
        var meta = JsonUtility.FromJson<LayerMeta>(File.ReadAllText(metaPath));
        var lm = LayerManager.Instance;

        while (lm.Layers.Count > 1)
            lm.RemoveLayer(lm.Layers.Count - 1);

        for (int i = 0; i < meta.layerCount; i++)
        {
            var files = Directory.GetFiles(dir, $"layer_{i}_*.png");

            if (files.Length == 0)
                continue;

            var bytes = File.ReadAllBytes(files[0]);
            var tex = new Texture2D(2, 2);
            tex.LoadImage(bytes);
            var layer = i == 0 ? lm.Layers[0] : lm.AddLayer();
            layer.texture.SetPixels32(tex.GetPixels32());
            layer.texture.Apply();
        }

        Debug.Log("Yükleme tamamlandı.");
    }

    [System.Serializable]
    class LayerMeta
    {
        public int layerCount;
    }
}
