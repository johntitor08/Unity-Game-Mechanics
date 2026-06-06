using UnityEngine;

[CreateAssetMenu(menuName = "UI/UI Theme", fileName = "UITheme_DarkGothic")]
public class UITheme : ScriptableObject
{
    [Header("Surfaces")]
    public Color panelBackground = new(0.105f, 0.103f, 0.133f, 0.97f);
    public Color panelBorder = new(0.470f, 0.392f, 0.255f, 1f);

    [Header("Text")]
    public Color headerText = new(0.784f, 0.659f, 0.431f, 1f);
    public Color bodyText = new(0.886f, 0.835f, 0.733f, 1f);
    public Color mutedText = new(0.580f, 0.553f, 0.500f, 1f);

    [Header("Accent / Interactive")]
    public Color accent = new(0.784f, 0.659f, 0.431f, 1f);
    public Color buttonBackground = new(0.157f, 0.149f, 0.196f, 1f);
    public Color buttonText = new(0.886f, 0.835f, 0.733f, 1f);
    public Color highlight = new(0.553f, 0.376f, 0.255f, 1f);
}
