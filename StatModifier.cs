using System.Collections.Generic;

public static class StoryFlags
{
    private static readonly HashSet<string> flags = new();

    public static void Add(string flag) => flags.Add(flag);

    public static bool Has(string flag) => flags.Contains(flag);

    public static void Load(IEnumerable<string> savedFlags)
    {
        flags.Clear();

        if (savedFlags != null)
            flags.UnionWith(savedFlags);
    }

    public static void Reset() => flags.Clear();

    public static IReadOnlyCollection<string> GetAll() => flags;
}
