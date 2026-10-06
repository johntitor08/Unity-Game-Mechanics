public static class DisplayNames
{
    public static string Display(this Rarity rarity) => rarity switch
    {
        Rarity.Common => Loc.T("Common", "Sıradan"),
        Rarity.Rare => Loc.T("Rare", "Nadir"),
        Rarity.Epic => Loc.T("Epic", "Destansı"),
        Rarity.Legendary => Loc.T("Legendary", "Efsanevi"),
        Rarity.Godly => Loc.T("Godly", "İlahi"),
        _ => rarity.ToString()
    };

    public static string Display(this StatType stat) => stat switch
    {
        StatType.Health => Loc.T("Health", "Can"),
        StatType.MaxHealth => Loc.T("Max Health", "Maks. Can"),
        StatType.Energy => Loc.T("Energy", "Enerji"),
        StatType.MaxEnergy => Loc.T("Max Energy", "Maks. Enerji"),
        StatType.Charisma => Loc.T("Charisma", "Karizma"),
        StatType.Strength => Loc.T("Strength", "Güç"),
        StatType.Intelligence => Loc.T("Intelligence", "Zekâ"),
        StatType.Damage => Loc.T("Damage", "Hasar"),
        StatType.Defense => Loc.T("Defense", "Savunma"),
        StatType.Speed => Loc.T("Speed", "Hız"),
        StatType.Luck => Loc.T("Luck", "Şans"),
        _ => stat.ToString()
    };

    public static string Display(this EquipmentSlot slot) => slot switch
    {
        EquipmentSlot.Weapon => Loc.T("Weapon", "Silah"),
        EquipmentSlot.Armor => Loc.T("Armor", "Zırh"),
        EquipmentSlot.Helmet => Loc.T("Helmet", "Kask"),
        EquipmentSlot.Accessory => Loc.T("Accessory", "Aksesuar"),
        EquipmentSlot.Shield => Loc.T("Shield", "Kalkan"),
        EquipmentSlot.Boots => Loc.T("Boots", "Bot"),
        _ => slot.ToString()
    };
}
