using UnityEngine;

public class CombatMusicHook : MonoBehaviour
{
    public AudioClip combatMusic;
    bool _hooked;

    void Update()
    {
        if (_hooked || CombatManager.Instance == null)
            return;

        CombatManager.Instance.OnCombatStarted += OnStart;
        CombatManager.Instance.OnCombatEnded += OnEnd;
        _hooked = true;
    }

    void OnStart()
    {
        if (GameAudioManager.Instance != null && combatMusic != null)
            GameAudioManager.Instance.PlayMusic(combatMusic);
    }

    void OnEnd()
    {
        var am = GameAudioManager.Instance;

        if (am == null)
            return;

        AudioClip resume = am.currentAmbientMusic != null ? am.currentAmbientMusic : am.defaultMusic;

        if (resume != null)
            am.PlayMusic(resume);
    }

    void OnDestroy()
    {
        if (CombatManager.Instance == null)
            return;

        CombatManager.Instance.OnCombatStarted -= OnStart;
        CombatManager.Instance.OnCombatEnded -= OnEnd;
    }
}
