using System.Collections.Generic;
using UnityEngine;

public class AshenveilQuestMapRegions : MonoBehaviour
{
    private readonly List<SpawnedRegion> spawned = new();
    private Transform regionRoot;
    private bool subscribed;

    [Header("Prefab")]
    public GameObject hoverRegionPrefab;

    [Header("Parent (optional)")]
    public RectTransform regionParentOverride;

    struct SpawnedRegion
    {
        public string questID;
        public string objectiveID;
        public GameObject instance;
    }

    void OnEnable()
    {
        QuestManager.OnReady += TryInitialize;
        TryInitialize();
    }

    void OnDisable()
    {
        QuestManager.OnReady -= TryInitialize;
        UnsubscribeQuestEvents();
    }

    void TryInitialize()
    {
        if (QuestManager.Instance == null || hoverRegionPrefab == null)
            return;

        EnsureRegionRoot();

        if (spawned.Count == 0)
            SpawnAllFromCatalog();

        SubscribeQuestEvents();
        RefreshAllVisibility();
    }

    public void EnsureRegionRoot()
    {
        if (regionRoot != null)
            return;

        if (regionParentOverride != null)
        {
            regionRoot = regionParentOverride;
            return;
        }

        var map = FindMapRect();

        if (map == null)
        {
            Debug.LogWarning("[AshenveilQuestMapRegions] Map RectTransform bulunamadı.");
            return;
        }

        var existing = map.Find("AshenveilQuestRegions");

        if (existing != null)
        {
            regionRoot = existing;
            return;
        }

        var go = new GameObject("AshenveilQuestRegions", typeof(RectTransform));
        var rt = (RectTransform)go.transform;
        rt.SetParent(map, false);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = Vector2.zero;
        rt.localScale = Vector3.one;
        regionRoot = rt;
    }

    void SpawnAllFromCatalog()
    {
        if (regionRoot == null)
            return;

        foreach (var entry in AshenveilQuestTriggerCatalog.All)
        {
            if (entry.component != AshenveilQuestTriggerCatalog.RecommendedComponent.UIHoverBridge || IsAlreadySpawned(entry.objectiveID))
                continue;

            SpawnRegion(entry);
        }
    }

    bool IsAlreadySpawned(string objectiveID)
    {
        foreach (var s in spawned)
            if (s.objectiveID == objectiveID)
                return true;

        return false;
    }

    void SpawnRegion(AshenveilQuestTriggerCatalog.Entry entry)
    {
        var instance = Instantiate(hoverRegionPrefab, regionRoot);
        instance.name = entry.suggestedSceneObjectName;

        if (!instance.TryGetComponent<QuestUIHoverBridge>(out var bridge))
            bridge = instance.AddComponent<QuestUIHoverBridge>();

        bridge.catalogObjectiveID = entry.objectiveID;
        bridge.requireQuestActive = true;
        bridge.ApplyCatalogEntry();
        instance.SetActive(false);

        spawned.Add(new SpawnedRegion
        {
            questID = entry.questID,
            objectiveID = entry.objectiveID,
            instance = instance
        });
    }

    void SubscribeQuestEvents()
    {
        if (subscribed || QuestManager.Instance == null)
            return;

        var qm = QuestManager.Instance;
        qm.OnQuestStarted += OnQuestChanged;
        qm.OnQuestCompleted += OnQuestChanged;
        qm.OnQuestAbandoned += OnQuestChanged;
        qm.OnObjectiveUpdated += OnObjectiveChanged;
        qm.OnObjectiveCompleted += OnObjectiveChanged;
        subscribed = true;
    }

    void UnsubscribeQuestEvents()
    {
        if (!subscribed || QuestManager.Instance == null)
            return;

        var qm = QuestManager.Instance;
        qm.OnQuestStarted -= OnQuestChanged;
        qm.OnQuestCompleted -= OnQuestChanged;
        qm.OnQuestAbandoned -= OnQuestChanged;
        qm.OnObjectiveUpdated -= OnObjectiveChanged;
        qm.OnObjectiveCompleted -= OnObjectiveChanged;
        subscribed = false;
    }

    void OnQuestChanged(QuestData _) => RefreshAllVisibility();

    void OnObjectiveChanged(QuestData _, QuestObjective __) => RefreshAllVisibility();

    void RefreshAllVisibility()
    {
        if (QuestManager.Instance == null)
            return;

        foreach (var entry in spawned)
        {
            if (entry.instance == null)
                continue;

            bool shouldShow = ShouldShowRegion(entry.questID, entry.objectiveID);

            if (entry.instance.activeSelf != shouldShow)
                entry.instance.SetActive(shouldShow);
        }
    }

    static bool ShouldShowRegion(string questID, string objectiveID)
    {
        var qm = QuestManager.Instance;

        if (!qm.IsQuestActive(questID))
            return false;

        return !qm.GetObjectiveState(questID, objectiveID).isCompleted;
    }

    public static RectTransform FindMapRect()
    {
        if (SceneEvent.Instance != null && SceneEvent.Instance.mapPanel != null)
        {
            var map = SceneEvent.Instance.mapPanel.transform.Find("Map");

            if (map is RectTransform mapRt)
                return mapRt;
        }

        var mapCanvas = GameObject.Find("MapCanvas");

        if (mapCanvas != null)
        {
            var map = mapCanvas.transform.Find("MapPanel/Map");

            if (map is RectTransform mapRt)
                return mapRt;
        }

        return null;
    }
}
