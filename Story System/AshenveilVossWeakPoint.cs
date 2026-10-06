using System.Collections;
using UnityEngine;

public class AshenveilVossWeakPoint : MonoBehaviour
{
    [Header("Boss Reference")]
    public EnemyData vossData;

    private bool _subscribed;
    private Coroutine _subscribeRoutine;
    public const string SerenaBondBroken = "serena_bond_broken";
    public const string VossKilled = "voss_killed";
    public const string CourtLanternKnown = "court_lantern_known";

    void OnEnable()
    {
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
        if (CombatManager.Instance == null)
            return;

        ApplyVossFateToVanguard();

        if (CombatManager.Instance.currentEnemy != vossData)
            return;

        ApplySerenasThread();

        if (!StoryFlags.Has(QuestFlags.VossWeakPointKnown))
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
            CombatUI.Instance.AddLogMessage(LanguageManager.Current == GameLanguage.TR ? "Voss'un savunması zayıf başlıyor..." : "Voss's defenses are weakened...");

        Debug.Log($"[AshenveilVossWeakPoint] DEF reduced: {currentDef} → {reducedDef}");
    }

    void ApplySerenasThread()
    {
        var sm = ScenarioManager.Instance;
        var current = sm != null ? sm.GetCurrentScenario() : null;

        if (current == null || current.scenarioID != "ashenveil_day3" || StoryFlags.Has(SerenaBondBroken))
            return;

        var stats = CombatManager.Instance.enemyStats;

        if (stats == null)
            return;

        ScaleHealth(stats, 1.25f);
        stats.Set(StatType.Strength, Mathf.RoundToInt(stats.Get(StatType.Strength) * 1.1f), save: false);

        if (CombatUI.Instance != null)
            CombatUI.Instance.AddLogMessage(LanguageManager.Current == GameLanguage.TR ? "Voss, Serena'nın ipliğiyle güçleniyor..." : "Voss draws strength from Serena's thread...");

        Debug.Log($"[AshenveilVossWeakPoint] Serena's thread: HP {stats.Get(StatType.MaxHealth)}");
    }

    void ApplyVossFateToVanguard()
    {
        var sm = ScenarioManager.Instance;
        var current = sm != null ? sm.GetCurrentScenario() : null;

        if (current == null || current.scenarioID != "ashenveil_day4" || current.steps == null)
            return;

        int index = sm.GetCurrentStepIndex();

        if (index < 0 || index >= current.steps.Length || current.steps[index].stepName != "d4_vanguard")
            return;

        var stats = CombatManager.Instance.enemyStats;

        if (stats == null)
            return;

        bool tr = LanguageManager.Current == GameLanguage.TR;

        if (StoryFlags.Has(VossKilled))
        {
            ScaleHealth(stats, 1.2f);
            stats.Set(StatType.Strength, Mathf.RoundToInt(stats.Get(StatType.Strength) * 1.15f), save: false);

            if (CombatUI.Instance != null)
                CombatUI.Instance.AddLogMessage(tr ? "Saray, öldürülen kâtibi için en öfkelilerini göndermiş..." : "The Court has sent its angriest for its dead clerk...");
        }
        else if (StoryFlags.Has(CourtLanternKnown))
        {
            ScaleHealth(stats, 0.85f);
            stats.Set(StatType.Defense, Mathf.RoundToInt(stats.Get(StatType.Defense) * 0.5f), save: false);

            if (CombatUI.Instance != null)
                CombatUI.Instance.AddLogMessage(tr ? "Voss'un dediği gibi fenere vuruyorsun; gölgeleri inceliyor..." : "You strike the lantern, as Voss told you. Their shadows thin...");
        }
    }

    static void ScaleHealth(EnemyStats stats, float factor)
    {
        int hp = Mathf.Max(1, Mathf.RoundToInt(stats.Get(StatType.MaxHealth) * factor));

        foreach (var s in stats.stats)
            if (s.type == StatType.MaxHealth || s.type == StatType.Health)
                s.maxValue = Mathf.Max(s.maxValue, hp);

        stats.Set(StatType.MaxHealth, hp, save: false);
        stats.Set(StatType.Health, hp, save: false);
    }
}
