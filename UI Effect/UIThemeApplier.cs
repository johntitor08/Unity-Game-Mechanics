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

        if (TryGetComponent<Graphic>(out var graphic))
            graphic.color = c;

        if (TryGetComponent<TMP_Text>(out var tmp))
            tmp.color = c;
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
            _ => Color.white,
        };
    }
}
