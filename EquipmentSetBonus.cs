using UnityEngine;

[System.Serializable]
public class EquipmentSetBonus
{
    public int setID;
    public string setName;
    public int totalPieces = 4;

    [Header("Set Bonuses")]
    public int twoPieceDefenseBonus = 5;
    public int threePieceDamageBonus = 10;
    public int fourPieceStatBonus = 15;
    public StatType fourPieceStat = StatType.Strength;

    public int GetBonusForPieces(int pieces)
    {
        // For defense calculation
        if (pieces >= 2)
            return twoPieceDefenseBonus;
        return 0;
    }

    public string GetActiveBonusDescription(int pieces)
    {
        string desc = "";

        if (pieces >= 2)
            desc += $"2-Piece: +{twoPieceDefenseBonus} Defense\n";
        if (pieces >= 3)
            desc += $"3-Piece: +{threePieceDamageBonus} Damage\n";
        if (pieces >= 4)
            desc += $"4-Piece: +{fourPieceStatBonus} {fourPieceStat}\n";

        return desc;
    }
}
