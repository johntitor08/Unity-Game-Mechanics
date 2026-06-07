using System.Collections.Generic;

public static class StoryFlags
{
    private static readonly HashSet<string> flags = new();
    public static event System.Action<string> OnFlagAdded;

    public static void Add(string flag)
    {
        if (flag != null && flags.Add(flag))
            OnFlagAdded?.Invoke(flag);
    }

    public static bool Has(string flag) => flags.Contains(flag);

    public static void Load(IEnumerable<string> savedFlags)
    {
        flags.Clear();

        if (savedFlags != null)
            flags.UnionWith(savedFlags);

        QuestFlags.MigrateLegacyOriginStartFlags();
    }

    public static void Reset() => flags.Clear();

    public static IReadOnlyCollection<string> GetAll() => flags;
}
