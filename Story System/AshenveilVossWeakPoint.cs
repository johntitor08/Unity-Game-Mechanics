using System.Collections;
using UnityEngine;

public class AshenveilVossWeakPoint : MonoBehaviour
{
    [Header("Boss Reference")]
    public EnemyData vossData;

    private bool _subscribed;
    private Coroutine _subscribeRoutine;

    void OnEnable()
    {
        // CombatManager is DontDestroyOnLoad and may not exist yet at scene load,
        // so wait for it instead of silently missing the subscription.
        _subscribeRoutine = StartCoroutine(SubscribeWhenReady());
    }

    void OnDisable()
    {
        if (_subscribeRoutine != null)
        {
            StopCoroutine(_subscribeRoutine);
            _subscribeRoutine = null;
        }

        if (_subscribed && CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStarted -= OnCombatStarted;

        _subscribed = false;
    }

    IEnumerator SubscribeWhenReady()
    {
        while (CombatManager.Instance == null)
            yield return null;

        CombatManager.Instance.OnCombatStarted += OnCombatStarted;
        _subscribed = true;
        _subscribeRoutine = null;
    }

    void OnCombatStarted()
    {
        if (CombatManager.Instance == null || CombatManager.Instance.currentEnemy != vossData || !StoryFlags.Has(QuestFlags.VossWeakPointKnown) || StoryFlags.Has(QuestFlags.VossWeakPointApplied))
            return;

        var enemyStats = CombatManager.Instance.enemyStats;

        if (enemyStats == null)
        {
            Debug.LogWarning("[AshenveilVossWeakPoint] enemyStats is null.");
            return;
        }

        int currentDef = enemyStats.Get(StatType.Defense);
        int reducedDef = Mathf.RoundToInt(currentDef * 0.5f);
        enemyStats.Set(StatType.Defense, reducedDef, save: false);
        StoryFlags.Add(QuestFlags.VossWeakPointApplied);

        if (CombatUI.Instance != null)
            CombatUI.Instance.AddLogMessage("Voss'un savunması zayıf başlıyor...");

        Debug.Log($"[AshenveilVossWeakPoint] DEF reduced: {currentDef} → {reducedDef}");
    }
}
