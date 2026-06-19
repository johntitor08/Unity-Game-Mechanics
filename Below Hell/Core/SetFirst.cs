#if !UNITY_WEBGL

using Firebase.Database;

#else

using UnityEngine.Networking;

#endif

using System.Collections;
using UnityEngine;

public class SetFirst : MonoBehaviour
{
#if UNITY_WEBGL

    public IEnumerator SaveFirstToDatabase(User user, string idToken)
    {
        string firstUrl = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + user.userID + "/first.json?auth=" + idToken;
        byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(user.first.ToString());
        using UnityWebRequest putRequest = new UnityWebRequest(firstUrl, "PUT");
        putRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
        putRequest.downloadHandler = new DownloadHandlerBuffer();
        putRequest.SetRequestHeader("Content-Type", "application/json");
        yield return putRequest.SendWebRequest();

        if (putRequest.result != UnityWebRequest.Result.Success)
            Debug.LogError("Failed to save first to database: " + putRequest.error);
    }

    public IEnumerator GetFirstFromDatabase(User user, string idToken)
    {
        string firstUrl = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + user.userID + "/first.json?auth=" + idToken;
        using UnityWebRequest getRequest = UnityWebRequest.Get(firstUrl);
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.Success)
        {
            string response = getRequest.downloadHandler.text;
            if (response != "null" && int.TryParse(response, out int first))
                user.first = first;
        }
        else
        {
            Debug.LogError("Failed to get first from database: " + getRequest.error);
        }
    }

#else

    public IEnumerator SaveFirstToDatabase(DatabaseReference databaseReference, User user)
    {
        if (databaseReference == null) { Debug.LogError("DatabaseReference is null"); yield break; }
        var task = databaseReference.Child("Users").Child(user.userID).Child("first").SetValueAsync(user.first);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to save first to database: " + inner.Message);
        }
    }

    public IEnumerator GetFirstFromDatabase(DatabaseReference databaseReference, User user)
    {
        if (databaseReference == null) { Debug.LogError("DatabaseReference is null"); yield break; }
        var task = databaseReference.Child("Users").Child(user.userID).Child("first").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            DataSnapshot snapshot = task.Result;
            if (snapshot != null && snapshot.Exists)
                user.first = int.Parse(snapshot.Value.ToString());
        }
        else
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to get first from database: " + inner.Message);
        }
    }

#endif
}
