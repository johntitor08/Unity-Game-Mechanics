using System;
using UnityEngine;

[Serializable]
public class PlayerScore : MonoBehaviour
{
    public string playerName;
    public int playerScore;

    public void SetPlayerScore(string name, int score)
    {
        playerName = name;
        playerScore = score;

    }

}