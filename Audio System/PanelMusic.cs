using UnityEngine;

public class PanelMusic : MonoBehaviour
{
    public AudioClip music;

    void OnEnable()
    {
        if (GameAudioManager.Instance != null && music != null)
            GameAudioManager.Instance.PlayMusic(music);
    }

    void OnDisable()
    {
        var am = GameAudioManager.Instance;

        if (am != null && am.defaultMusic != null)
            am.PlayMusic(am.defaultMusic);
    }
}
