using UnityEngine;
using TMPro;

[RequireComponent(typeof(TMP_Text))]
[DisallowMultipleComponent]
public class LocalizedText : MonoBehaviour
{
    [TextArea] public string en;
    [TextArea] public string tr;
    private TMP_Text _text;

    void Awake() => _text = GetComponent<TMP_Text>();

    void OnEnable()
    {
        LanguageManager.OnLanguageChanged += OnLanguageChanged;
        Apply();
    }

    void OnDisable() => LanguageManager.OnLanguageChanged -= OnLanguageChanged;

    void OnLanguageChanged(GameLanguage lang) => Apply();

    public void Apply()
    {
        if (_text == null)
            _text = GetComponent<TMP_Text>();

        string value = LanguageManager.Current == GameLanguage.TR ? tr : en;

        if (!string.IsNullOrEmpty(value))
            _text.text = value;
    }
}
