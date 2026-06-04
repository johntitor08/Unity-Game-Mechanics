using UnityEngine;

[CreateAssetMenu(fileName = "Origin", menuName = "Story/Origin")]
public class PlayerOriginData : ScriptableObject
{
    [Header("Identity")]
    public string originID;
    public string displayName;
    [TextArea(2, 4)]
    public string summary;
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

    public string GetSaveID() => originID;
}
