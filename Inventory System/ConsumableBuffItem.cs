using UnityEngine;

[CreateAssetMenu(fileName = "ConsumableBuffItem", menuName = "Inventory/Consumable Buff Item")]
public class ConsumableBuffItem : ItemData
{
    [Header("Combat Buff")]
    public PlayerBuffManager.BuffType buffType = PlayerBuffManager.BuffType.Damage;
    public float damageMultiplier = 1f;
    public float damageReduction = 0f;
    public int fights = 2;

    public void Use()
    {
        if (PlayerBuffManager.Instance == null)
            return;

        PlayerBuffManager.Instance.AddBuff(new PlayerBuffManager.Buff
        {
            id = string.IsNullOrEmpty(itemID) ? name : itemID,
            type = buffType,
            damageMultiplier = damageMultiplier,
            damageReduction = damageReduction,
            fightsRemaining = Mathf.Max(1, fights),
            displayName = itemName,
            icon = icon
        });

        Debug.Log($"Used {itemName}: {buffType} buff for {fights} fight(s).");
    }
}
