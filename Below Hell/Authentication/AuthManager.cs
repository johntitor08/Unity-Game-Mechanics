#if !UNITY_WEBGL

using Firebase;
using Firebase.Database;
using Firebase.Auth;
using System.Threading.Tasks;

#else

using UnityEngine.Networking;

#endif

using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AuthManager : MonoBehaviour
{
    private static readonly WaitForSeconds _waitForSeconds10 = new(10);
    private const string ApiKey = "AIzaSyAKE3Xlx6Ni5z3WRRLm0en8UBPyH495Gts";
    private const string DbUrl = "https://below-hell-f2f0f-default-rtdb.firebaseio.com";

#if !UNITY_WEBGL

    private DatabaseReference databaseReference;
    private FirebaseAuth auth;

#else

    private string _idToken;

#endif

    private UserManager userManager;
    private User currentUser;

    [Header("UI Inputs")]
    public TMP_InputField usernameRegisterInput;
    public TMP_InputField passwordRegisterInput;
    public TMP_InputField repasswordRegisterInput;
    public TMP_InputField usernameLoginInput;
    public TMP_InputField passwordLoginInput;

    [Header("UI Buttons")]
    public Button registerButton;
    public Button loginButton;

    private void Start()
    {
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();

#if !UNITY_WEBGL

        Firebase.FirebaseApp.LogLevel = Firebase.LogLevel.Error;

#endif

        SetUIInteractable(false);
        StartCoroutine(InitializeFirebase());
    }

    private void SetUIInteractable(bool state)
    {
        if (registerButton != null) registerButton.interactable = state;
        if (loginButton != null) loginButton.interactable = state;
    }

    private IEnumerator InitializeFirebase()
    {
#if !UNITY_WEBGL

        var initTask = FirebaseApp.CheckAndFixDependenciesAsync();
        yield return WaitForTaskSafe(initTask);

        if (initTask.Result != DependencyStatus.Available)
        {
            Debug.LogError("Firebase dependencies not available!");
            yield break;
        }

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;
        auth = FirebaseAuth.DefaultInstance;
        userManager.Initialize(databaseReference);

#else

        yield return null;

#endif

        Debug.Log("Firebase initialized successfully.");
        SetUIInteractable(true);
    }

    public void OnClickRegisterButton() => StartCoroutine(UserRegister());

    public void OnClickLoginButton() => StartCoroutine(UserLogin());

#if UNITY_WEBGL

    [Serializable]
    private class AuthResponse
    {
        public string localId;
        public string idToken;
        public string refreshToken;
        public string expiresIn;
    }

    private IEnumerator UserRegister()
    {
        string username = usernameRegisterInput.text.Trim().ToLower();
        string password = passwordRegisterInput.text.Trim();
        string repassword = repasswordRegisterInput.text.Trim();

        if (username.Length < 3 || username.Length > 12 ||
            password.Length < 6 || password.Length > 20 ||
            password != repassword)
        {
            Debug.LogError("Invalid input");
            yield break;
        }

        string checkUrl = $"{DbUrl}/Usernames/{username}.json";
        using UnityWebRequest checkReq = UnityWebRequest.Get(checkUrl);
        yield return checkReq.SendWebRequest();

        if (checkReq.result == UnityWebRequest.Result.Success &&
            checkReq.downloadHandler.text != "null")
        {
            Debug.LogError("Username already taken!");
            yield break;
        }

        string email = username + "@game.com";
        string signUpUrl = $"https://identitytoolkit.googleapis.com/v1/accounts:signUp?key={ApiKey}";
        string signUpBody = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";
        using UnityWebRequest signUpReq = PostJson(signUpUrl, signUpBody);
        yield return signUpReq.SendWebRequest();

        if (signUpReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Register failed: " + signUpReq.downloadHandler.text);
            yield break;
        }

        AuthResponse authResp = JsonUtility.FromJson<AuthResponse>(signUpReq.downloadHandler.text);

        if (string.IsNullOrEmpty(authResp.localId) || string.IsNullOrEmpty(authResp.idToken))
        {
            Debug.LogError("Register response missing uid/token: " + signUpReq.downloadHandler.text);
            yield break;
        }

        string uid = authResp.localId;
        string idToken = authResp.idToken;
        _idToken = idToken;
        userManager.IdToken = idToken;
        User newUser = new(username, uid, DateTime.Now.ToString(), true);
        string userJson = JsonUtility.ToJson(newUser);
        string saveUserUrl = $"{DbUrl}/Users/{uid}.json?auth={idToken}";
        using UnityWebRequest saveUserReq = PutJson(saveUserUrl, userJson);
        yield return saveUserReq.SendWebRequest();

        if (saveUserReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to save user data: " + saveUserReq.downloadHandler.text);
            yield break;
        }

        string saveNameUrl = $"{DbUrl}/Usernames/{username}.json?auth={idToken}";
        using UnityWebRequest saveNameReq = PutJson(saveNameUrl, $"\"{uid}\"");
        yield return saveNameReq.SendWebRequest();

        if (saveNameReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to save username mapping: " + saveNameReq.downloadHandler.text);
            yield break;
        }

        FinishLogin(newUser);
    }

#else

    private IEnumerator UserRegister()
    {
        string username = usernameRegisterInput.text.Trim().ToLower();
        string password = passwordRegisterInput.text.Trim();
        string repassword = repasswordRegisterInput.text.Trim();

        if (username.Length < 3 || username.Length > 12 ||
            password.Length < 6 || password.Length > 20 ||
            password != repassword)
        {
            Debug.LogError("Invalid input");
            yield break;
        }

        string email = username + "@game.com";
        var usernameCheck = databaseReference.Child("Usernames").Child(username).GetValueAsync();
        yield return WaitForTaskSafe(usernameCheck);

        if (usernameCheck.Result.Exists)
        {
            Debug.LogError("Username already taken!");
            yield break;
        }

        var registerTask = auth.CreateUserWithEmailAndPasswordAsync(email, password);
        yield return WaitForTaskSafe(registerTask);

        if (registerTask.Exception != null)
        {
            Debug.LogError(registerTask.Exception);
            yield break;
        }

        string uid = registerTask.Result.User.UserId;
        User newUser = new(username, uid, DateTime.Now.ToString(), true);
        string json = JsonUtility.ToJson(newUser);
        var saveUser = databaseReference.Child("Users").Child(uid).SetRawJsonValueAsync(json);
        yield return WaitForTaskSafe(saveUser);
        var saveUsername = databaseReference.Child("Usernames").Child(username).SetValueAsync(uid);
        yield return WaitForTaskSafe(saveUsername);
        FinishLogin(newUser);
    }

#endif

#if UNITY_WEBGL

    private IEnumerator UserLogin()
    {
        string username = usernameLoginInput.text.Trim().ToLower();
        string password = passwordLoginInput.text.Trim();
        string email = username + "@game.com";
        string getUIDUrl = $"{DbUrl}/Usernames/{username}.json";
        using UnityWebRequest getUIDReq = UnityWebRequest.Get(getUIDUrl);
        yield return getUIDReq.SendWebRequest();

        if (getUIDReq.result != UnityWebRequest.Result.Success ||
            getUIDReq.downloadHandler.text == "null")
        {
            Debug.LogError("User not found!");
            yield break;
        }

        string signInUrl  = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithPassword?key={ApiKey}";
        string signInBody = $"{{\"email\":\"{email}\",\"password\":\"{password}\",\"returnSecureToken\":true}}";
        using UnityWebRequest signInReq = PostJson(signInUrl, signInBody);
        yield return signInReq.SendWebRequest();

        if (signInReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Wrong password! Response: " + signInReq.downloadHandler.text);
            yield break;
        }

        AuthResponse authResp = JsonUtility.FromJson<AuthResponse>(signInReq.downloadHandler.text);

        if (string.IsNullOrEmpty(authResp.localId) || string.IsNullOrEmpty(authResp.idToken))
        {
            Debug.LogError("Sign-in response missing uid/token: " + signInReq.downloadHandler.text);
            yield break;
        }

        string uid = authResp.localId;
        string idToken = authResp.idToken;
        _idToken = idToken;
        userManager.IdToken = idToken;
        string userDataUrl = $"{DbUrl}/Users/{uid}.json?auth={idToken}";
        using UnityWebRequest userDataReq = UnityWebRequest.Get(userDataUrl);
        yield return userDataReq.SendWebRequest();

        if (userDataReq.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError("Failed to load user data: " + userDataReq.downloadHandler.text);
            yield break;
        }

        User user = JsonUtility.FromJson<User>(userDataReq.downloadHandler.text);
        FinishLogin(user);
    }

#else

    private IEnumerator UserLogin()
    {
        string username = usernameLoginInput.text.Trim().ToLower();
        string password = passwordLoginInput.text.Trim();
        var getUID = databaseReference.Child("Usernames").Child(username).GetValueAsync();
        yield return WaitForTaskSafe(getUID);

        if (!getUID.Result.Exists)
        {
            Debug.LogError("User not found!");
            yield break;
        }

        string email = username + "@game.com";
        var loginTask = auth.SignInWithEmailAndPasswordAsync(email, password);
        yield return WaitForTaskSafe(loginTask);

        if (loginTask.Exception != null)
        {
            Debug.LogError("Wrong password!");
            yield break;
        }

        string uid = loginTask.Result.User.UserId;
        var userData = databaseReference.Child("Users").Child(uid).GetValueAsync();
        yield return WaitForTaskSafe(userData);
        User user = JsonUtility.FromJson<User>(userData.Result.GetRawJsonValue());
        FinishLogin(user);
    }

#endif

    private void FinishLogin(User user)
    {
        currentUser = user;
        userManager.SetOnline(user);
        StartCoroutine(UserOnline(user));
        StartCoroutine(Heartbeat());
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator UserOnline(User user)
    {
#if UNITY_WEBGL

        string url = $"{DbUrl}/Users/{user.userID}/isOnline.json?auth={_idToken}";
        using UnityWebRequest req = UnityWebRequest.Put(url, "true");
        yield return req.SendWebRequest();

#else

        var task = databaseReference.Child("Users").Child(user.userID).Child("isOnline").SetValueAsync(true);
        yield return WaitForTaskSafe(task);

#endif
    }

    private IEnumerator Heartbeat()
    {
        while (true)
        {
            yield return _waitForSeconds10;
            if (currentUser == null) continue;

#if UNITY_WEBGL

            string url = $"{DbUrl}/Users/{currentUser.userID}/lastSeen.json?auth={_idToken}";

            using UnityWebRequest req = UnityWebRequest.Put(
                url,
                DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString()
            );

            yield return req.SendWebRequest();

#else

            var task = databaseReference.Child("Users").Child(currentUser.userID).Child("lastSeen").SetValueAsync(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            yield return WaitForTaskSafe(task);

#endif
        }
    }

    private IEnumerator SetOffline()
    {
        if (currentUser == null) yield break;

#if UNITY_WEBGL

        string url = $"{DbUrl}/Users/{currentUser.userID}/isOnline.json?auth={_idToken}";
        using UnityWebRequest req = UnityWebRequest.Put(url, "false");
        yield return req.SendWebRequest();

#else

        var task = databaseReference.Child("Users").Child(currentUser.userID).Child("isOnline").SetValueAsync(false);
        yield return WaitForTaskSafe(task);

#endif
    }

    private void OnApplicationQuit()
    {
        if (currentUser != null)
            StartCoroutine(SetOffline());
    }

#if !UNITY_WEBGL

    private IEnumerator WaitForTaskSafe(Task task, float timeout = 10f)
    {
        float timer = 0f;

        while (!task.IsCompleted && timer < timeout)
        {
            timer += Time.deltaTime;
            yield return null;
        }

        if (!task.IsCompleted)
        {
            Debug.LogError("Firebase task timed out!");
            yield break;
        }

        if (task.Exception != null)
        {
            Debug.LogError("Firebase task exception: " +
                           task.Exception.Flatten().InnerException?.Message);
            yield break;
        }
    }

#endif

#if UNITY_WEBGL

    private static UnityWebRequest PostJson(string url, string jsonBody)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest req = new(url, "POST");
        req.uploadHandler   = new UploadHandlerRaw(bytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }

    private static UnityWebRequest PutJson(string url, string jsonBody)
    {
        byte[] bytes = System.Text.Encoding.UTF8.GetBytes(jsonBody);
        UnityWebRequest req = new(url, "PUT");
        req.uploadHandler   = new UploadHandlerRaw(bytes);
        req.downloadHandler = new DownloadHandlerBuffer();
        req.SetRequestHeader("Content-Type", "application/json");
        return req;
    }

#endif
}
