using UnityEngine;
using UnityEngine.UI;

public class PhaseVisuals : MonoBehaviour
{
    public Image overlay;

    public Color morning = new Color(1f, 0.85f, 0.7f, 0.15f);
    public Color noon = new Color(1f, 1f, 1f, 0f);
    public Color evening = new Color(1f, 0.6f, 0.4f, 0.25f);
    public Color night = new Color(0.2f, 0.3f, 0.6f, 0.45f);

    public float fadeSpeed = 2f;
    Color target;

    void Start()
    {
        TimePhaseManager.Instance.OnPhaseChanged += ApplyPhase;
        ApplyPhase(TimePhaseManager.Instance.currentPhase);
    }

    void ApplyPhase(TimePhase phase)
    {
        target = phase switch
        {
            TimePhase.Morning => morning,
            TimePhase.Noon => noon,
            TimePhase.Evening => evening,
            TimePhase.Night => night,
            _ => noon
        };
    }

    void Update()
    {
        overlay.color = Color.Lerp(overlay.color, target, Time.deltaTime * fadeSpeed);
    }
}
