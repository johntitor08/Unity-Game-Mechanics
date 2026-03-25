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

    void Awake() => Instance = this;

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
        if (InventoryManager.Instance == null)
            return;

        InventoryManager.Instance.OnInventoryChanged -= Refresh;
        InventoryManager.Instance.OnInventoryChanged += Refresh;
        Refresh();
    }

    void Refresh()
    {
        int index = 0;

        foreach (var (inst, qty) in InventoryManager.Instance.GetEquipmentInstances())
        {
            EnsureSlot(index).Setup(inst.baseData.itemID, qty, inst.upgradeLevel);
            index++;
        }

        foreach (var (item, qty) in InventoryManager.Instance.GetNonEquipmentItems())
        {
            EnsureSlot(index).Setup(item.itemID, qty, 0);
            index++;
        }

        for (int i = index; i < slots.Count; i++)
            slots[i].gameObject.SetActive(false);
    }

    ItemSlot EnsureSlot(int index)
    {
        if (index >= slots.Count)
            slots.Add(Instantiate(slotPrefab, content));

        slots[index].gameObject.SetActive(true);
        return slots[index];
    }
}
