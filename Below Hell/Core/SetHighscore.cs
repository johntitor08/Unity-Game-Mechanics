#if !UNITY_WEBGL

using Firebase.Database;

#else

using UnityEngine.Networking;

#endif

using System.Collections;
using UnityEngine;

public class SetHighscore : MonoBehaviour
{
#if UNITY_WEBGL

    public IEnumerator SaveHighscoreToDatabase(User user, string idToken)
    {
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + user.userID + "/highscore.json?auth=" + idToken;
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(user.highscore.ToString());
        using UnityWebRequest putRequest = new UnityWebRequest(url, "PUT");
        putRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        putRequest.downloadHandler = new DownloadHandlerBuffer();
        putRequest.SetRequestHeader("Content-Type", "application/json");
        yield return putRequest.SendWebRequest();

        if (putRequest.result != UnityWebRequest.Result.Success)
            Debug.LogError("Failed to save highscore to database: " + putRequest.error);
    }

    public IEnumerator GetHighscoreFromDatabase(User user, string idToken)
    {
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + user.userID + "/highscore.json?auth=" + idToken;
        using UnityWebRequest getRequest = UnityWebRequest.Get(url);
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.Success)
        {
            string response = getRequest.downloadHandler.text;
            if (response != "null" && int.TryParse(response, out int highscore))
                user.highscore = highscore;
        }
        else
        {
            Debug.LogError("Failed to get highscore from database: " + getRequest.error);
        }
    }

#else

    public IEnumerator SaveHighscoreToDatabase(DatabaseReference databaseReference, User user)
    {
        if (databaseReference == null) { Debug.LogError("DatabaseReference is null"); yield break; }
        var task = databaseReference.Child("Users").Child(user.userID).Child("highscore").SetValueAsync(user.highscore);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to save highscore to database: " + inner.Message);
        }
    }

    public IEnumerator GetHighscoreFromDatabase(DatabaseReference databaseReference, User user)
    {
        if (databaseReference == null) { Debug.LogError("DatabaseReference is null"); yield break; }
        var task = databaseReference.Child("Users").Child(user.userID).Child("highscore").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            DataSnapshot snapshot = task.Result;

            if (snapshot != null && snapshot.Exists)
                user.highscore = int.Parse(snapshot.Value.ToString());
        }
        else
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to get highscore from database: " + inner.Message);
        }
    }

#endif
}
