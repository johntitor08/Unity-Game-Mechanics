using UnityEngine;

public class NPCRoutine : MonoBehaviour
{
    public GameObject dayForm;
    public GameObject eveningForm;
    public GameObject nightForm;

    void Start()
    {
        TimePhaseManager.Instance.OnPhaseChanged += Apply;
        Apply(TimePhaseManager.Instance.currentPhase);
    }

    void Apply(TimePhase phase)
    {
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
