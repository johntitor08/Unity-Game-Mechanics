using UnityEngine;

public class SceneMusic : MonoBehaviour
{
    public AudioClip music;
    public AudioClip ambience;
    public bool playOnEnable = false;

    void Start() => Apply();

    void OnEnable()
    {
        if (playOnEnable)
            Apply();
    }

    public void Apply()
    {
        if (GameAudioManager.Instance == null)
            return;

        if (music != null)
            GameAudioManager.Instance.PlayMusic(music);

        if (ambience != null)
            GameAudioManager.Instance.PlayAmbience(ambience);
    }
}
