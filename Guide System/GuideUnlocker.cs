using UnityEngine;

public class GuideUnlocker : MonoBehaviour
{
    bool _invHooked, _dlgHooked, _flagHooked;

    void OnEnable()
    {
        StoryFlags.OnFlagAdded -= OnFlag;
        StoryFlags.OnFlagAdded += OnFlag;
        _flagHooked = true;
    }

    void Start()
    {
        TryHook();
        CheckLoreAgainstExistingFlags();
    }

    void Update()
    {
        if (!_invHooked || !_dlgHooked)
            TryHook();
    }

    void TryHook()
    {
        if (!_invHooked && InventoryManager.Instance != null)
        {
            InventoryManager.Instance.OnInventoryChanged -= CheckBooks;
            InventoryManager.Instance.OnInventoryChanged += CheckBooks;
            _invHooked = true;
            CheckBooks();
        }

        if (!_dlgHooked && DialogueManager.Instance != null)
        {
            DialogueManager.Instance.OnDialogueStart -= CheckSpeaker;
            DialogueManager.Instance.OnDialogueStart += CheckSpeaker;
            _dlgHooked = true;
        }
    }

    void OnDestroy()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= CheckBooks;

        if (DialogueManager.Instance != null)
            DialogueManager.Instance.OnDialogueStart -= CheckSpeaker;

        StoryFlags.OnFlagAdded -= OnFlag;
    }

    void CheckBooks()
    {
        if (GuideManager.Instance == null || InventoryManager.Instance == null)
            return;

        foreach (var e in GuideManager.Instance.entries)
        {
            if (e == null || e.category != GuideCategory.Book || e.bookItem == null)
                continue;

            if (InventoryManager.Instance.GetQuantity(e.bookItem) > 0)
            {
                GuideManager.Instance.Unlock(e.id);
                StoryFlags.Add("read_" + e.id);
            }
        }
    }

    void CheckSpeaker(DialogueNode node)
    {
        if (GuideManager.Instance == null || node == null || string.IsNullOrEmpty(node.speakerName))
            return;

        foreach (var e in GuideManager.Instance.entries)
        {
            if (e == null || e.category != GuideCategory.Character)
                continue;

            if (string.Equals(e.title, node.speakerName, System.StringComparison.OrdinalIgnoreCase))
                GuideManager.Instance.Unlock(e.id);
        }
    }

    void OnFlag(string flag)
    {
        if (GuideManager.Instance == null || string.IsNullOrEmpty(flag))
            return;

        foreach (var e in GuideManager.Instance.entries)
            if (e != null && !string.IsNullOrEmpty(e.unlockFlag) && e.unlockFlag == flag)
                GuideManager.Instance.Unlock(e.id);
    }

    void CheckLoreAgainstExistingFlags()
    {
        if (GuideManager.Instance == null)
            return;

        foreach (var e in GuideManager.Instance.entries)
            if (e != null && !string.IsNullOrEmpty(e.unlockFlag) && StoryFlags.Has(e.unlockFlag))
                GuideManager.Instance.Unlock(e.id);
    }
}
