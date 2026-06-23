using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PhaseBackground : MonoBehaviour
{
    [System.Serializable]
    public class PhaseSet
    {
        public string label;
        public Sprite morning;
        public Sprite noon;
        public Sprite evening;
        public Sprite night;

        public bool Contains(Sprite s) => s != null && (s == morning || s == noon || s == evening || s == night);

        public Sprite For(TimePhase p) => p switch
        {
            TimePhase.Morning => morning,
            TimePhase.Noon => noon,
            TimePhase.Evening => evening,
            TimePhase.Night => night,
            _ => noon
        };
    }

    public Image background;
    public List<PhaseSet> sets = new();

    void Update()
    {
        if (background == null || background.sprite == null || TimePhaseManager.Instance == null)
            return;

        var cur = background.sprite;

        foreach (var set in sets)
        {
            if (!set.Contains(cur))
                continue;

            var want = set.For(TimePhaseManager.Instance.currentPhase);

            if (want != null && cur != want)
                background.sprite = want;

            return;
        }
    }
}
