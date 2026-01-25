using System.Collections.Generic;

public static class StoryFlags
{
    public static HashSet<string> flags = new();

    public static void Add(string flag) => flags.Add(flag);

    public static bool Has(string flag) => flags.Contains(flag);
}
