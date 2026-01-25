using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "EquipmentSetData", menuName = "Equipment/EquipmentSetData")]
public class EquipmentSetData : ScriptableObject
{
    public int setID;
    public string setName;
    public int totalPieces = 4;
    public List<EquipmentSetStatBonus> bonuses;
}

[System.Serializable]
public class EquipmentSetStatBonus
{
    public int requiredPieces;
    public StatType stat;
    public int value;
}
