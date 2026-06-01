using UnityEngine;

[System.Serializable]
public class ObjectAnimationBase
{
    public string objectAnimationName;
    public Texture2D[] objectAnimationTextures;

    public ObjectAnimationBase(string objectAnimationName, Texture2D[] objectAnimationTextures)
    {
        this.objectAnimationName = objectAnimationName;
        this.objectAnimationTextures = objectAnimationTextures;

    }

}