using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LayerPanelUI : MonoBehaviour
{
    [Header("References")]
    public Transform listParent;
    public GameObject layerItemPrefab;

    void Start()
    {
        LayerManager.Instance.OnLayersChanged += Refresh;
        Refresh();
    }

    void Refresh()
    {
        foreach (Transform child in listParent)
            Destroy(child.gameObject);

        var layers = LayerManager.Instance.Layers;

        for (int i = layers.Count - 1; i >= 0; i--)
        {
            int idx = i;
            var layer = layers[i];
            var item = Instantiate(layerItemPrefab, listParent);
            var label = item.GetComponentInChildren<TextMeshProUGUI>();

            if (label != null)
                label.text = layer.name;

            var toggle = item.GetComponentInChildren<Toggle>();

            if (toggle != null)
            {
                toggle.isOn = layer.visible;

                toggle.onValueChanged.AddListener(v =>
                {
                    layer.visible = v;
                    DrawingCanvas.Instance.SendMessage("RefreshDisplay");
                });
            }

            
            if (item.TryGetComponent<Button>(out var btn))
                btn.onClick.AddListener(() => LayerManager.Instance.SetActive(idx));

            var sliders = item.GetComponentsInChildren<Slider>();

            foreach (var s in sliders)
            {
                if (s.name.ToLower().Contains("opacity"))
                {
                    s.value = layer.opacity;
                    
                    s.onValueChanged.AddListener(v =>
                    {
                        layer.opacity = v;
                        DrawingCanvas.Instance.SendMessage("RefreshDisplay");
                    });
                }
            }
        }
    }
}
