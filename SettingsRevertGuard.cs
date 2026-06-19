using UnityEngine;

public class SettingsRevertGuard : MonoBehaviour
{
    void OnEnable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.CaptureSnapshot();
    }

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
            SettingsManager.Instance.RestoreSnapshot();
    }
}
