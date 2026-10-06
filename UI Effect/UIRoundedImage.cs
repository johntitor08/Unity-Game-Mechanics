using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
[ExecuteAlways]
public class UIRoundedImage : MonoBehaviour
{
    public int radius = 6;

    void OnEnable()
    {
        Apply();
    }

    [ContextMenu("Apply Rounding")]
    public void Apply()
    {
        var image = GetComponent<Image>();
        image.sprite = UISprites.Rounded(radius);
        image.type = Image.Type.Sliced;
    }
}
