using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class GameAudioManager : MonoBehaviour
{
    public static GameAudioManager Instance { get; private set; }
    const string PP_MASTER = "audio_master", PP_MUSIC = "audio_music", PP_AMB = "audio_amb", PP_SFX = "audio_sfx";
    Coroutine _musicFade;

    [Header("Sources")]
    [SerializeField] private AudioSource musicSource;
    [SerializeField] private AudioSource ambienceSource;
    [SerializeField] private AudioSource sfxSource;

    [Header("Default clips")]
    public AudioClip defaultMusic;
    public AudioClip defaultAmbience;
    public AudioClip uiClick;
    public AudioClip uiBack;
    public AudioClip pageTurn;

    [Header("Volumes")]
    [Range(0f, 1f)] public float masterVolume = 1f;
    [Range(0f, 1f)] public float musicVolume = 0.7f;
    [Range(0f, 1f)] public float ambienceVolume = 0.6f;
    [Range(0f, 1f)] public float sfxVolume = 1f;

    [Header("Fade")]
    public float crossfadeDuration = 1.2f;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            SceneSingletonAdopt.Adopt(Instance, this);
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureSources();
        LoadVolumes();
        ApplyVolumes();
    }

    void Start()
    {
        if (defaultMusic != null)
            PlayMusic(defaultMusic);

        if (defaultAmbience != null)
            PlayAmbience(defaultAmbience);
    }

    void EnsureSources()
    {
        if (musicSource == null)
            musicSource = CreateSource("Music", true);

        if (ambienceSource == null)
            ambienceSource = CreateSource("Ambience", true);

        if (sfxSource == null)
            sfxSource = CreateSource("SFX", false);
    }

    AudioSource CreateSource(string n, bool loop)
    {
        var go = new GameObject(n);
        go.transform.SetParent(transform, false);
        var s = go.AddComponent<AudioSource>();
        s.loop = loop;
        s.playOnAwake = false;
        return s;
    }

    public void PlayMusic(AudioClip clip, bool restartIfSame = false)
    {
        if (clip == null || (!restartIfSame && musicSource.clip == clip && musicSource.isPlaying))
            return;

        if (_musicFade != null)
            StopCoroutine(_musicFade);

        _musicFade = StartCoroutine(CrossfadeMusic(clip));
    }

    IEnumerator CrossfadeMusic(AudioClip clip)
    {
        float dur = Mathf.Max(0.01f, crossfadeDuration);
        float t = 0f, startVol = musicSource.volume;

        if (musicSource.isPlaying)
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                musicSource.volume = Mathf.Lerp(startVol, 0f, t / dur);
                yield return null;
            }

        musicSource.clip = clip;
        musicSource.Play();
        float target = musicVolume * masterVolume;
        t = 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(0f, target, t / dur);
            yield return null;
        }

        musicSource.volume = target;
        _musicFade = null;
    }

    public void StopMusic()
    {
        if (_musicFade != null)
            StopCoroutine(_musicFade);

        musicSource.Stop();
    }

    public void PlayAmbience(AudioClip clip)
    {
        if (clip == null || (ambienceSource.clip == clip && ambienceSource.isPlaying))
            return;

        ambienceSource.clip = clip;
        ambienceSource.volume = ambienceVolume * masterVolume;
        ambienceSource.Play();
    }

    public void StopAmbience() => ambienceSource.Stop();

    public void PlaySfx(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null)
            return;

        sfxSource.PlayOneShot(clip, Mathf.Clamp01(sfxVolume * masterVolume) * volumeScale);
    }

    public void PlayUiClick() => PlaySfx(uiClick);

    public void PlayUiBack() => PlaySfx(uiBack);

    public void PlayPageTurn() => PlaySfx(pageTurn);

    public void SetMasterVolume(float v)
    {
        masterVolume = Mathf.Clamp01(v);
        ApplyVolumes();
        Save();
    }

    public void SetMusicVolume(float v)
    {
        musicVolume = Mathf.Clamp01(v);
        ApplyVolumes();
        Save();
    }

    public void SetAmbienceVolume(float v)
    {
        ambienceVolume = Mathf.Clamp01(v);
        ApplyVolumes();
        Save();
    }

    public void SetSfxVolume(float v)
    {
        sfxVolume = Mathf.Clamp01(v);
        Save();
    }

    void ApplyVolumes()
    {
        if (musicSource != null && _musicFade == null)
            musicSource.volume = musicVolume * masterVolume;

        if (ambienceSource != null)
            ambienceSource.volume = ambienceVolume * masterVolume;
    }

    void LoadVolumes()
    {
        masterVolume = PlayerPrefs.GetFloat(PP_MASTER, masterVolume);
        musicVolume = PlayerPrefs.GetFloat(PP_MUSIC, musicVolume);
        ambienceVolume = PlayerPrefs.GetFloat(PP_AMB, ambienceVolume);
        sfxVolume = PlayerPrefs.GetFloat(PP_SFX, sfxVolume);
    }

    void Save()
    {
        PlayerPrefs.SetFloat(PP_MASTER, masterVolume);
        PlayerPrefs.SetFloat(PP_MUSIC, musicVolume);
        PlayerPrefs.SetFloat(PP_AMB, ambienceVolume);
        PlayerPrefs.SetFloat(PP_SFX, sfxVolume);
        PlayerPrefs.Save();
    }
}
