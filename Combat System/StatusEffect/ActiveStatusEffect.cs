using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveStatusEffect
{
    public StatusEffectData data;
    public float remainingDuration;
    public float nextTickTime;
    public int currentStacks;
    public GameObject particleInstance;
    public int remainingRounds;
    public SerializableDictionary<int, int> appliedModifierAmounts = new();
    public SerializableDictionary<int, int> baseStatSnapshots = new();

    public ActiveStatusEffect(StatusEffectData effectData)
    {
        data = effectData;
        remainingDuration = effectData.duration;
        nextTickTime = effectData.tickInterval;
        currentStacks = 1;
        remainingRounds = effectData.durationRounds;
    }

    public bool IsExpired()
    {
        return !data.isPermanent && remainingDuration <= 0f;
    }

    public void RefreshDuration()
    {
        remainingDuration = data.duration;
        remainingRounds = data.durationRounds;
    }

    public void AddStack()
    {
        if (data.canStack && currentStacks < data.maxStacks)
            currentStacks++;
    }
}
