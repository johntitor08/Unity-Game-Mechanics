using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
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
        ButtonText
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

        var graphic = GetComponent<Graphic>();
        if (graphic != null)
            graphic.color = c;

        var tmp = GetComponent<TMP_Text>();
        if (tmp != null)
            tmp.color = c;
    }

    Color Resolve(Role r)
    {
        switch (r)
        {
            case Role.PanelBackground: return theme.panelBackground;
            case Role.PanelBorder: return theme.panelBorder;
            case Role.Header: return theme.headerText;
            case Role.Body: return theme.bodyText;
            case Role.Muted: return theme.mutedText;
            case Role.Accent: return theme.accent;
            case Role.ButtonBackground: return theme.buttonBackground;
            case Role.ButtonText: return theme.buttonText;
            default: return Color.white;
        }
    }
}
