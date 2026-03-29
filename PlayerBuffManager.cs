using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBuffManager : MonoBehaviour
{
    public static PlayerBuffManager Instance;
    private readonly List<Buff> activeBuffs = new();
    public event Action OnBuffsChanged;

    [Header("Buff UI")]
    public Transform buffUIParent;
    public GameObject buffUIPrefab;
    private readonly Dictionary<string, BuffUI> buffUIMap = new();

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
        public Sprite icon;
        public string displayName;
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
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
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

                if (buffUIMap.TryGetValue(buff.id, out var existingUI))
                    existingUI.UpdateTimer(buff.duration);

                OnBuffsChanged?.Invoke();
                return;
            }
        }

        buff.endTime = Time.time + buff.duration;
        activeBuffs.Add(buff);
        SpawnBuffUI(buff);
        OnBuffsChanged?.Invoke();
    }

    void UpdateBuffs()
    {
        bool changed = false;

        for (int i = activeBuffs.Count - 1; i >= 0; i--)
        {
            Buff buff = activeBuffs[i];
            float timeLeft = buff.endTime - Time.time;

            if (buffUIMap.TryGetValue(buff.id, out var ui))
                ui.UpdateTimer(timeLeft);

            if (Time.time >= buff.endTime)
            {
                RemoveBuffUI(buff.id);
                activeBuffs.RemoveAt(i);
                changed = true;
            }
        }

        if (changed)
            OnBuffsChanged?.Invoke();
    }

    private void SpawnBuffUI(Buff buff)
    {
        if (buffUIPrefab == null || buffUIParent == null || string.IsNullOrEmpty(buff.displayName))
            return;

        GameObject obj = Instantiate(buffUIPrefab, buffUIParent);
        
        if (!obj.TryGetComponent<BuffUI>(out var ui))
            return;

        ui.Setup(buff.icon, buff.displayName, buff.duration);
        string key = buff.stackable ? buff.id + "_" + Time.time : buff.id;
        buffUIMap[key] = ui;
    }

    private void RemoveBuffUI(string id)
    {
        if (buffUIMap.TryGetValue(id, out var ui))
        {
            if (ui != null)
                Destroy(ui.gameObject);

            buffUIMap.Remove(id);
        }
    }

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;

        foreach (var buff in activeBuffs)
        {
            if (buff.type == BuffType.Damage)
                multiplier *= buff.damageMultiplier;
        }

        return multiplier;
    }

    public float GetDamageReduction()
    {
        float reduction = 0f;

        foreach (var buff in activeBuffs)
        {
            if (buff.type == BuffType.Defense)
                reduction += buff.damageReduction;
        }

        return Mathf.Clamp01(reduction);
    }

    public bool HasBuff(string id) => activeBuffs.Exists(b => b.id == id);

    public List<Buff> GetActiveBuffs() => new(activeBuffs);

    public void ClearAll()
    {
        foreach (var id in buffUIMap.Keys)
        {
            if (buffUIMap[id] != null)
                Destroy(buffUIMap[id].gameObject);
        }

        buffUIMap.Clear();
        activeBuffs.Clear();
        OnBuffsChanged?.Invoke();
    }
}
