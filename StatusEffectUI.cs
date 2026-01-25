using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StatusEffectUI : MonoBehaviour
{
    [Header("References")]
    public StatusEffectManager effectManager;
    public Transform iconContainer;
    public StatusEffectIcon iconPrefab;

    private readonly List<StatusEffectIcon> activeIcons = new();
    private bool isSubscribed = false;

    void OnEnable()
    {
        TrySubscribe();
    }

    void OnDisable()
    {
        if (isSubscribed && effectManager != null)
        {
            effectManager.OnEffectApplied -= OnEffectApplied;
            effectManager.OnEffectRemoved -= OnEffectRemoved;
            isSubscribed = false;
        }
    }

    void TrySubscribe()
    {
        if (effectManager != null && !isSubscribed)
        {
            effectManager.OnEffectApplied += OnEffectApplied;
            effectManager.OnEffectRemoved += OnEffectRemoved;
            isSubscribed = true;
        }
    }

    void Update()
    {
        if (effectManager == null) return;

        for (int i = activeIcons.Count - 1; i >= 0; i--)
        {
            var icon = activeIcons[i];
            if (icon == null) continue;
            ActiveStatusEffect effect = effectManager.GetActiveEffect(icon.effectType);

            if (effect != null)
            {
                icon.UpdateEffect(effect);
            }
            else
            {
                activeIcons.RemoveAt(i);
                Destroy(icon.gameObject);
            }
        }
    }

    private void OnEffectApplied(StatusEffectData effect)
    {
        StatusEffectIcon icon = activeIcons.FirstOrDefault(i => i.effectType == effect.effectType);

        if (icon != null)
        {
            icon.UpdateEffect(effectManager.GetActiveEffect(effect.effectType));
        }
        else
        {
            icon = Instantiate(iconPrefab, iconContainer);
            icon.Setup(effectManager.GetActiveEffect(effect.effectType));
            activeIcons.Add(icon);
        }
    }

    private void OnEffectRemoved(StatusEffectData effect)
    {
        StatusEffectIcon icon = activeIcons.FirstOrDefault(i => i.effectType == effect.effectType);

        if (icon != null)
        {
            activeIcons.Remove(icon);
            Destroy(icon.gameObject);
        }
    }
}
