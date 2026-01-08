using UnityEngine;

public class PhaseDebug : MonoBehaviour
{
    public void Morning() => TimePhaseManager.Instance.SetPhase(TimePhase.Morning);
    public void Noon() => TimePhaseManager.Instance.SetPhase(TimePhase.Noon);
    public void Evening() => TimePhaseManager.Instance.SetPhase(TimePhase.Evening);
    public void Night() => TimePhaseManager.Instance.SetPhase(TimePhase.Night);
}
