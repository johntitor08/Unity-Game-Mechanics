#if UNITY_EDITOR || DEVELOPMENT_BUILD

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR

using UnityEditor.Recorder;
using UnityEditor.Recorder.Input;

#endif

public class TeaserDirector : MonoBehaviour
{
    private static WaitForSecondsRealtime _waitForSecondsRealtime0_2 = new(0.2f);
    private static WaitForSecondsRealtime _waitForSecondsRealtime0_8 = new(0.8f);
    private static WaitForSecondsRealtime _waitForSecondsRealtime0_4 = new(0.4f);
    private static WaitForSecondsRealtime _waitForSecondsRealtime1 = new(1f);
    public EnemyData combatEnemy;
    public DialogueNode choiceNode;
    public DialogueNode vossNode;
    public string scenarioID = "ashenveil_day3";
    public DialogueNode serenaNode;
    public List<ItemData> inventoryItems = new();
    public AudioClip music;
    [Range(0f, 1f)]
    public float musicVolume = 0.85f;
    public float musicStartAt = 0f;
    public Sprite logo;
    public TMPro.TMP_FontAsset cardFont;
    public Color cardColor = new(0.88f, 0.74f, 0.47f);
    public string presentsCard = "noraStudios presents";
    public string nightCard = "In Ashenveil, the night listens.";
    public string mapCard = "A town that keeps its secrets.";
    public string choiceCard = "Every word is remembered.";
    public string combatCard = "Face what hunts the dark.";
    public string vossCard = "Bound by a contract older than the town.";
    public string endLine = "Wishlist now on Steam";
    public string musicCredit = "Music: \"Five Armies\" by Kevin MacLeod (incompetech.com) — CC BY 4.0";
    public int fightHealthBonus = 400;
    public List<GameObject> hiddenCanvases = new();
    public string[] hiddenCanvasNames = { "IconCanvas", "TimeCanvas", "CurrencyCanvas", "ForegroundCanvas" };
    public float barAspect = 2.39f;
    public bool useBars = false;
    public bool singleTake = true;
    public string outputFolder = "Recordings";
    public int width = 1920;
    public int height = 1080;
    public int frameRate = 60;
    private static readonly float[] TimeScales = { 1f, 0.5f, 0.25f };
    private int timeScaleIndex;
    private bool hudHidden;
    private GameObject barsRoot;
    private bool shooting;
    private bool fightBoosted;
    private Canvas overlay;
    private UnityEngine.UI.Image fader;
    private TMPro.TextMeshProUGUI cardText;
    private UnityEngine.UI.Image logoImage;
    private TMPro.TextMeshProUGUI endText;
    private TMPro.TextMeshProUGUI creditText;
    private AudioSource musicSource;
    private float savedTypeVolume = -1f;

    [Header("Keys")]
    public KeyCode nightKey = KeyCode.F1;
    public KeyCode scenarioKey = KeyCode.F2;
    public KeyCode combatKey = KeyCode.F3;
    public KeyCode choiceKey = KeyCode.F4;
    public KeyCode finaleKey = KeyCode.F5;
    public KeyCode shootAllKey = KeyCode.F6;
    public KeyCode languageKey = KeyCode.L;
    public KeyCode hudKey = KeyCode.H;
    public KeyCode barsKey = KeyCode.B;
    public KeyCode slowerKey = KeyCode.LeftBracket;
    public KeyCode fasterKey = KeyCode.RightBracket;
    public KeyCode recordKey = KeyCode.R;

    #if UNITY_EDITOR

    private RecorderController recorder;

    #endif

    void Update()
    {
        if (Input.GetKeyDown(nightKey))
            NightMode();

        if (Input.GetKeyDown(scenarioKey))
            PlayVoss();

        if (Input.GetKeyDown(combatKey))
            StartFight();

        if (Input.GetKeyDown(choiceKey))
            ShowChoice();

        if (Input.GetKeyDown(finaleKey))
            PlayFinale();

        if (Input.GetKeyDown(shootAllKey))
            StartShoot();

        if (Input.GetKeyDown(languageKey))
            ToggleLanguage();

        if (Input.GetKeyDown(hudKey))
            ToggleHud();

        if (Input.GetKeyDown(barsKey))
            ToggleBars();

        if (Input.GetKeyDown(slowerKey))
            StepTimeScale(1);

        if (Input.GetKeyDown(fasterKey))
            StepTimeScale(-1);

        if (Input.GetKeyDown(recordKey))
            ToggleRecording();
    }

    public void StartShoot()
    {
        if (shooting)
        {
            Debug.LogWarning("[Teaser] A shoot is already running.");
            return;
        }

        StartCoroutine(ShootAll());
    }

    public void StartScreenshots()
    {
        if (shooting)
        {
            Debug.LogWarning("[Teaser] A shoot is already running.");
            return;
        }

        StartCoroutine(ShootScreenshots());
    }

    IEnumerator ShootScreenshots()
    {
        shooting = true;
        Debug.Log("[Teaser] Screenshots started. Leave the Unity window in front until it reports done.");
        LanguageManager.SetLanguage(GameLanguage.EN);

        if (!hudHidden)
            ToggleHud();

        if (barsRoot != null)
            ToggleBars();

        yield return Still("01_night", () => NightMode());
        yield return Still("02_voss", () => PlayVoss(), advances: 2);
        yield return Still("03_combat", () => StartFight(), attacks: 3);
        LeaveCombat();
        yield return _waitForSecondsRealtime1;
        yield return Still("04_choice", () => ShowChoice(), advances: 6, allowChoices: true);
        yield return Still("05_finale", () => PlayFinale(), advances: 2);
        shooting = false;
        Debug.Log($"[Teaser] Screenshots done. Output is in {ScreenshotFolder()}");
    }

    IEnumerator Still(string label, System.Action setup, int advances = 0, int attacks = 0, bool allowChoices = false)
    {
        Resume();

        if (CloseDialogue())
            yield return WaitForDialogueToClose();

        setup?.Invoke();
        yield return _waitForSecondsRealtime1;

        for (int i = 0; i < advances; i++)
        {
            AdvanceDialogue(allowChoices);
            yield return _waitForSecondsRealtime0_4;
        }

        for (int i = 0; i < attacks; i++)
        {
            AttackOnce();
            yield return _waitForSecondsRealtime0_8;
        }

        var dialogue = DialogueManager.Instance;

        if (dialogue != null && dialogue.State == DialogueState.Typing && dialogue.typewriter != null && dialogue.typewriter.IsTyping)
            dialogue.typewriter.Complete();

        yield return _waitForSecondsRealtime1;
        string path = System.IO.Path.Combine(ScreenshotFolder(), label + ".png");
        ScreenCapture.CaptureScreenshot(path);

        for (int i = 0; i < 10 && !System.IO.File.Exists(path); i++)
            yield return _waitForSecondsRealtime0_2;

        Debug.Log($"[Teaser] Still {label} -> {(System.IO.File.Exists(path) ? "written" : "MISSING")}. {DescribeShot()}");
    }

    string ScreenshotFolder()
    {
        string folder = System.IO.Path.Combine(
            System.IO.Directory.GetParent(Application.dataPath).FullName, "Screenshots");

        if (!System.IO.Directory.Exists(folder))
            System.IO.Directory.CreateDirectory(folder);

        return folder;
    }

    IEnumerator ShootAll()
    {
        shooting = true;
        Debug.Log("[Teaser] Shoot started. Leave the Unity window in front until it reports done.");
        LanguageManager.SetLanguage(GameLanguage.EN);

        if (!hudHidden)
            ToggleHud();

        if (useBars == (barsRoot == null))
            ToggleBars();

        EnsureOverlay();
        SetFader(1f);
        BeginTrailerAudio();
        StockInventory();

        if (singleTake)
            StartRecording("teaser_full");

        yield return Card(presentsCard, 2.2f, small: true);
        yield return Card(nightCard, 2.6f);
        yield return Clip("01_night", 6f, () => NightMode(), zoom: BackgroundImage(), zoomTo: 1.08f);
        yield return Dip();
        yield return Clip("02_serena", 8f, () => { DayMode(); StartNode("02_serena", serenaNode); }, advanceEvery: 2.2f);
        yield return Card(mapCard, 2.2f);
        yield return Clip("03_map", 5f, () => OpenMap(true), zoom: MapImage(), zoomTo: 1.12f);
        OpenMap(false);
        yield return Dip();
        yield return Clip("04_inventory", 4.5f, () => OpenInventory(true));
        OpenInventory(false);
        yield return Card(choiceCard, 2.2f);
        yield return Clip("05_choice", 7f, () => ShowChoice(), advanceEvery: 1f, allowChoices: true);
        yield return Card(combatCard, 2.2f, underneath: () => { CloseDialogue(); StartFight(); });
        yield return Clip("06_combat", 11f, null, attackEvery: 2.2f, firstAttackAt: 1f);
        LeaveCombat();
        yield return Card(vossCard, 2.6f);
        yield return Clip("07_voss", 12f, () => PlayVoss(), advanceEvery: 3.2f, switchLanguageAt: 6f, restoreLanguageAt: 9.5f);
        yield return Dip();
        yield return Clip("08_finale", 8f, () => PlayFinale(), advanceEvery: 2f);
        yield return EndCard(6.5f);

        if (singleTake)
            StopRecording();

        EndTrailerAudio();
        SetFader(0f);
        HideOverlayText();
        shooting = false;
        Debug.Log($"[Teaser] Shoot done. Output is in {FolderPath()}");
    }

    static IEnumerator Wait(float seconds)
    {
        float t = 0f;

        while (t < seconds)
        {
            yield return null;
            t += Time.unscaledDeltaTime;
        }
    }

    void EnsureOverlay()
    {
        if (overlay != null)
            return;

        var root = new GameObject("__TeaserOverlay", typeof(Canvas), typeof(UnityEngine.UI.CanvasScaler));
        root.transform.SetParent(transform, false);
        overlay = root.GetComponent<Canvas>();
        overlay.renderMode = RenderMode.ScreenSpaceOverlay;
        overlay.sortingOrder = 32000;
        var scaler = root.GetComponent<UnityEngine.UI.CanvasScaler>();
        scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f;
        fader = MakeImage(root.transform, "Fader", Vector2.zero, Vector2.one, Vector2.zero);
        fader.color = new Color(0f, 0f, 0f, 0f);
        cardText = MakeText(root.transform, "Card", new Vector2(0f, 0f), new Vector2(1600, 300), 60f);
        logoImage = MakeImage(root.transform, "Logo", new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(1150, 363));
        ((RectTransform)logoImage.transform).anchoredPosition = new Vector2(0, 90);
        logoImage.sprite = logo;
        logoImage.preserveAspect = true;
        logoImage.color = new Color(1f, 1f, 1f, 0f);
        endText = MakeText(root.transform, "EndLine", new Vector2(0f, -170f), new Vector2(1600, 90), 46f);
        creditText = MakeText(root.transform, "Credit", new Vector2(0f, -470f), new Vector2(1800, 60), 22f);
        creditText.color = new Color(0.8f, 0.78f, 0.74f, 0f);
    }

    static UnityEngine.UI.Image MakeImage(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 size)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.sizeDelta = size;
        rt.anchoredPosition = Vector2.zero;
        var image = go.GetComponent<UnityEngine.UI.Image>();
        image.raycastTarget = false;
        return image;
    }

    TMPro.TextMeshProUGUI MakeText(Transform parent, string name, Vector2 position, Vector2 size, float fontSize)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TMPro.TextMeshProUGUI));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.sizeDelta = size;
        rt.anchoredPosition = position;
        var text = go.GetComponent<TMPro.TextMeshProUGUI>();

        if (cardFont != null)
            text.font = cardFont;

        text.fontSize = fontSize;
        text.alignment = TMPro.TextAlignmentOptions.Center;
        text.textWrappingMode = TMPro.TextWrappingModes.Normal;
        text.raycastTarget = false;
        text.color = new Color(cardColor.r, cardColor.g, cardColor.b, 0f);
        return text;
    }

    void SetFader(float alpha)
    {
        if (fader != null)
            fader.color = new Color(0f, 0f, 0f, alpha);
    }

    static IEnumerator FadeGraphic(UnityEngine.UI.Graphic g, float to, float seconds)
    {
        if (g == null)
            yield break;

        float from = g.color.a;
        float t = 0f;

        while (t < seconds)
        {
            yield return null;
            t += Time.unscaledDeltaTime;
            var c = g.color;
            g.color = new Color(c.r, c.g, c.b, Mathf.Lerp(from, to, Mathf.SmoothStep(0f, 1f, t / seconds)));
        }

        var e = g.color;
        g.color = new Color(e.r, e.g, e.b, to);
    }

    IEnumerator Card(string text, float hold, bool small = false, System.Action underneath = null)
    {
        yield return FadeGraphic(fader, 1f, 0.5f);
        underneath?.Invoke();
        cardText.text = text;
        cardText.fontSize = small ? 38f : 60f;
        yield return FadeGraphic(cardText, 1f, 0.45f);
        yield return Wait(hold);
        yield return FadeGraphic(cardText, 0f, 0.45f);
    }

    IEnumerator Dip()
    {
        yield return FadeGraphic(fader, 1f, 0.35f);
    }

    IEnumerator EndCard(float hold)
    {
        yield return FadeGraphic(fader, 1f, 0.6f);
        endText.text = endLine;
        creditText.text = musicCredit;
        StartCoroutine(FadeGraphic(logoImage, 1f, 1.2f));
        yield return Wait(0.6f);
        StartCoroutine(FadeGraphic(endText, 1f, 0.8f));
        yield return FadeGraphic(creditText, 1f, 0.8f);
        float fadeOut = Mathf.Min(3f, hold);
        yield return Wait(hold - fadeOut);
        StartCoroutine(FadeMusic(0f, fadeOut));
        yield return Wait(fadeOut);
    }

    void HideOverlayText()
    {
        foreach (var g in new UnityEngine.UI.Graphic[] { cardText, endText, creditText, logoImage })
            if (g != null)
                g.color = new Color(g.color.r, g.color.g, g.color.b, 0f);
    }

    void BeginTrailerAudio()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.playOnAwake = false;
            musicSource.loop = false;
            musicSource.ignoreListenerPause = true;
        }

        var dialogue = DialogueManager.Instance;

        if (dialogue != null && dialogue.typewriter != null && savedTypeVolume < 0f)
        {
            savedTypeVolume = dialogue.typewriter.typeSoundVolume;
            dialogue.typewriter.typeSoundVolume = 0f;
        }

        StartCoroutine(MuteGameMusicWhileShooting());

        if (music == null)
        {
            Debug.LogWarning("[Teaser] No music assigned; the take will only carry game sound.");
            return;
        }

        musicSource.clip = music;
        musicSource.time = Mathf.Clamp(musicStartAt, 0f, music.length - 1f);
        musicSource.volume = 0f;
        musicSource.Play();
        StartCoroutine(FadeMusic(musicVolume, 1.5f));
    }

    IEnumerator FadeMusic(float to, float seconds)
    {
        if (musicSource == null)
            yield break;

        float from = musicSource.volume;
        float t = 0f;

        while (t < seconds)
        {
            yield return null;
            t += Time.unscaledDeltaTime;
            musicSource.volume = Mathf.Lerp(from, to, t / seconds);
        }

        musicSource.volume = to;
    }

    IEnumerator MuteGameMusicWhileShooting()
    {
        var hidden = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var gm = GameAudioManager.Instance;
        var music = gm != null ? typeof(GameAudioManager).GetField("musicSource", hidden)?.GetValue(gm) as AudioSource : null;
        var ambience = gm != null ? typeof(GameAudioManager).GetField("ambienceSource", hidden)?.GetValue(gm) as AudioSource : null;

        while (shooting)
        {
            if (music != null)
                music.volume = 0f;

            if (ambience != null)
                ambience.volume = 0f;

            yield return null;
        }
    }

    void EndTrailerAudio()
    {
        if (musicSource != null)
            musicSource.Stop();

        var dialogue = DialogueManager.Instance;

        if (dialogue != null && dialogue.typewriter != null && savedTypeVolume >= 0f)
            dialogue.typewriter.typeSoundVolume = savedTypeVolume;

        savedTypeVolume = -1f;
        var gm = GameAudioManager.Instance;

        if (gm != null)
            typeof(GameAudioManager).GetMethod("ApplyVolumes",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)?.Invoke(gm, null);
    }

    public void DayMode()
    {
        if (TimePhaseManager.Instance != null)
            TimePhaseManager.Instance.SetPhase(TimePhase.Morning);
    }

    static Transform BackgroundImage()
    {
        var canvas = GameObject.Find("BackgroundCanvas");
        return canvas != null ? canvas.transform.Find("bg") : null;
    }

    static Transform MapPanel()
    {
        var canvas = GameObject.Find("MapCanvas");
        return canvas != null ? canvas.transform.Find("MapPanel") : null;
    }

    static Transform MapImage()
    {
        var panel = MapPanel();
        return panel != null ? panel.Find("Map") : null;
    }

    void OpenMap(bool open)
    {
        var panel = MapPanel();

        if (panel == null)
        {
            Debug.LogWarning("[Teaser] No MapCanvas/MapPanel.");
            return;
        }

        panel.gameObject.SetActive(open);
        var group = panel.GetComponent<CanvasGroup>();

        if (group != null && open)
            group.alpha = 1f;
    }

    void StockInventory()
    {
        var inventory = InventoryManager.Instance;

        if (inventory == null)
            return;

        foreach (var item in inventoryItems)
            if (item != null && inventory.GetTotalQuantity(item.itemID) == 0)
                inventory.AddItem(item, 1);
    }

    void OpenInventory(bool open)
    {
        var ui = InventoryUI.Instance;

        if (ui == null || ui.panel == null)
            return;

        ui.panel.SetActive(open);
        var group = ui.panel.GetComponent<CanvasGroup>();

        if (group != null && open)
            group.alpha = 1f;
    }

    IEnumerator Clip(string label, float seconds, System.Action setup, float advanceEvery = 0f, float switchLanguageAt = -1f, float restoreLanguageAt = -1f, float attackEvery = 0f, float firstAttackAt = 0f, bool allowChoices = false, Transform zoom = null, float zoomTo = 1f)
    {
        Resume();

        if (CloseDialogue())
            yield return WaitForDialogueToClose();

        setup?.Invoke();
        yield return null;

        if (!singleTake)
            StartRecording(label);

        if (fader != null && fader.color.a > 0f)
            StartCoroutine(FadeGraphic(fader, 0f, 0.6f));

        Vector3 zoomFrom = zoom != null ? zoom.localScale : Vector3.one;
        float elapsed = 0f;
        float nextAdvance = advanceEvery;
        float nextAttack = attackEvery > 0f ? Mathf.Max(firstAttackAt, 0.01f) : 0f;
        bool languageSwitched = switchLanguageAt < 0f;
        bool languageRestored = restoreLanguageAt < 0f;

        while (elapsed < seconds)
        {
            yield return null;
            elapsed += Time.unscaledDeltaTime;

            if (zoom != null)
                zoom.localScale = zoomFrom * Mathf.Lerp(1f, zoomTo, Mathf.SmoothStep(0f, 1f, elapsed / seconds));

            if (advanceEvery > 0f && elapsed >= nextAdvance)
            {
                AdvanceDialogue(allowChoices);
                nextAdvance += advanceEvery;
            }

            if (attackEvery > 0f && elapsed >= nextAttack)
            {
                AttackOnce();
                nextAttack += attackEvery;
            }

            if (!languageSwitched && elapsed >= switchLanguageAt)
            {
                ToggleLanguage();
                languageSwitched = true;
            }

            if (!languageRestored && elapsed >= restoreLanguageAt)
            {
                ToggleLanguage();
                languageRestored = true;
            }
        }

        if (!singleTake)
            StopRecording();

        if (zoom != null)
            zoom.localScale = zoomFrom;

        Debug.Log($"[Teaser] Clip {label} done ({seconds:F0}s). {DescribeShot()}");
    }

    void Resume()
    {
        Time.timeScale = 1f;
        timeScaleIndex = 0;
        var menu = FindAnyObjectByType<GameMenuManager>();

        if (menu != null)
            menu.ResumeGame();
    }

    void AdvanceDialogue(bool allowChoices)
    {
        var dialogue = DialogueManager.Instance;

        if (dialogue == null || !dialogue.IsInDialogue())
            return;

        if (dialogue.State == DialogueState.Choices)
            return;

        if (dialogue.State == DialogueState.Typing && dialogue.typewriter != null && dialogue.typewriter.IsTyping)
        {
            dialogue.typewriter.Complete();
            return;
        }

        if (!allowChoices && IsOnLastLine(dialogue))
            return;

        var step = typeof(DialogueManager).GetMethod("NextLine", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        step?.Invoke(dialogue, null);
    }

    static string DescribeShot()
    {
        var dialogue = DialogueManager.Instance;

        if (dialogue == null)
            return "no DialogueManager.";

        if (!dialogue.IsInDialogue())
            return "no dialogue on screen.";

        var container = typeof(DialogueManager).GetField("choicesContainer") ?.GetValue(dialogue) as Transform;
        int buttons = container != null ? container.childCount : -1;
        var node = typeof(DialogueManager) .GetField("currentNode", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance) ?.GetValue(dialogue) as DialogueNode;
        return $"'{(node != null ? node.name : "?")}' {dialogue.State}, {buttons} choice button(s) on screen.";
    }

    static bool IsOnLastLine(DialogueManager dialogue)
    {
        const System.Reflection.BindingFlags Hidden = System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance;
        var nodeField = typeof(DialogueManager).GetField("currentNode", Hidden);
        var indexField = typeof(DialogueManager).GetField("currentLineIndex", Hidden);

        if (nodeField == null || indexField == null)
            return false;

        var node = nodeField.GetValue(dialogue) as DialogueNode;

        if (node == null || node.lines == null)
            return false;

        return (int)indexField.GetValue(dialogue) >= node.lines.Length - 1;
    }

    bool CloseDialogue()
    {
        var dialogue = DialogueManager.Instance;

        if (dialogue == null || !dialogue.IsInDialogue())
            return false;

        var end = typeof(DialogueManager).GetMethod("EndDialogue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        end?.Invoke(dialogue, null);
        return true;
    }

    IEnumerator WaitForDialogueToClose(float timeout = 2f)
    {
        float waited = 0f;

        while (waited < timeout)
        {
            var dialogue = DialogueManager.Instance;

            if (dialogue == null || !dialogue.IsInDialogue())
                yield break;

            yield return null;
            waited += Time.unscaledDeltaTime;
        }

        Debug.LogWarning("[Teaser] Dialogue did not close within the timeout; the next shot may sit under it.");
    }

    void AttackOnce()
    {
        var combat = CombatManager.Instance;

        if (combat == null || !combat.IsPlayerTurn())
            return;

        var actions = combat.GetAvailableActions();

        if (actions == null || actions.Count == 0)
            return;

        combat.ExecutePlayerAction(actions[0]);
    }

    void LeaveCombat()
    {
        var combat = CombatManager.Instance;

        if (combat != null)
        {
            var end = typeof(CombatManager).GetMethod("EndCombatInternal", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (end != null)
                end.Invoke(combat, null);
            else
                combat.TryFleeAction();
        }

        if (PlayerStats.Instance != null)
        {
            if (fightBoosted)
            {
                PlayerStats.Instance.ModifyMax(StatType.MaxHealth, -fightHealthBonus);
                fightBoosted = false;
            }

            PlayerStats.Instance.FullRestore();
        }

        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (!canvas.isRootCanvas || canvas.name != "CombatCanvas")
                continue;

            var panel = canvas.transform.Find("CombatPanel");

            if (panel != null)
                panel.gameObject.SetActive(false);
        }
    }

    public void NightMode()
    {
        if (TimePhaseManager.Instance == null)
        {
            Debug.LogWarning("[Teaser] No TimePhaseManager in the scene.");
            return;
        }

        TimePhaseManager.Instance.SetPhase(TimePhase.Night);

        if (!hudHidden)
            ToggleHud();
    }

    void StartNode(string label, DialogueNode node)
    {
        if (node == null)
        {
            Debug.LogWarning($"[Teaser] {label}: no node assigned.");
            return;
        }

        var dialogue = DialogueManager.Instance;

        if (dialogue == null)
        {
            Debug.LogWarning($"[Teaser] {label}: no DialogueManager in the scene.");
            return;
        }

        if (dialogue.State != DialogueState.Idle)
        {
            Debug.LogError($"[Teaser] {label}: a dialogue is still {dialogue.State}, so this shot " + "would be dropped. The previous shot did not close cleanly.");
            return;
        }

        dialogue.StartDialogue(node);

        if (!dialogue.IsInDialogue())
            Debug.LogError($"[Teaser] {label}: '{node.name}' did not open.");
    }

    public void PlayVoss()
    {
        StartNode("02_voss", vossNode);
    }

    public void PlayScenario()
    {
        var debug = FindAnyObjectByType<ScenarioDebug>();

        if (debug == null)
        {
            Debug.LogWarning("[Teaser] No ScenarioDebug in the scene; cannot force a scenario.");
            return;
        }

        debug.ForceStart(scenarioID);
    }

    public void StartFight()
    {
        if (combatEnemy == null || CombatManager.Instance == null)
        {
            Debug.LogWarning("[Teaser] Assign combatEnemy, and make sure CombatManager is in the scene.");
            return;
        }

        if (PlayerStats.Instance != null)
        {
            if (shooting && !fightBoosted && fightHealthBonus > 0)
            {
                PlayerStats.Instance.ModifyMax(StatType.MaxHealth, fightHealthBonus);
                fightBoosted = true;
            }

            PlayerStats.Instance.FullRestore();
        }

        CombatManager.Instance.StartCombat(combatEnemy);
    }

    public void ShowChoice()
    {
        StartNode("04_choice", choiceNode);
    }

    public void PlayFinale()
    {
        var scene = FindAnyObjectByType<SceneEvent>();

        if (scene == null || scene.finaleCutsceneNode == null)
        {
            Debug.LogWarning("[Teaser] No SceneEvent with a finaleCutsceneNode in the scene.");
            return;
        }

        StartNode("05_finale", scene.finaleCutsceneNode);
    }

    public void ToggleLanguage()
    {
        var next = LanguageManager.Current == GameLanguage.EN ? GameLanguage.TR : GameLanguage.EN;
        LanguageManager.SetLanguage(next);
    }

    public void ToggleHud()
    {
        if (hiddenCanvases.Count == 0)
            CollectHudCanvases();

        hudHidden = !hudHidden;

        foreach (var go in hiddenCanvases)
            if (go != null)
                go.SetActive(!hudHidden);
    }

    void CollectHudCanvases()
    {
        foreach (var canvas in FindObjectsByType<Canvas>(FindObjectsInactive.Include))
        {
            if (!canvas.isRootCanvas)
                continue;

            foreach (var name in hiddenCanvasNames)
                if (canvas.name == name)
                    hiddenCanvases.Add(canvas.gameObject);
        }
    }

    public void ToggleBars()
    {
        if (barsRoot != null)
        {
            Destroy(barsRoot);
            barsRoot = null;
            return;
        }

        barsRoot = new GameObject("__TeaserBars", typeof(Canvas));
        barsRoot.transform.SetParent(transform, false);
        var canvas = barsRoot.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 32760;
        float source = (float)Screen.width / Mathf.Max(Screen.height, 1);
        float barHeight = Mathf.Clamp01((1f - source / barAspect) * 0.5f);
        MakeBar(barsRoot.transform, new Vector2(0f, 1f - barHeight), Vector2.one);
        MakeBar(barsRoot.transform, Vector2.zero, new Vector2(1f, barHeight));
    }

    static void MakeBar(Transform parent, Vector2 anchorMin, Vector2 anchorMax)
    {
        var go = new GameObject("Bar", typeof(RectTransform), typeof(UnityEngine.UI.Image));
        go.transform.SetParent(parent, false);
        var rt = (RectTransform)go.transform;
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var image = go.GetComponent<UnityEngine.UI.Image>();
        image.color = Color.black;
        image.raycastTarget = false;
    }

    public void StepTimeScale(int direction)
    {
        timeScaleIndex = Mathf.Clamp(timeScaleIndex + direction, 0, TimeScales.Length - 1);
        Time.timeScale = TimeScales[timeScaleIndex];
    }

    string FolderPath()
    {
        return System.IO.Path.Combine(System.IO.Directory.GetParent(Application.dataPath).FullName, outputFolder);
    }

    public void ToggleRecording()
    {
        #if UNITY_EDITOR

        if (recorder != null && recorder.IsRecording())
            StopRecording();
        else
            StartRecording("teaser");

        #else

        Debug.LogWarning("[Teaser] Recording is Editor-only.");

        #endif
    }

    public void StartRecording(string label)
    {
        #if UNITY_EDITOR

        if (recorder != null && recorder.IsRecording())
            return;

        string folder = FolderPath();
        System.IO.Directory.CreateDirectory(folder);
        var clip = ScriptableObject.CreateInstance<MovieRecorderSettings>();
        clip.name = label;
        clip.Enabled = true;
        clip.ImageInputSettings = new GameViewInputSettings { OutputWidth = width, OutputHeight = height };
        clip.AudioInputSettings.PreserveAudio = true;
        clip.OutputFile = System.IO.Path.Combine(folder, label);
        var settings = ScriptableObject.CreateInstance<RecorderControllerSettings>();
        settings.AddRecorderSettings(clip);
        settings.SetRecordModeToManual();
        settings.FrameRate = frameRate;
        recorder = new RecorderController(settings);
        recorder.PrepareRecording();
        recorder.StartRecording();

        #endif
    }

    public void StopRecording()
    {
        #if UNITY_EDITOR

        if (recorder == null)
            return;

        if (recorder.IsRecording())
            recorder.StopRecording();

        recorder = null;

        #endif
    }

    void OnDisable()
    {
        Time.timeScale = 1f;
        StopRecording();
    }
}

#endif
