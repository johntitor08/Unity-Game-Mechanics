using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu]
public class ObjectDatabase : ScriptableObject
{
    public List<ObjectBase> objects;
    private Dictionary<string, ObjectBase> objectDictionary;

    public int ObjectCount { get { return objects.Count; } }

    public ObjectBase GetObject(int index)
    {
        return objects[index];

    }

    public ObjectBase GetObjectByName(string objectName)
    {
        if (objectDictionary == null)
        {
            objectDictionary = new Dictionary<string, ObjectBase>();

            foreach (var obj in objects)
            {
                objectDictionary.Add(obj.objectName, obj);

            }

        }

        if (objectDictionary.TryGetValue(objectName, out ObjectBase result))
        {
            return result;

        }

        Debug.LogWarning("ObjectDatabase: object not found: " + objectName);
        return null;

    }

}
