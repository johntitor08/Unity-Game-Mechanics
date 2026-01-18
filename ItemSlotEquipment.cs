using UnityEngine;

public class ItemSlotEquipment : MonoBehaviour
{
    private readonly ItemData item;

    public void OnEquipButtonClicked()
    {
        if (item is EquipmentData equipment)
        {
            if (EquipmentManager.Instance.CanEquip(equipment))
            {
                // Remove from inventory
                InventoryManager.Instance.RemoveItem(equipment, 1);

                // Equip
                EquipmentManager.Instance.Equip(equipment);
            }
            else
            {
                Debug.Log("Cannot equip this item!");
            }
        }
    }
}
