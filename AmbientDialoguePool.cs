using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class AmbientDialogueLine
{
    [TextArea(2, 4)]
    public string text;

    [Header("Conditions")]
    public string[] requiredFlags;
    public string[] forbiddenFlags;
    public int minDay = 1;
    public int maxDay = 0;
    public TimePhase[] allowedPhases;
    [Min(0.01f)]
    public float weight = 1f;
}

[CreateAssetMenu(menuName = "Dialogue/Ambient Dialogue Pool")]
public class AmbientDialoguePool : ScriptableObject
{
    [Header("Speaker")]
    public string speakerName = "NPC";
    public Sprite speakerPortrait;
    public Color speakerNameColor = Color.gold;

    [Header("Lines")]
    public AmbientDialogueLine[] lines;

    [TextArea(2, 4)]
    public string fallbackLine = "...";

    [System.NonSerialized]
    private int lastPickedIndex = -1;

    public string PickLine()
    {
        int day = TimeUI.Instance != null ? TimeUI.Instance.GetCurrentDay() : 1;
        TimePhase phase = TimePhaseManager.Instance != null ? TimePhaseManager.Instance.currentPhase : TimePhase.Morning;
        List<int> eligible = new();

        if (lines != null)
            for (int i = 0; i < lines.Length; i++)
                if (IsEligible(lines[i], day, phase))
                    eligible.Add(i);

        if (eligible.Count > 1)
            eligible.Remove(lastPickedIndex);

        if (eligible.Count == 0)
            return fallbackLine;

        int picked = PickWeighted(eligible);
        lastPickedIndex = picked;
        return lines[picked].text;
    }

    bool IsEligible(AmbientDialogueLine line, int day, TimePhase phase)
    {
        if (line == null || string.IsNullOrEmpty(line.text) || day < line.minDay || (line.maxDay > 0 && day > line.maxDay) || (line.allowedPhases != null && line.allowedPhases.Length > 0 && System.Array.IndexOf(line.allowedPhases, phase) < 0))
            return false;

        if (line.requiredFlags != null)
            foreach (var flag in line.requiredFlags)
                if (!string.IsNullOrEmpty(flag) && !StoryFlags.Has(flag))
                    return false;

        if (line.forbiddenFlags != null)
            foreach (var flag in line.forbiddenFlags)
                if (!string.IsNullOrEmpty(flag) && StoryFlags.Has(flag))
                    return false;

        return true;
    }

    int PickWeighted(List<int> eligible)
    {
        float total = 0f;

        foreach (int i in eligible)
            total += lines[i].weight;

        float roll = Random.value * total;

        foreach (int i in eligible)
        {
            roll -= lines[i].weight;

            if (roll <= 0f)
                return i;
        }

        return eligible[^1];
    }
}
