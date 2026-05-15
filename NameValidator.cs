public class NameValidator
{
    public static bool IsValidName(string name, out string error)
    {
        error = "";

        if (string.IsNullOrWhiteSpace(name))
        {
            error = "Ýsim boþ olamaz!";
            return false;
        }

        if (name.Length < 3)
        {
            error = "Ýsim çok kýsa!";
            return false;
        }

        if (name.Length > 15)
        {
            error = "Ýsim çok uzun!";
            return false;
        }

        return true;
    }

    public static string SanitizeName(string name)
    {
        // Remove extra spaces
        name = name.Trim();

        // Replace multiple spaces with single space
        while (name.Contains("  "))
        {
            name = name.Replace("  ", " ");
        }

        // Capitalize first letter
        if (name.Length > 0)
        {
            name = char.ToUpper(name[0]) + name[1..];
        }

        return name;
    }
}
