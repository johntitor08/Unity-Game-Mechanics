using UnityEngine;

public enum GuideCategory
{
    Book, Character, Lore
}

[CreateAssetMenu(fileName = "GuideEntry", menuName = "Guide/Guide Entry")]
public class GuideEntry : ScriptableObject
{
    public string id;
    public string title;
    public GuideCategory category = GuideCategory.Lore;
    public Sprite icon;
    public ItemData bookItem;
    public bool unlockedByDefault = true;
    public string unlockFlag;

    [TextArea(3, 15)]
    public string body;
}
