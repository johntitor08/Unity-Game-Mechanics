using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class Typewriter : MonoBehaviour
{
    public static Typewriter Instance { get; private set; }
    private Coroutine routine;
    private bool isTyping;
    private string cachedText;
    private TextMeshProUGUI cachedTextUI;
    public event Action OnTypingComplete;
    public bool IsTyping => isTyping;
    public TextMeshProUGUI CurrentTarget => cachedTextUI;
    private AudioSource _audioSource;

    [Header("Settings")]
    public float speed = 0.03f;
    public bool skipPunctuation = true;
    public float punctuationDelay = 0.2f;

    [Header("Audio")]
    public AudioClip typeSound;
    public float typeSoundVolume = 0.3f;
    public bool playSoundPerCharacter = true;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        _audioSource = gameObject.AddComponent<AudioSource>();
        _audioSource.playOnAwake = false;
    }

    public void StartTyping(TextMeshProUGUI textUI, string text)
    {
        if (routine != null)
            StopCoroutine(routine);

        cachedText = text;
        cachedTextUI = textUI;
        routine = StartCoroutine(Type());
    }

    IEnumerator Type()
    {
        isTyping = true;
        cachedTextUI.text = "";

        foreach (char c in cachedText)
        {
            cachedTextUI.text += c;

            if (playSoundPerCharacter && typeSound != null && !char.IsWhiteSpace(c))
                PlayTypeSound();

            if (skipPunctuation && IsPunctuation(c))
                yield return new WaitForSeconds(punctuationDelay);
            else
                yield return new WaitForSeconds(speed);
        }

        isTyping = false;
        routine = null;
        OnTypingComplete?.Invoke();
    }

    public void Complete()
    {
        if (!isTyping)
            return;

        if (routine != null)
            StopCoroutine(routine);

        cachedTextUI.text = cachedText;
        isTyping = false;
        routine = null;
        OnTypingComplete?.Invoke();
    }

    bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' || c == ';' || c == ':';
    }

    void PlayTypeSound()
    {
        if (_audioSource != null && typeSound != null)
            _audioSource.PlayOneShot(typeSound, typeSoundVolume);
    }
}
