using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIPanelManager : MonoBehaviour
{
    public static UIPanelManager Instance { get; private set; }
    public List<PanelEntry> panels = new();
    public bool enforceMutualExclusion = true;

    [System.Serializable]
    public class PanelEntry
    {
        public GameObject root;
        public GameObject[] companions;
        public GameObject firstSelected;
        [System.NonSerialized] public bool wasActive;
        [System.NonSerialized] public UIPanelAnimator animator;
        [System.NonSerialized] public bool animatorResolved;
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (panels != null)
        {
            foreach (var entry in panels)
                if (entry != null && entry.root != null)
                    entry.wasActive = entry.root.activeSelf;
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    void Update()
    {
        if (panels == null)
            return;

        for (int i = 0; i < panels.Count; i++)
        {
            var entry = panels[i];

            if (entry == null || entry.root == null)
                continue;

            bool isActive = entry.root.activeSelf;

            if (enforceMutualExclusion && isActive && !entry.wasActive)
            {
                CloseOthers(entry);
                FocusEntry(entry);
            }

            entry.wasActive = isActive;
        }
    }

    public void OpenPanel(PanelEntry entry)
    {
        if (entry == null)
            return;

        SetEntryActive(entry, true);

        if (enforceMutualExclusion)
            CloseOthers(entry);

        FocusEntry(entry);
    }

    public bool CloseOpenPanels()
    {
        if (panels == null)
            return false;

        bool closedAny = false;

        foreach (var entry in panels)
        {
            if (entry == null || entry.root == null)
                continue;

            if (entry.root.activeSelf)
            {
                SetEntryActive(entry, false);
                closedAny = true;
            }
        }

        return closedAny;
    }

    public bool AnyPanelOpen()
    {
        if (panels == null)
            return false;

        foreach (var entry in panels)
            if (entry != null && entry.root != null && entry.root.activeSelf)
                return true;

        return false;
    }

    void CloseOthers(PanelEntry keep)
    {
        foreach (var entry in panels)
        {
            if (entry == null || entry == keep || entry.root == null)
                continue;

            if (entry.root.activeSelf)
                SetEntryActive(entry, false);
        }
    }

    void SetEntryActive(PanelEntry entry, bool active)
    {
        if (active)
        {
            if (entry.root != null && !entry.root.activeSelf)
                entry.root.SetActive(true);

            SetCompanions(entry, true);
            entry.wasActive = true;
            return;
        }

        var animator = ResolveAnimator(entry);

        if (animator != null && entry.root != null && entry.root.activeInHierarchy)
        {
            var root = entry.root;
            var captured = entry;

            animator.PlayClose(() =>
            {
                if (root != null)
                    root.SetActive(false);

                SetCompanions(captured, false);
            });

            return;
        }

        if (entry.root != null && entry.root.activeSelf)
            entry.root.SetActive(false);

        SetCompanions(entry, false);
        entry.wasActive = false;
    }

    void SetCompanions(PanelEntry entry, bool active)
    {
        if (entry.companions == null)
            return;

        foreach (var companion in entry.companions)
            if (companion != null && companion.activeSelf != active)
                companion.SetActive(active);
    }

    UIPanelAnimator ResolveAnimator(PanelEntry entry)
    {
        if (!entry.animatorResolved || entry.animator == null)
        {
            entry.animator = entry.root != null ? entry.root.GetComponent<UIPanelAnimator>() : null;
            entry.animatorResolved = entry.animator != null;
        }

        return entry.animator;
    }

    void FocusEntry(PanelEntry entry)
    {
        if (entry.firstSelected == null || EventSystem.current == null)
            return;

        EventSystem.current.SetSelectedGameObject(null);
        EventSystem.current.SetSelectedGameObject(entry.firstSelected);
    }
}
