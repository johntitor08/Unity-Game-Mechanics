using System.Collections;
using TMPro;
using UnityEngine;

public class Typewriter : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 0.03f;
    public bool skipPunctuation = true;
    public float punctuationDelay = 0.2f;

    [Header("Audio")]
    public AudioClip typeSound;
    public float typeSoundVolume = 0.3f;
    public bool playSoundPerCharacter = true;

    private Coroutine routine;
    private bool isTyping;

    public void StartTyping(TextMeshProUGUI textUI, string text)
    {
        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(Type(textUI, text));
    }

    IEnumerator Type(TextMeshProUGUI textUI, string text)
    {
        isTyping = true;
        textUI.text = "";

        foreach (char c in text)
        {
            textUI.text += c;

            // Play sound
            if (playSoundPerCharacter && typeSound != null && !char.IsWhiteSpace(c))
            {
                PlayTypeSound();
            }

            // Delay for punctuation
            if (skipPunctuation && IsPunctuation(c))
            {
                yield return new WaitForSeconds(punctuationDelay);
            }
            else
            {
                yield return new WaitForSeconds(speed);
            }
        }

        isTyping = false;
        routine = null;
    }

    public void Complete(TextMeshProUGUI textUI, string fullText)
    {
        if (routine != null)
            StopCoroutine(routine);

        textUI.text = fullText;
        isTyping = false;
        routine = null;
    }

    bool IsPunctuation(char c)
    {
        return c == '.' || c == ',' || c == '!' || c == '?' || c == ';' || c == ':';
    }

    void PlayTypeSound()
    {
        if (typeSound != null && Camera.main != null)
        {
            AudioSource.PlayClipAtPoint(typeSound, Camera.main.transform.position, typeSoundVolume);
        }
    }

    public bool IsTyping => isTyping;
}
