using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu]
public class ObjectAnimationDatabase : ScriptableObject
{
    public List<ObjectAnimationBase> objects;

    public int ObjectCount { get { return objects.Count; } }

    public ObjectAnimationBase GetObject(int index)
    {
        return objects[index];

    }

}