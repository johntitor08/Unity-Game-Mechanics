public class NameValidator
{
    public static bool IsValidName(string name, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(name))
        {
            error = "�sim bo� olamaz!";
            return false;
        }

        if (name.Length < 3)
        {
            error = "�sim �ok k�sa!";
            return false;
        }

        if (name.Length > 15)
        {
            error = "�sim �ok uzun!";
            return false;
        }

        return true;
    }

    public static string SanitizeName(string name)
    {
        name = name.Trim();

        while (name.Contains("  "))
        {
            name = name.Replace("  ", " ");
        }

        if (name.Length > 0)
        {
            name = char.ToUpper(name[0]) + name[1..];
        }

        return name;
    }
}
