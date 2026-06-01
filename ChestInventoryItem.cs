using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChestInventoryItem : MonoBehaviour
{
    private InventorySystem inventorySystem;
    public RawImage itemImage;
    public TMP_Text itemName;
    public TMP_Text itemQuantity;

    private void Start()
    {
        inventorySystem = GameObject.Find("GameManager").GetComponent<InventorySystem>();
    }

    public void SetItem(Texture2D image, string name, int quantity)
    {
        itemImage.texture = image;
        itemName.text = name;
        itemQuantity.text = quantity.ToString();
    }

    public void GetItem()
    {
        if (int.TryParse(itemQuantity.text, out int quantity))
        {
            inventorySystem.AddItem((Texture2D)itemImage.texture, itemName.text, quantity);
            Destroy(gameObject);
        }
        else
        {
            Debug.LogError("Invalid item quantity: " + itemQuantity.text);
        }
    }
}
