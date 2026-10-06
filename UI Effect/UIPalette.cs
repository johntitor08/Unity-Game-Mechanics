using UnityEngine;

public static class UIPalette
{
    public static readonly Color Gold = new(0.784f, 0.659f, 0.431f);
    public static readonly Color Cream = new(0.886f, 0.835f, 0.733f);
    public static readonly Color Muted = new(0.659f, 0.604f, 0.525f);
    public static readonly Color Light = new(0.934f, 0.897f, 0.824f);
    public static readonly Color Good = new(0.561f, 0.780f, 0.478f);
    public static readonly Color Bad = new(0.851f, 0.467f, 0.420f);
    public static readonly Color Common = new(0.741f, 0.714f, 0.659f);
    public static readonly Color Rare = new(0.420f, 0.616f, 0.878f);
    public static readonly Color Epic = new(0.698f, 0.463f, 0.859f);
    public static readonly Color Legendary = new(0.933f, 0.647f, 0.275f);
    public static readonly Color Godly = new(0.863f, 0.310f, 0.271f);

    public static Color ForRarity(Rarity rarity)
    {
        return rarity switch
        {
            Rarity.Common => Common,
            Rarity.Rare => Rare,
            Rarity.Epic => Epic,
            Rarity.Legendary => Legendary,
            Rarity.Godly => Godly,
            _ => Cream
        };
    }

    public static string Hex(Color color) => "#" + ColorUtility.ToHtmlStringRGB(color);
}
