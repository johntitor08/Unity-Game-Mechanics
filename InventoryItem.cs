using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryItem : MonoBehaviour
{
    private MPPGameManager gameManager;
    private InventorySystem inventorySystem;
    public RawImage itemImage;
    public TMP_Text itemName;
    public TMP_Text itemQuantity;

    private void Start()
    {
        gameManager = GameObject.Find("GameManager").GetComponent<MPPGameManager>();
        inventorySystem = GameObject.Find("GameManager").GetComponent<InventorySystem>();
    }

    public void SetItem(Texture2D image, string name, int quantity)
    {
        itemImage.texture = image;
        itemName.text = name;
        itemQuantity.text = quantity.ToString();
    }

    public void UseItem()
    {
        gameManager.nameOfInventoryItemToUse = itemName.text;
        inventorySystem.RemoveItem(this);
    }
}
