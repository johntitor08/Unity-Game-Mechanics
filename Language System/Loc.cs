public static class Loc
{
    public static string T(string en, string tr) => LanguageManager.Current == GameLanguage.TR ? tr : en;
}
