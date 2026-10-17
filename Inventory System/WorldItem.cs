using UnityEngine;
using UnityEngine.EventSystems;

public class WorldItem : MonoBehaviour, IPointerClickHandler
{
    public ItemData data;
    public int quantity = 1;

    [Header("Quest")]
    public string catalogObjectiveID;
    private string questObjectiveTag;
    private bool _collected;

    private string CollectedFlag => "world_item_collected:" + name;

    private bool Persistent => data != null && (data.itemType == ItemType.Quest || data.readable);

    public bool IsCollected => Persistent && StoryFlags.Has(CollectedFlag);

    void OnEnable()
    {
        if (IsCollected)
            Destroy(gameObject);
    }

    void Awake()
    {
        if (!string.IsNullOrEmpty(catalogObjectiveID) && AshenveilQuestTriggerCatalog.TryGet(catalogObjectiveID, out var entry))
            questObjectiveTag = entry.tag;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!DialogueManager.WorldClicksBlocked)
            Collect();
    }

    public void Collect()
    {
        if (_collected)
            return;

        if (data == null)
        {
            Debug.LogWarning($"[WorldItem] {name} has no ItemData assigned.");
            return;
        }

        if (InventoryManager.Instance == null)
        {
            Debug.LogWarning("[WorldItem] InventoryManager not available.");
            return;
        }

        _collected = true;
        InventoryManager.Instance.AddItem(data, quantity);

        if (Persistent)
            StoryFlags.Add(CollectedFlag);

        if (!string.IsNullOrEmpty(questObjectiveTag) && QuestManager.Instance != null)
            QuestManager.Instance.NotifyObjectInteracted(questObjectiveTag, quantity);

        if (ForegroundNotifier.Instance != null)
            ForegroundNotifier.Instance.ShowLoot(data, quantity);

        SaveSystem.SaveGame();
        Destroy(gameObject);
    }
}
