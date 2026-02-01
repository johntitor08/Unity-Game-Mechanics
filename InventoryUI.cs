using System.Collections.Generic;
using UnityEngine;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance;
    private readonly List<ItemSlot> slots = new();

    [Header("UI")]
    public GameObject panel;
    public Transform content;
    public ItemSlot slotPrefab;

    void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
            panel.SetActive(!panel.activeSelf);
    }

    void OnEnable()
    {
        TrySubscribe();
        InventoryManager.OnReady += TrySubscribe;
    }

    void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;

        InventoryManager.OnReady -= TrySubscribe;
    }

    void TrySubscribe()
    {
        if (InventoryManager.Instance == null) return;
        InventoryManager.Instance.OnInventoryChanged -= Refresh;
        InventoryManager.Instance.OnInventoryChanged += Refresh;
        Refresh();
    }

    void Refresh()
    {
        int index = 0;

        foreach (var pair in InventoryManager.Instance.GetItems())
        {
            if (index >= slots.Count)
            {
                var s = Instantiate(slotPrefab, content);
                slots.Add(s);
            }

            slots[index].Setup(pair.Key, pair.Value);
            slots[index].gameObject.SetActive(true);
            index++;
        }

        for (int i = index; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);
    }
}
