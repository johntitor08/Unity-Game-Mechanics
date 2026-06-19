#if !UNITY_WEBGL

using Firebase.Database;

#else

using UnityEngine.Networking;

#endif

using System.Collections;
using UnityEngine;

public class UserManager : MonoBehaviour
{
    private const string DbUrl = "https://below-hell-f2f0f-default-rtdb.firebaseio.com";

#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#else

    public string IdToken { get; set; }

#endif

    public User user;

    public bool IsAuthenticated
    {
        get
        {
#if UNITY_WEBGL
            return !string.IsNullOrEmpty(IdToken);
#else
            return Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null;
#endif
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

#if !UNITY_WEBGL
    public void Initialize(DatabaseReference dbRef)
    {
        databaseReference = dbRef;
    }
#endif

    public void SetOnline(User user)
    {
        this.user = user;

#if UNITY_WEBGL

        StartCoroutine(SetOnlineWebGL());

#else

        if (databaseReference == null)
        {
            Debug.LogError("UserManager: databaseReference is null — was Initialize() called?");
            return;
        }

        databaseReference.Child("Users").Child(user.userID).Child("isOnline").SetValueAsync(true);
        databaseReference.Child("Users").Child(user.userID).Child("isOnline").OnDisconnect().SetValue(false);

#endif
    }

#if UNITY_WEBGL

    private IEnumerator SetOnlineWebGL()
    {
        if (user == null) yield break;
        string url = $"{DbUrl}/Users/{user.userID}/isOnline.json";
        byte[] body = System.Text.Encoding.UTF8.GetBytes("true");
        using UnityWebRequest req = new(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
    }

#endif

    public void SetOfflineInstant()
    {
        if (user == null) return;

#if UNITY_WEBGL

        StartCoroutine(SetOfflineWebGL());

#else

        if (databaseReference == null) return;
        databaseReference.Child("Users").Child(user.userID).Child("isOnline").SetValueAsync(false);

#endif

        user = null;
    }

#if UNITY_WEBGL

    private IEnumerator SetOfflineWebGL()
    {
        if (user == null) yield break;
        string url = $"{DbUrl}/Users/{user.userID}/isOnline.json";
        byte[] body = System.Text.Encoding.UTF8.GetBytes("false");
        using UnityWebRequest req = new(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(body);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        yield return req.SendWebRequest();
    }

#endif

    private void OnApplicationQuit()
    {
        SetOfflineInstant();
    }
}
