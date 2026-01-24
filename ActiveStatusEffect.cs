using UnityEngine;

[System.Serializable]
public class ActiveStatusEffect
{
    public StatusEffectData data;
    public float remainingDuration;
    public float nextTickTime;
    public int currentStacks;
    public GameObject particleInstance;

    public ActiveStatusEffect(StatusEffectData effectData)
    {
        data = effectData;
        remainingDuration = effectData.duration;
        nextTickTime = 0f;
        currentStacks = 1;
    }

    public bool IsExpired()
    {
        return !data.isPermanent && remainingDuration <= 0f;
    }

    public void RefreshDuration()
    {
        remainingDuration = data.duration;
    }

    public void AddStack()
    {
        if (data.canStack && currentStacks < data.maxStacks)
        {
            currentStacks++;
        }
    }
}
