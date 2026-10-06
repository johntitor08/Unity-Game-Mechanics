using UnityEngine;

public class AspectLock : MonoBehaviour
{
    private const float Target = 16f / 9f;
    private const float Tolerance = 0.01f;
    private const float SettleSeconds = 0.3f;
    private static AspectLock instance;
    private int _lastW, _lastH;
    private FullScreenMode _lastMode;
    private float _settleAt = -1f;
    private int _requestedFromW, _requestedFromH;
    private FullScreenMode _requestedFromMode;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Create()
    {
        if (instance != null || Application.isMobilePlatform)
            return;

        var go = new GameObject("AspectLock");
        DontDestroyOnLoad(go);
        instance = go.AddComponent<AspectLock>();
    }

    public static bool Is16x9(int width, int height) => height > 0 && Mathf.Abs((float)width / height - Target) / Target <= Tolerance;

    public static Vector2Int Fit(int width, int height)
    {
        int w = Mathf.Min(width, Mathf.RoundToInt(height * Target));
        int h = Mathf.RoundToInt(w / Target);
        return new Vector2Int(w, h);
    }

    void Start()
    {
        Remember();
        Enforce();
    }

    void Update()
    {
        if (Screen.width != _lastW || Screen.height != _lastH || Screen.fullScreenMode != _lastMode)
        {
            Remember();
            _settleAt = Time.unscaledTime + SettleSeconds;
            return;
        }

        if (_settleAt >= 0f && Time.unscaledTime >= _settleAt)
        {
            _settleAt = -1f;
            Enforce();
        }
    }

    void Remember()
    {
        _lastW = Screen.width;
        _lastH = Screen.height;
        _lastMode = Screen.fullScreenMode;
    }

    void Enforce()
    {
        int w = Screen.width, h = Screen.height;
        var mode = Screen.fullScreenMode;

        if (Is16x9(w, h))
            return;

        if (w == _requestedFromW && h == _requestedFromH && mode == _requestedFromMode)
            return;

        _requestedFromW = w;
        _requestedFromH = h;
        _requestedFromMode = mode;
        var fit = Fit(w, h);
        Screen.SetResolution(fit.x, fit.y, mode);
    }
}
