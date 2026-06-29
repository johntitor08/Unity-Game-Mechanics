using UnityEngine;

public class PhaseMusicSwitcher : MonoBehaviour
{
    public AudioClip eveningMusic;
    public AudioClip nightMusic;
    bool _hooked;

    void OnEnable() => TryHook();

    void Start()
    {
        TryHook();
        Apply(CurrentPhase());
    }

    void OnDisable()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.OnPhaseChanged -= Apply;

        _hooked = false;
    }

    void TryHook()
    {
        if (_hooked || TimePhaseManager.Instance == null)
            return;

        TimePhaseManager.Instance.OnPhaseChanged += Apply;
        _hooked = true;
    }

    TimePhase CurrentPhase() => TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;

    void Apply(TimePhase phase)
    {
        var am = GameAudioManager.Instance;

        if (am == null)
            return;

        AudioClip target = phase == TimePhase.Night && nightMusic != null ? nightMusic : phase == TimePhase.Evening && eveningMusic != null ? eveningMusic : am.defaultMusic;
        am.currentAmbientMusic = target;
        bool inCombat = CombatManager.Instance != null && CombatManager.Instance.inCombat;

        if (!inCombat)
            am.PlayMusic(target);
    }
}
