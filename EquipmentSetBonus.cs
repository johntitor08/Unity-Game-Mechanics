[System.Serializable]
public class EquipmentSetBonus
{
    public EquipmentSetData data;

    public void Apply(int pieces, bool apply)
    {
        if (PlayerStats.Instance == null || data == null) return;

        int mult = apply ? 1 : -1;

        foreach (var bonus in data.bonuses)
        {
            if (pieces >= bonus.requiredPieces)
            {
                PlayerStats.Instance.Modify(
                    bonus.stat,
                    bonus.value * mult,
                    false
                );
            }
        }
    }

    public string GetDescription(int pieces)
    {
        string desc = "";

        foreach (var bonus in data.bonuses)
        {
            if (pieces >= bonus.requiredPieces)
            {
                desc += $"<color=#FFD966>{bonus.requiredPieces}-Piece:</color> " +
                        $"+{bonus.value} {bonus.stat}\n";
            }
        }

        return desc;
    }
}
