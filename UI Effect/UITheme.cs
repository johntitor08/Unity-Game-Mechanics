using UnityEngine;

[CreateAssetMenu(menuName = "UI/UI Theme", fileName = "UITheme_DarkGothic")]
public class UITheme : ScriptableObject
{
    [Header("Surfaces")]
    public Color panelBackground = new(0.105f, 0.103f, 0.133f, 0.97f);
    public Color panelBorder = new(0.470f, 0.392f, 0.255f, 1f);
    public Color panelLift = new(0.188f, 0.180f, 0.243f, 0.96f);

    [Header("Text")]
    public Color headerText = new(0.784f, 0.659f, 0.431f, 1f);
    public Color bodyText = new(0.886f, 0.835f, 0.733f, 1f);
    public Color mutedText = new(0.580f, 0.553f, 0.500f, 1f);

    [Header("Accent / Interactive")]
    public Color accent = new(0.784f, 0.659f, 0.431f, 1f);
    public Color buttonBackground = new(0.157f, 0.149f, 0.196f, 1f);
    public Color buttonText = new(0.886f, 0.835f, 0.733f, 1f);
    public Color highlight = new(0.553f, 0.376f, 0.255f, 1f);
    public Color primaryButtonText = new(0.934f, 0.897f, 0.824f, 1f);
    public Color buttonSecondary = new(0.263f, 0.251f, 0.341f, 1f);

    [Header("Structure")]
    public Color dialogueSurface = new(0.105f, 0.103f, 0.133f, 0.95f);
    public Color hairline = new(0.784f, 0.659f, 0.431f, 0.20f);
    public Color shade = new(0f, 0f, 0f, 0.72f);

    [Header("Type scale (1920x1080 reference)")]
    public float sizeTitle = 64f;
    public float sizeHeader = 40f;
    public float sizeBody = 32f;
    public float sizeLabel = 28f;
    public float sizeSmall = 24f;

    [Header("Corner radii")]
    public int radiusPanel = 22;
    public int radiusButton = 18;
    public int radiusSlot = 12;
    public int radiusBar = 12;

    [Header("Metrics")]
    public float buttonHeight = 52f;
    public float buttonHeightCompact = 40f;
    public float buttonMinWidth = 180f;
    public float iconButtonSize = 44f;
    public float headerHeight = 64f;

    public float NearestSize(float current)
    {
        float[] steps = { sizeSmall, sizeLabel, sizeBody, sizeHeader, sizeTitle };
        float best = steps[0];
        float bestDelta = Mathf.Abs(current - best);

        for (int i = 1; i < steps.Length; i++)
        {
            float delta = Mathf.Abs(current - steps[i]);

            if (delta < bestDelta)
            {
                bestDelta = delta;
                best = steps[i];
            }
        }

        return best;
    }
}
