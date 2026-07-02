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
    public string titleTR;
    public GuideCategory category = GuideCategory.Lore;
    public Sprite icon;
    public ItemData bookItem;
    public bool unlockedByDefault = true;
    public string unlockFlag;
    [TextArea(3, 15)]
    public string body;
    [TextArea(3, 15)]
    public string bodyTR;

    public string DisplayTitle => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(titleTR) ? titleTR : title;
    public string DisplayBody => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(bodyTR) ? bodyTR : body;
}
