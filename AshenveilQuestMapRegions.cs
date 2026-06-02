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

    [Header("Q01 · Bir Fincan Huzur")]
    public bool spawnQ01OnStart = true;

    public Q01RegionPlacement[] q01Placements =
    {
        new("Ashenveil_Kitchen_Stove", "q01_obj3", new Vector2(420f, -180f), QuestUIHoverBridge.TriggerKind.Interact),
        new("Ashenveil_Maren_TeaHandIn", "q01_obj4", new Vector2(-120f, 280f), QuestUIHoverBridge.TriggerKind.Talk)
    };

    [System.Serializable]
    public struct Q01RegionPlacement
    {
        public string objectName;
        public string catalogObjectiveID;
        public Vector2 anchoredPosition;
        public QuestUIHoverBridge.TriggerKind kind;

        public Q01RegionPlacement(string objectName, string catalogObjectiveID, Vector2 anchoredPosition, QuestUIHoverBridge.TriggerKind kind)
        {
            this.objectName = objectName;
            this.catalogObjectiveID = catalogObjectiveID;
            this.anchoredPosition = anchoredPosition;
            this.kind = kind;
        }
    }

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

        if (spawnQ01OnStart && spawned.Count == 0)
            SpawnQ01Regions();

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
            Debug.LogWarning("[AshenveilQuestMapRegions] Map RectTransform bulunamadı. MapPanel altında 'Map' objesi veya regionParentOverride atayın.");
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

    public void SpawnQ01Regions()
    {
        if (hoverRegionPrefab == null)
            return;

        EnsureRegionRoot();

        if (regionRoot == null)
            return;

        foreach (var placement in q01Placements)
            SpawnRegion(AshenveilQuestIds.Q01, placement);
    }

    void SpawnRegion(string questID, Q01RegionPlacement placement)
    {
        foreach (var s in spawned)
        {
            if (s.instance != null && s.instance.name == placement.objectName)
                return;
        }

        var instance = Instantiate(hoverRegionPrefab, regionRoot);
        instance.name = placement.objectName;

        if (instance.TryGetComponent(out RectTransform rt))
            rt.anchoredPosition = placement.anchoredPosition;
        
        if (!instance.TryGetComponent<QuestUIHoverBridge>(out var bridge))
            bridge = instance.AddComponent<QuestUIHoverBridge>();

        bridge.catalogObjectiveID = placement.catalogObjectiveID;
        bridge.kind = placement.kind;
        bridge.requireQuestActive = true;
        bridge.ApplyCatalogEntry();

        spawned.Add(new SpawnedRegion
        {
            questID = questID,
            objectiveID = placement.catalogObjectiveID,
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

    void OnObjectiveChanged(QuestData quest, QuestObjective objective) => RefreshAllVisibility();

    void RefreshAllVisibility()
    {
        if (QuestManager.Instance == null)
            return;

        foreach (var entry in spawned)
        {
            if (entry.instance == null)
                continue;

            bool show = ShouldShowRegion(entry.questID, entry.objectiveID);
            entry.instance.SetActive(show);
        }
    }

    static bool ShouldShowRegion(string questID, string objectiveID)
    {
        var qm = QuestManager.Instance;

        if (!qm.IsQuestActive(questID))
            return false;

        var state = qm.GetObjectiveState(questID, objectiveID);
        return !state.isCompleted;
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
