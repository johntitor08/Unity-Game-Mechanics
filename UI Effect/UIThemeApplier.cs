using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
[ExecuteAlways]
public class UIThemeApplier : MonoBehaviour
{
    public enum Role
    {
        PanelBackground,
        PanelBorder,
        Header,
        Body,
        Muted,
        Accent,
        ButtonBackground,
        ButtonText,
        Surface,
        SurfaceLift,
        Slot,
        Hairline,
        Scrim,
        DialogueSurface
    }

    public UITheme theme;
    public Role role = Role.PanelBackground;
    public bool applyOnEnable = true;

    void OnEnable()
    {
        if (applyOnEnable)
            Apply();
    }

    [ContextMenu("Apply Theme")]
    public void Apply()
    {
        if (theme == null)
            return;

        Color c = Resolve(role);

        if (TryGetComponent<Graphic>(out var graphic))
            graphic.color = c;

        if (TryGetComponent<TMP_Text>(out var tmp))
            tmp.color = c;

        ApplySurfaceSprite();
    }

    void ApplySurfaceSprite()
    {
        if (!TryGetComponent<Image>(out var image) || image.type == Image.Type.Filled)
            return;

        switch (role)
        {
            case Role.Surface:
            case Role.SurfaceLift:
            case Role.DialogueSurface:
                image.sprite = UISprites.Rounded(theme.radiusPanel);
                image.type = Image.Type.Sliced;
                break;

            case Role.Slot:
                image.sprite = UISprites.Rounded(theme.radiusSlot);
                image.type = Image.Type.Sliced;
                break;

            case Role.Hairline:
            case Role.Scrim:
                image.sprite = UISprites.Solid();
                image.type = Image.Type.Simple;
                break;
        }
    }

    Color Resolve(Role r)
    {
        return r switch
        {
            Role.PanelBackground => theme.panelBackground,
            Role.PanelBorder => theme.panelBorder,
            Role.Header => theme.headerText,
            Role.Body => theme.bodyText,
            Role.Muted => theme.mutedText,
            Role.Accent => theme.accent,
            Role.ButtonBackground => theme.buttonBackground,
            Role.ButtonText => theme.buttonText,
            Role.Surface => theme.panelBackground,
            Role.SurfaceLift => theme.panelLift,
            Role.DialogueSurface => theme.dialogueSurface,
            Role.Slot => theme.buttonBackground,
            Role.Hairline => theme.hairline,
            Role.Scrim => theme.shade,
            _ => Color.white,
        };
    }
}
