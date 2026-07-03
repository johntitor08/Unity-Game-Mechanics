using UnityEngine;

[CreateAssetMenu(fileName = "Origin", menuName = "Story/Origin")]
public class PlayerOriginData : ScriptableObject
{
    [Header("Identity")]
    public string originID;
    public string displayName;
    public string displayNameTR;
    [TextArea(2, 4)]
    public string summary;
    [TextArea(2, 4)]
    public string summaryTR;
    public Sprite icon;

    [Header("Starting Stats")]
    public int baseHP = 60;
    public int baseATK = 50;
    public int baseDEF = 40;
    public int baseMANA = 60;
    public int baseSPD = 50;

    [Header("Flags set on origin select")]
    public string[] flagsOnSelect;

    [Header("Starting Items")]
    public ItemData[] startingItems;
    public int[] startingItemQty;

    [Header("Passive Description")]
    [TextArea(1, 3)]
    public string passiveDescription;

    public string DisplayName => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(displayNameTR) ? displayNameTR : displayName;
    public string DisplaySummary => LanguageManager.Current == GameLanguage.TR && !string.IsNullOrEmpty(summaryTR) ? summaryTR : summary;

    public string GetSaveID() => originID;
}
