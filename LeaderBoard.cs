using System.Collections.Generic;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    public List<PlayerScore> scores = new();

    public void AddScore(string name, int score)
    {
        if (score > 0)
        {
            GameObject scoreObject = new("PlayerScore_" + name);
            scoreObject.transform.SetParent(transform);
            PlayerScore playerScoreComponent = scoreObject.AddComponent<PlayerScore>();
            playerScoreComponent.SetPlayerScore(name, score);
            scores.Add(playerScoreComponent);
            scores.Sort((a, b) => b.playerScore.CompareTo(a.playerScore));

        }

    }

}
