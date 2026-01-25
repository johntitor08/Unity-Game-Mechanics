using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffManager : MonoBehaviour
{
    public static PlayerBuffManager Instance;
    private readonly List<Buff> activeBuffs = new();
    public event Action OnBuffsChanged;

    [Serializable]
    public class Buff
    {
        public string id;
        public BuffType type;
        public float multiplier = 1f;
        public float duration = 5f;
        public bool stackable = false;
        [HideInInspector] public float endTime;
        public float damageMultiplier = 1f;
        public float damageReduction = 0f;
    }

    public enum BuffType
    {
        HealthRegen,
        EnergyRegen,
        Damage,
        Defense,
        Speed,
        CritChance
    }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Update()
    {
        UpdateBuffs();
    }

    public void AddBuff(Buff buff)
    {
        if (!buff.stackable)
        {
            Buff existing = activeBuffs.Find(b => b.id == buff.id);

            if (existing != null)
            {
                existing.endTime = Time.time + buff.duration;
                OnBuffsChanged?.Invoke();
                return;
            }
        }

        buff.endTime = Time.time + buff.duration;
        activeBuffs.Add(buff);
        OnBuffsChanged?.Invoke();
    }

    void UpdateBuffs()
    {
        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            if (Time.time >= activeBuffs[i].endTime)
            {
                activeBuffs.RemoveAt(i);
                OnBuffsChanged?.Invoke();
            }
        }
    }

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;

        foreach (var buff in activeBuffs)
        {
            multiplier *= buff.damageMultiplier;
        }

        return multiplier;
    }

    public float GetDamageReduction()
    {
        float reduction = 0f;

        foreach (var buff in activeBuffs)
        {
            reduction += buff.damageReduction;
        }

        return Mathf.Clamp01(reduction);
    }

    public bool HasBuff(string id)
    {
        return activeBuffs.Exists(b => b.id == id);
    }

    public List<Buff> GetActiveBuffs()
    {
        return new List<Buff>(activeBuffs);
    }

    public void ClearAll()
    {
        activeBuffs.Clear();
        OnBuffsChanged?.Invoke();
    }
}
