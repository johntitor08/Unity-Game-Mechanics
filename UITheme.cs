using UnityEngine;

[CreateAssetMenu(menuName = "UI/UI Theme", fileName = "UITheme_DarkGothic")]
public class UITheme : ScriptableObject
{
    [Header("Surfaces")]
    [Tooltip("Panel / window background fill.")]
    public Color panelBackground = new Color(0.105f, 0.103f, 0.133f, 0.97f); // #1B1A22
    [Tooltip("Panel borders / dividers / frames.")]
    public Color panelBorder = new Color(0.470f, 0.392f, 0.255f, 1f);        // muted brass

    [Header("Text")]
    public Color headerText = new Color(0.784f, 0.659f, 0.431f, 1f);          // gold #C8A96E
    public Color bodyText = new Color(0.886f, 0.835f, 0.733f, 1f);            // parchment #E2D5BB
    public Color mutedText = new Color(0.580f, 0.553f, 0.500f, 1f);

    [Header("Accent / Interactive")]
    public Color accent = new Color(0.784f, 0.659f, 0.431f, 1f);              // gold
    public Color buttonBackground = new Color(0.157f, 0.149f, 0.196f, 1f);    // #282632
    public Color buttonText = new Color(0.886f, 0.835f, 0.733f, 1f);          // parchment
    public Color highlight = new Color(0.553f, 0.376f, 0.255f, 1f);           // ember
}
