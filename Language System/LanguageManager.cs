using UnityEngine;

public enum GameLanguage { EN, TR }

public static class LanguageManager
{
    const string Key = "game_language";
    static GameLanguage? _current;
    public static event System.Action<GameLanguage> OnLanguageChanged;

    public static GameLanguage Current
    {
        get
        {
            _current ??= (GameLanguage)PlayerPrefs.GetInt(Key, (int)GameLanguage.EN);
            return _current.Value;
        }
    }

    public static void SetLanguage(GameLanguage lang)
    {
        _current = lang;
        PlayerPrefs.SetInt(Key, (int)lang);
        PlayerPrefs.Save();
        OnLanguageChanged?.Invoke(lang);
    }

    public static void Toggle() => SetLanguage(Current == GameLanguage.EN ? GameLanguage.TR : GameLanguage.EN);
}
