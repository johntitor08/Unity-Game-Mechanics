using System;

public interface IStatOwner
{
    event Action<StatType, int, int> OnStatChanged;
    event Action OnDeath;
    int Get(StatType type);
    void Modify(StatType type, int amount, bool save = true);
    void Set(StatType type, int value, bool save = true);
}
