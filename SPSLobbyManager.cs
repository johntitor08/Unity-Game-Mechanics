#if !UNITY_WEBGL

using Firebase.Database;

#else

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

#endif

using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(LeaderBoard))]
public class SPSLobbyManager : MonoBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private LeaderBoard leaderboard;
    private UserManager userManager;
    public PlayerScore playerScoreItemPrefab;
    public Transform playerScoreItemContentObject;

    private IEnumerator Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();

        yield return new WaitUntil(() => userManager != null && userManager.IsAuthenticated);

#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif

        leaderboard = GetComponent<LeaderBoard>();
        yield return StartCoroutine(GetAllHighscoresFromDatabase());
        int maxLength = 10;

        for (int i = 0; i < Mathf.Min(leaderboard.scores.Count, maxLength); i++)
        {
            PlayerScore playerScoreItem = leaderboard.scores[i];
            PlayerScore newPlayerScoreItem = Instantiate(playerScoreItemPrefab, playerScoreItemContentObject);
            TMP_Text[] childTexts = newPlayerScoreItem.GetComponentsInChildren<TMP_Text>();
            childTexts[0].text = (i + 1).ToString();
            childTexts[1].text = playerScoreItem.playerName;
            childTexts[2].text = playerScoreItem.playerScore.ToString();
        }
    }

    public void JoinGame()
    {
        SceneManager.LoadScene("SPSGame");
    }

#if UNITY_WEBGL

    public IEnumerator GetAllHighscoresFromDatabase()
    {
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users.json?auth=" + userManager.IdToken;
        using UnityWebRequest getRequest = UnityWebRequest.Get(url);
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.Success)
        {
            JObject json = JsonConvert.DeserializeObject<JObject>(getRequest.downloadHandler.text);

            foreach (var user in json)
            {
                string username = user.Value["username"]?.ToString();
                if (string.IsNullOrEmpty(username)) continue;
                if (user.Value["highscore"] == null) continue;
                int highscore = int.Parse(user.Value["highscore"].ToString());
                leaderboard.AddScore(username, highscore);
            }
        }
        else
        {
            Debug.LogError("Failed to get all highscores from the database: " + getRequest.error);
        }
    }

#else

    public IEnumerator GetAllHighscoresFromDatabase()
    {
        var task = databaseReference.Child("Users").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            DataSnapshot usersSnapshot = task.Result;

            if (usersSnapshot != null && usersSnapshot.HasChildren)
            {
                foreach (var userSnapshot in usersSnapshot.Children)
                {
                    DataSnapshot usernameSnapshot = userSnapshot.Child("username");
                    DataSnapshot highscoreSnapshot = userSnapshot.Child("highscore");

                    if (usernameSnapshot.Exists && highscoreSnapshot.Exists)
                    {
                        string username = usernameSnapshot.Value.ToString();
                        int highscore = int.Parse(highscoreSnapshot.Value.ToString());
                        leaderboard.AddScore(username, highscore);
                    }
                }
            }
        }
        else
        {
            Debug.LogError("Failed to get all highscores from database: " + task.Exception);
        }
    }

#endif

}
