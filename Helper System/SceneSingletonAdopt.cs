using System.Collections;
using System.Reflection;
using UnityEngine;

public static class SceneSingletonAdopt
{
    const BindingFlags Fields = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

    public static void Adopt(MonoBehaviour persistent, MonoBehaviour fresh)
    {
        if (persistent == null || fresh == null || ReferenceEquals(persistent, fresh))
            return;

        foreach (var f in fresh.GetType().GetFields(Fields))
        {
            if (typeof(Object).IsAssignableFrom(f.FieldType))
            {
                if (f.GetValue(fresh) is Object v && v != null)
                    f.SetValue(persistent, v);

                continue;
            }

            if (f.FieldType.IsArray && typeof(Object).IsAssignableFrom(f.FieldType.GetElementType()))
            {
                if (f.GetValue(fresh) is object arr)
                    f.SetValue(persistent, arr);

                continue;
            }

            if (f.FieldType.IsGenericType && typeof(IList).IsAssignableFrom(f.FieldType))
            {
                var args = f.FieldType.GetGenericArguments();

                if (args.Length == 1 && typeof(Object).IsAssignableFrom(args[0]) && f.GetValue(fresh) is object list)
                    f.SetValue(persistent, list);
            }
        }
    }
}
