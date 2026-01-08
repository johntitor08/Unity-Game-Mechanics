using UnityEngine;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance;

    public GameObject shopPanel;
    public Transform shopContent;
    public ShopSlot shopSlotPrefab;

    private System.Collections.Generic.List<ShopSlot> slots = new();

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        ProfileManager.Instance.OnCurrencyChanged += RefreshShop;
        RefreshShop();
    }

    public void OpenShop()
    {
        shopPanel.SetActive(true);
        RefreshShop();
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    public void RefreshShop()
    {
        int index = 0;

        foreach (var shopItem in ShopManager.Instance.shopItems)
        {
            if (index >= slots.Count)
            {
                var slot = Instantiate(shopSlotPrefab, shopContent);
                slots.Add(slot);
            }

            slots[index].Setup(shopItem);
            slots[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);
    }
}
