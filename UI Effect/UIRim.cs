using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
[DisallowMultipleComponent]
[ExecuteAlways]
public class UIRim : MonoBehaviour
{
    public UITheme theme;
    public int radius = 22;
    public int width = 2;
    [Range(0f, 1f)]
    [Tooltip("Main panels read best around 0.75, cards inside them around 0.35.")]
    public float opacity = 0.75f;

    void OnEnable()
    {
        Apply();
    }

    [ContextMenu("Apply Rim")]
    public void Apply()
    {
        var image = GetComponent<Image>();
        image.sprite = UISprites.RoundedRing(radius, width);
        image.type = Image.Type.Sliced;
        image.raycastTarget = false;
        image.enabled = image.sprite != null;
        var c = theme != null ? theme.accent : new Color(0.784f, 0.659f, 0.431f);
        c.a = opacity;
        image.color = c;
    }
}
