using UnityEngine;

public abstract class HotkeyPanelUI : MonoBehaviour
{
    protected static bool PanelInputBlocked()
    {
        return GameMenuManager.Instance != null && GameMenuManager.Instance.IsPaused();
    }
}
