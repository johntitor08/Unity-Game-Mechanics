using System.Collections.Generic;
using UnityEngine;

public class ScenarioDebug : MonoBehaviour
{
    public string scenarioID = "ashenveil_day2";
    ScenarioManager SM => ScenarioManager.Instance;

    [ContextMenu("Play · Day 2")]
    public void PlayDay2() => ForceStart("ashenveil_day2");

    [ContextMenu("Play · Day 3")]
    public void PlayDay3() => ForceStart("ashenveil_day3");

    [ContextMenu("Play · Day 4")]
    public void PlayDay4() => ForceStart("ashenveil_day4");

    [ContextMenu("Play · Red Saint")]
    public void PlayRedSaint() => ForceStart("ashenveil_red_saint");

    [ContextMenu("Play · Bound Archivist")]
    public void PlayBoundArchivist() => ForceStart("ashenveil_bound_archivist");

    [ContextMenu("Play · Bound Archivist Q2")]
    public void PlayBoundArchivistQ2() => ForceStart("ashenveil_bound_archivist_q2");

    [ContextMenu("Play · Bound Archivist Q3")]
    public void PlayBoundArchivistQ3() => ForceStart("ashenveil_bound_archivist_q3");

    [ContextMenu("Play · Foreign Echo")]
    public void PlayForeignEcho() => ForceStart("ashenveil_foreign_echo");

    [ContextMenu("Play · Foreign Echo Q2")]
    public void PlayForeignEchoQ2() => ForceStart("ashenveil_foreign_echo_q2");

    [ContextMenu("Play · Foreign Echo Q3")]
    public void PlayForeignEchoQ3() => ForceStart("ashenveil_foreign_echo_q3");

    [ContextMenu("Play · Sinned Guardian")]
    public void PlaySinnedGuardian() => ForceStart("ashenveil_sinned_guardian");

    [ContextMenu("Play · Sinned Guardian Q2")]
    public void PlaySinnedGuardianQ2() => ForceStart("ashenveil_sinned_guardian_q2");

    [ContextMenu("Play · Sinned Guardian Q3")]
    public void PlaySinnedGuardianQ3() => ForceStart("ashenveil_sinned_guardian_q3");

    [ContextMenu("Play · Awamori")]
    public void PlayAwamori() => ForceStart("ashenveil_awamori");

    [ContextMenu("Play · Eulogy")]
    public void PlayEulogy() => ForceStart("ashenveil_eulogy");

    [ContextMenu("Play · Scenario By ID (field above)")]
    public void PlayByID() => ForceStart(scenarioID);

    public void ForceStart(string id)
    {
        if (!Application.isPlaying)
        {
            Debug.LogWarning("[ScenarioDebug] Enter Play mode first — scenarios only run at runtime.");
            return;
        }

        if (SM == null)
        {
            Debug.LogError("[ScenarioDebug] ScenarioManager.Instance is null (not in scene / not yet initialized).");
            return;
        }

        var data = SM.GetScenarioByID(id);

        if (data == null)
        {
            Debug.LogError($"[ScenarioDebug] Scenario '{id}' not found in ScenarioManager.availableScenarios.");
            return;
        }

        if (data.requiredFlags != null)
            foreach (var flag in data.requiredFlags)
                StoryFlags.Add(flag);

        var completed = SM.GetCompletedScenarios();

        if (data.prerequisiteScenarios != null)
            foreach (var prereq in data.prerequisiteScenarios)
                if (prereq != null)
                    completed.Add(prereq.scenarioID);

        completed.Remove(id);
        SM.SetCompletedScenarios(completed);

        if (!SM.CanStartScenario(data))
        {
            Debug.LogWarning($"[ScenarioDebug] CanStartScenario still false for '{id}'. " + "Check requiredLevel or that DialogueManager/ScenarioManager are present.");
            return;
        }

        SM.StartScenario(data);
        Debug.Log($"<color=#7ec8e3>[ScenarioDebug]</color> Started '{id}'. Advance with mouse-click / Space.");
    }

    [ContextMenu("Reset · Clear story flags + completed scenarios")]
    public void ResetStory()
    {
        StoryFlags.Reset();

        if (SM != null)
            SM.SetCompletedScenarios(new HashSet<string>());

        Debug.Log("[ScenarioDebug] Cleared story flags and completed scenarios.");
    }
}
