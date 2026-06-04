using UnityEngine;

public class QuestLocationTrigger : QuestPlayerTriggerBase
{
    [Header("Location")]
    public string locationTag;

    [Min(1)]
    public int progressAmount = 1;

    public bool fireOnEnter = true;

    protected override void Update()
    {
        if (!fireOnEnter)
            base.Update();
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);

        if (!fireOnEnter || !other.CompareTag("Player"))
            return;

        TryFire();
    }

    protected override void OnFire()
    {
        string tag = string.IsNullOrEmpty(locationTag) ? gameObject.name : locationTag;
        QuestManager.Instance.NotifyLocationReached(tag, progressAmount);
    }
}
