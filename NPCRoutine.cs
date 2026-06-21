using UnityEngine;

public class NPCRoutine : MonoBehaviour
{
    public GameObject dayForm;
    public GameObject eveningForm;
    public GameObject nightForm;

    void Start()
    {
        if (TimePhaseManager.Instance == null)
            return;

        TimePhaseManager.Instance.OnPhaseChanged += Apply;
        Apply(TimePhaseManager.Instance.currentPhase);
    }

    void OnDestroy()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= Apply;
    }

    void Apply(TimePhase phase)
    {
        if (dayForm == null || eveningForm == null || nightForm == null)
            return;

        dayForm.SetActive(false);
        eveningForm.SetActive(false);
        nightForm.SetActive(false);

        switch (phase)
        {
            case TimePhase.Morning:
            case TimePhase.Noon:
                dayForm.SetActive(true);
                break;

            case TimePhase.Evening:
                eveningForm.SetActive(true);
                break;

            case TimePhase.Night:
                nightForm.SetActive(true);
                break;
        }
    }
}
