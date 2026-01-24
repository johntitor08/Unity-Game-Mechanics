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
    public event Action<StatusEffectData> OnEffectExpired;
    private IStatOwner statOwner;

    private void Awake()
    {
        statOwner = GetComponent<IStatOwner>();

        if (statOwner == null)
            Debug.LogError($"{name}: IStatOwner component missing!", this);

        if (particleParent == null)
            particleParent = transform;
    }

    private void Update()
    {
        UpdateEffects();
    }

    private void UpdateEffects()
    {
        if (activeEffects.Count == 0) return;
        float dt = Time.deltaTime;

        for (int i = activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = activeEffects[i];

            // Handle duration
            if (!effect.data.isPermanent)
            {
                effect.remainingDuration -= dt;

                if (effect.IsExpired())
                {
                    RemoveEffect(effect);
                    continue;
                }
            }

            // Handle ticks
            if (effect.data.hasTicks && effect.data.tickInterval > 0f)
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

    public void ApplyEffect(StatusEffectData effectData)
    {
        if (effectData == null || statOwner == null) return;
        ActiveStatusEffect existing = GetActiveEffect(effectData.effectType);

        if (existing != null)
        {
            // Handle stacking
            if (effectData.canStack)
            {
                ApplyStatModifiers(existing, false);
                existing.AddStack();
                ApplyStatModifiers(existing, true);
            }

            // Refresh duration if needed
            if (effectData.refreshOnReapply)
            {
                existing.RefreshDuration();
                OnEffectApplied?.Invoke(existing.data); // Notify UI
            }

            return;
        }

        // New effect
        ActiveStatusEffect newEffect = new(effectData);
        activeEffects.Add(newEffect);
        ApplyStatModifiers(newEffect, true);

        // Spawn particle
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
        if (effect == null || statOwner == null) return;
        ApplyStatModifiers(effect, false);

        if (effect.particleInstance != null)
            Destroy(effect.particleInstance);

        activeEffects.Remove(effect);
        OnEffectExpired?.Invoke(effect.data);
        OnEffectRemoved?.Invoke(effect.data);
    }

    private void ProcessTick(ActiveStatusEffect effect)
    {
        if (effect == null || statOwner == null) return;
        int totalDamage = effect.data.tickDamage * effect.currentStacks;

        if (totalDamage != 0)
        {
            statOwner.Modify(StatType.Health, -totalDamage);
            OnEffectTick?.Invoke(effect.data, totalDamage);
            PlaySound(effect.data.tickSound);
        }
    }

    // Public removal helpers
    public void RemoveEffect(StatusEffectType type)
    {
        ActiveStatusEffect effect = GetActiveEffect(type);
        if (effect != null) RemoveEffect(effect);
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

    // Queries
    public bool HasEffect(StatusEffectType type) => GetActiveEffect(type) != null;

    public ActiveStatusEffect GetActiveEffect(StatusEffectType type) =>
        activeEffects.FirstOrDefault(e => e.data.effectType == type);

    public bool CanAct() => !activeEffects.Any(e => e.data.preventActions);
    public bool CanMove() => !activeEffects.Any(e => e.data.preventMovement);

    public float GetDamageMultiplier()
    {
        float multiplier = 1f;

        foreach (var effect in activeEffects)
            multiplier *= effect.data.damageMultiplier != 0f ? effect.data.damageMultiplier : 1f;

        return multiplier;
    }

    public float GetDamageReduction()
    {
        float reduction = 0f;

        foreach (var effect in activeEffects)
            reduction += effect.data.damageReduction;

        return Mathf.Clamp01(reduction);
    }

    // Apply or remove stat modifiers
    private void ApplyStatModifiers(ActiveStatusEffect effect, bool apply)
    {
        if (effect.data.statModifiers == null || statOwner == null) return;

        foreach (var mod in effect.data.statModifiers)
        {
            int amount = mod.amount * effect.currentStacks;

            if (mod.isPercentage)
                amount = Mathf.RoundToInt(statOwner.Get(mod.statType) * mod.amount / 100f);

            statOwner.Modify(mod.statType, apply ? amount : -amount);
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (clip != null && Camera.main != null)
            AudioSource.PlayClipAtPoint(clip, Camera.main.transform.position, 0.5f);
    }
}
