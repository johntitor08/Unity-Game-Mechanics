using UnityEngine;

[System.Serializable]
public class ObjectBase
{
    public string objectName;
    public Texture2D objectTexture;

    public ObjectBase(string objectName, Texture2D objectTexture)
    {
        this.objectName = objectName;
        this.objectTexture = objectTexture;

    }

}