using UnityEngine;

[System.Serializable]
public class LootTableEntry
{
    public EquipmentData item;
    [Range(0f, 1f)]
    public float dropChance = 0.5f;
}

public class EnemyLoot : MonoBehaviour
{
    public LootTableEntry[] lootTable;

    public void DropLoot()
    {
        foreach (var entry in lootTable)
        {
            if (Random.value <= entry.dropChance)
            {
                Debug.Log($"Dropped: {entry.item.itemName}");
                InventoryManager.Instance.AddItem(entry.item, 1);
            }
        }
    }
}
