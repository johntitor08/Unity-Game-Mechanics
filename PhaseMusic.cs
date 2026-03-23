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
        if (morning != null)
            morning.Stop();

        if (noon != null)
            noon.Stop();

        if (evening != null)
            evening.Stop();

        if (night != null)
            night.Stop();

        switch (p)
        {
            case TimePhase.Morning:

                if (morning != null)
                    morning.Play();

                break;

            case TimePhase.Noon:

                if (noon != null)
                    noon.Play();

                break;

            case TimePhase.Evening:

                if (evening != null)
                    evening.Play();

                break;

            case TimePhase.Night:

                if (night != null)
                    night.Play();

                break;
        }
    }
}
