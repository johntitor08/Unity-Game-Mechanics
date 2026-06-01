#if !UNITY_WEBGL

using Firebase.Database;

#else

using UnityEngine.Networking;

#endif

using SimpleFileBrowser;
using System.Collections;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SetProfileImage : MonoBehaviour
{
#if !UNITY_WEBGL

    private DatabaseReference databaseReference;

#endif

    private UserManager userManager;
    [SerializeField] private RawImage profileImage;
    [SerializeField] private TMP_Text selectText;
    private Texture2D texture2D;
    private byte[] fileContent;

    private void Start()
    {
#if !UNITY_WEBGL

        databaseReference = FirebaseDatabase.DefaultInstance.RootReference;

#endif
        userManager = GameObject.Find("UserData").GetComponent<UserManager>();
        StartCoroutine(GetProfileImageFromDatabase());
        FileBrowser.SetFilters(true, new FileBrowser.Filter("Images", ".jpg", ".png"));
        FileBrowser.SetDefaultFilter(".jpg");
        FileBrowser.SetExcludedExtensions(".lnk", ".tmp", ".zip", ".rar", ".exe");
        FileBrowser.AddQuickLink("Users", "C:\\Users", null);
    }

    private IEnumerator DisplayProfileImage()
    {
        yield return FileBrowser.WaitForLoadDialog(FileBrowser.PickMode.FilesAndFolders, true, null, null, "Load Files and Folders", "Load");

        if (FileBrowser.Success)
        {
            fileContent = FileBrowserHelpers.ReadBytesFromFile(FileBrowser.Result[0]);
            string destinationPath = Path.Combine(Application.persistentDataPath, FileBrowserHelpers.GetFilename(FileBrowser.Result[0]));
            FileBrowserHelpers.CopyFile(FileBrowser.Result[0], destinationPath);
            texture2D = new Texture2D(2, 2);
            texture2D.LoadImage(fileContent);
            profileImage.texture = texture2D;
            selectText.gameObject.SetActive(false);
            StartCoroutine(SaveProfileImageToDatabase());
        }
    }

    public void SelectProfileImage() => StartCoroutine(DisplayProfileImage());

    private byte[] CompressImage(byte[] imageBytes, int quality)
    {
        Texture2D texture = new(2, 2);
        texture.LoadImage(imageBytes);
        byte[] compressedBytes = texture.EncodeToJPG(quality);
        Destroy(texture);
        return compressedBytes;
    }

    private void LoadBase64Image(string base64)
    {
        byte[] imageBytes = System.Convert.FromBase64String(base64);
        texture2D = new Texture2D(2, 2);
        texture2D.LoadImage(imageBytes);
        profileImage.texture = texture2D;
        selectText.gameObject.SetActive(false);
    }

#if !UNITY_WEBGL

    private IEnumerator SaveProfileImageToDatabase()
    {
        byte[] imageBytes = CompressImage(fileContent, 80);
        string base64 = System.Convert.ToBase64String(imageBytes);
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("profileImage").SetValueAsync(base64);
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception != null)
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to save profile image: " + inner.Message);
        }
        else
        {
            userManager.user.profileImage = base64;
        }
    }

    private IEnumerator GetProfileImageFromDatabase()
    {
        var task = databaseReference.Child("Users").Child(userManager.user.userID).Child("profileImage").GetValueAsync();
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.Exception == null)
        {
            if (task.Result != null && task.Result.Exists && !string.IsNullOrEmpty(task.Result.Value.ToString()))
                LoadBase64Image(task.Result.Value.ToString());
            else
            {
                profileImage.texture = null;
                selectText.gameObject.SetActive(true);
            }
        }
        else
        {
            var inner = task.Exception.InnerException ?? task.Exception;
            Debug.LogError("Failed to get profile image: " + inner.Message);
        }
    }

#else

    private IEnumerator SaveProfileImageToDatabase()
    {
        byte[] imageBytes = CompressImage(fileContent, 80);
        string base64 = System.Convert.ToBase64String(imageBytes);
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + userManager.user.userID + "/profileImage.json?auth=" + userManager.IdToken;
        byte[] jsonBytes = System.Text.Encoding.UTF8.GetBytes("\"" + base64 + "\"");
        using UnityWebRequest putRequest = new UnityWebRequest(url, "PUT");
        putRequest.uploadHandler = new UploadHandlerRaw(jsonBytes);
        putRequest.downloadHandler = new DownloadHandlerBuffer();
        putRequest.SetRequestHeader("Content-Type", "application/json");
        yield return putRequest.SendWebRequest();

        if (putRequest.result == UnityWebRequest.Result.Success)
            userManager.user.profileImage = base64;
        else
            Debug.LogError("Failed to save profile image: " + putRequest.error);
    }

    private IEnumerator GetProfileImageFromDatabase()
    {
        string url = "https://below-hell-f2f0f-default-rtdb.firebaseio.com/Users/" + userManager.user.userID + "/profileImage.json?auth=" + userManager.IdToken;
        using UnityWebRequest getRequest = UnityWebRequest.Get(url);
        yield return getRequest.SendWebRequest();

        if (getRequest.result == UnityWebRequest.Result.Success)
        {
            string response = getRequest.downloadHandler.text;
            if (response != "null" && !string.IsNullOrEmpty(response))
                LoadBase64Image(response.Trim('"'));
            else
            {
                profileImage.texture = null;
                selectText.gameObject.SetActive(true);
            }
        }
        else
        {
            Debug.LogError("Failed to get profile image: " + getRequest.error);
        }
    }

#endif
}
