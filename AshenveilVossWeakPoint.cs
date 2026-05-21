using UnityEngine;

public class AshenveilVossWeakPoint : MonoBehaviour
{
    [Header("Boss Reference")]
    public EnemyData vossData;

    void OnEnable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStarted += OnCombatStarted;
    }

    void OnDisable()
    {
        if (CombatManager.Instance != null)
            CombatManager.Instance.OnCombatStarted -= OnCombatStarted;
    }

    void OnCombatStarted()
    {
        if (CombatManager.Instance == null || CombatManager.Instance.currentEnemy != vossData || !StoryFlags.Has("voss_weak_point_known") || StoryFlags.Has("voss_weak_point_applied"))
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
        StoryFlags.Add("voss_weak_point_applied");

        if (CombatUI.Instance != null)
            CombatUI.Instance.AddLogMessage("Voss'un savunması zayıf başlıyor...");

        Debug.Log($"[AshenveilVossWeakPoint] DEF reduced: {currentDef} → {reducedDef}");
    }
}
