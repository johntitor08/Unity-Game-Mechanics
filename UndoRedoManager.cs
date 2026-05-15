using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    private AudioSource _src;

    [Header("Clips")]
    public AudioClip brushStroke;
    public AudioClip eraserStroke;
    public AudioClip fillSound;
    public AudioClip undoSound;
    public AudioClip saveSound;

    [Header("Settings")]
    [Range(0f, 1f)] public float masterVolume = 0.6f;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }

        _src = gameObject.AddComponent<AudioSource>();
        _src.volume = masterVolume;
    }

    public void Play(AudioClip clip)
    {
        if (clip == null || _src == null)
            return;

        _src.PlayOneShot(clip, masterVolume);
    }

    public void PlayBrush() => Play(brushStroke);

    public void PlayEraser() => Play(eraserStroke);

    public void PlayFill() => Play(fillSound);

    public void PlayUndo() => Play(undoSound);

    public void PlaySave() => Play(saveSound);
}
