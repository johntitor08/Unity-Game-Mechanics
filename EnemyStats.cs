using System.Collections.Generic;
using UnityEngine;

public class EnemyStats : StatsBase
{
    private static readonly WaitForSeconds _waitForSeconds0_1 = new(0.1f);

    [Header("Enemy Settings")]
    public bool destroyOnDeath = true;
    public float deathDelay = 0.1f;

    protected override void Awake()
    {
        base.Awake();

        if (stats.Count == 0)
        {
            stats = new List<Stat>
            {
                new(StatType.MaxHealth, 100, 1, 999),
                new(StatType.Health, 100, 0, 999),
                new(StatType.Strength, 5, 0, 999),
                new(StatType.Defense, 3, 0, 999)
            };

            InitializeStats();
        }
    }

    public void InitializeFromData(EnemyData data)
    {
        if (data == null) return;
        stats.Clear();
        stats.Add(new Stat(StatType.MaxHealth, data.maxHealth, 1, data.maxHealth));
        stats.Add(new Stat(StatType.Health, data.maxHealth, 0, data.maxHealth));
        stats.Add(new Stat(StatType.Strength, data.attack, 0, 999));
        stats.Add(new Stat(StatType.Defense, data.defense, 0, 999));
        stats.Add(new Stat(StatType.Speed, data.speed, 0, 999));
        InitializeStats();
    }

    protected override void OnDie()
    {
        if (destroyOnDeath)
        {
            StartCoroutine(DieDelayed());
        }
    }

    System.Collections.IEnumerator DieDelayed()
    {
        yield return _waitForSeconds0_1;
        Destroy(gameObject);
    }
}
