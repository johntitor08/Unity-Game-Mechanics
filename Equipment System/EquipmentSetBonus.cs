[System.Serializable]
public class EquipmentSetBonus
{
    public EquipmentSetData data;

    public int GetBonus(int pieces, StatType stat)
    {
        if (data == null || data.bonuses == null)
            return 0;

        int total = 0;

        foreach (var bonus in data.bonuses)
            if (pieces >= bonus.requiredPieces && bonus.stat == stat)
                total += bonus.value;

        return total;
    }

    public string GetDescription(int pieces)
    {
        string desc = "";

        foreach (var bonus in data.bonuses)
            if (pieces >= bonus.requiredPieces)
                desc += $"<color={UIPalette.Hex(UIPalette.Gold)}>{Loc.T($"{bonus.requiredPieces}-Piece", $"{bonus.requiredPieces} Parça")}:</color> +{bonus.value} {bonus.stat.Display()}\n";

        return desc;
    }
}
