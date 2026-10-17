using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(IStatOwner))]
public class StatusEffectManager : MonoBehaviour
{
    [Header("Effect Settings")]
    public Transform particleParent;
    public List<ActiveStatusEffect> activeEffects = new();

    public event Action<StatusEffectData> OnEffectApplied;
    public event Action<StatusEffectData> OnEffectRemoved;
    public event Action<StatusEffectData, int> OnEffectTick;
    public event Action<StatusEffectData, int> OnEffectHeal;
    public event Action<StatusEffectData> OnEffectExpired;
    private IStatOwner statOwner;
    private Camera _mainCamera;

    private void Awake()
    {
        statOwner = GetComponent<IStatOwner>();

        if (statOwner == null)
            Debug.LogError($"{name}: IStatOwner component missing!", this);

        if (particleParent == null)
            particleParent = transform;

        _mainCamera = Camera.main;
    }

    private void Update()
    {
        UpdateEffects();
    }

    private void UpdateEffects()
    {
        if (activeEffects.Count == 0)
            return;

        float dt = Time.deltaTime;

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];

            if (!effect.data.isPermanent && !effect.data.isRoundBased)
            {
                effect.remainingDuration -= dt;

                if (effect.IsExpired())
                {
                    RemoveEffect(effect);
                    continue;
                }
            }

            if (effect.data.hasTicks && effect.data.tickInterval > 0f && !effect.data.isRoundBased)
            {
                effect.nextTickTime -= dt;

                while (effect.nextTickTime <= 0f)
                {
                    ProcessTick(effect);
                    effect.nextTickTime += effect.data.tickInterval;
                }
            }
        }
    }

    public void OnEnemyTurnStart()
    {
        TickRoundBasedEffects(onPlayerTurn: false);
    }

    public void OnPlayerTurnStart()
    {
        TickRoundBasedEffects(onPlayerTurn: true);
    }

    private void TickRoundBasedEffects(bool onPlayerTurn)
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];

            if (!effect.data.isRoundBased)
                continue;

            bool shouldTick = onPlayerTurn ? !effect.data.isDebuff : effect.data.isDebuff;

            if (!shouldTick)
                continue;

            if (effect.data.hasTicks)
                ProcessTick(effect);

            effect.remainingRounds--;

            if (effect.remainingRounds <= 0)
                RemoveEffect(effect);
        }
    }

    public void ApplyEffect(StatusEffectData effectData)
    {
        if (effectData == null || statOwner == null)
            return;

        ActiveStatusEffect existing = GetActiveEffect(effectData.effectType);

        if (existing != null)
        {
            if (effectData.canStack)
                existing.AddStack();

            if (effectData.refreshOnReapply)
            {
                existing.RefreshDuration();
                OnEffectApplied?.Invoke(existing.data);
            }

            return;
        }

        ActiveStatusEffect newEffect = new(effectData);
        activeEffects.Add(newEffect);

        if (effectData.particleEffectPrefab != null && particleParent != null)
        {
            newEffect.particleInstance = Instantiate(effectData.particleEffectPrefab, particleParent);
            newEffect.particleInstance.transform.localPosition = Vector3.zero;
        }

        PlaySound(effectData.applySound);
        OnEffectApplied?.Invoke(effectData);
    }

    private void RemoveEffect(ActiveStatusEffect effect)
    {
        if (effect == null || statOwner == null)
            return;

        if (effect.particleInstance != null)
            Destroy(effect.particleInstance);

        activeEffects.Remove(effect);
        OnEffectExpired?.Invoke(effect.data);
        OnEffectRemoved?.Invoke(effect.data);
    }

    private void ProcessTick(ActiveStatusEffect effect)
    {
        if (effect == null || statOwner == null)
            return;

        if (effect.data.tickHeal > 0)
        {
            int totalHeal = effect.data.tickHeal * effect.currentStacks;
            statOwner.Modify(StatType.Health, totalHeal, false);
            OnEffectHeal?.Invoke(effect.data, totalHeal);
            PlaySound(effect.data.tickSound);
        }
        else if (effect.data.tickDamage != 0)
        {
            int totalDamage = effect.data.tickDamage * effect.currentStacks;
            statOwner.Modify(StatType.Health, -totalDamage, false);
            OnEffectTick?.Invoke(effect.data, totalDamage);
            PlaySound(effect.data.tickSound);
        }
    }

    public void RemoveEffect(StatusEffectType type)
    {
        ActiveStatusEffect effect = GetActiveEffect(type);

        if (effect != null)
            RemoveEffect(effect);
    }

    public void RemoveAllEffects()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
            RemoveEffect(activeEffects[i]);
    }

    public void RemoveAllDebuffs()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (activeEffects[i].data.isDebuff && activeEffects[i].data.isPurgeableByPlayer)
                RemoveEffect(activeEffects[i]);
        }
    }

    public void RemoveAllBuffs()
    {
        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            if (!activeEffects[i].data.isDebuff && activeEffects[i].data.isPurgeableByPlayer)
                RemoveEffect(activeEffects[i]);
        }
    }

    public bool HasEffect(StatusEffectType type) => GetActiveEffect(type) != null;

    public ActiveStatusEffect GetActiveEffect(StatusEffectType type) => activeEffects.FirstOrDefault(e => e.data.effectType == type);

    public bool CanAct() => !activeEffects.Any(e => e.data.preventActions);

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;

        foreach (var effect in activeEffects)
        {
            if (effect.data.damageMultiplier > 0f)
                multiplier *= effect.data.damageMultiplier;
        }

        return multiplier;
    }

    public float GetDamageReduction()
    {
        float reduction = 0f;

        foreach (var effect in activeEffects)
            reduction += effect.data.damageReduction;

        return Mathf.Clamp01(reduction);
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && _mainCamera != null)
            AudioSource.PlayClipAtPoint(clip, _mainCamera.transform.position, 0.5f);
    }
}
