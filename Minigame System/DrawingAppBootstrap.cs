using UnityEngine;

[DefaultExecutionOrder(-100)]
public class DrawingAppBootstrap : MonoBehaviour
{
    [SerializeField] private BrushSettings brushSettings;

    void Awake()
    {
        if (brushSettings == null)
        {
            Debug.LogError("[DrawingAppBootstrap] BrushSettings asset not assigned.");
            return;
        }

        if (BrushSettings.Instance == null)
            brushSettings.GetActiveColor();
    }
}
