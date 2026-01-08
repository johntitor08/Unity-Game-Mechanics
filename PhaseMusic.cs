using UnityEngine;

public class PhaseMusic : MonoBehaviour
{
    public AudioSource morning;
    public AudioSource noon;
    public AudioSource evening;
    public AudioSource night;

    void Start()
    {
        TimePhaseManager.Instance.OnPhaseChanged += Play;
        Play(TimePhaseManager.Instance.currentPhase);
    }

    void Play(TimePhase p)
    {
        morning.Stop(); noon.Stop(); evening.Stop(); night.Stop();

        switch (p)
        {
            case TimePhase.Morning: morning.Play(); break;
            case TimePhase.Noon: noon.Play(); break;
            case TimePhase.Evening: evening.Play(); break;
            case TimePhase.Night: night.Play(); break;
        }
    }
}
