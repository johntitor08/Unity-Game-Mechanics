using System.Collections;
using UnityEngine;
using TMPro;

public class SaveUI : MonoBehaviour
{
    public static SaveUI Instance;

    [Header("Panel")]
    public GameObject savePanel;

    [Header("Slots")]
    public SaveSlotUI[] slots;

    [Header("Toast")]
    public CanvasGroup toastCanvasGroup;
    public TextMeshProUGUI toastText;
    public float toastDuration = 2f;
    public float fadeDuration = 0.3f;
    private Coroutine toastCoroutine;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        for (int i = 0; i < slots.Length; i++)
            slots[i].Initialize(i, this);

        if (toastCanvasGroup != null)
        {
            toastCanvasGroup.alpha = 0f;
            toastCanvasGroup.blocksRaycasts = false;
        }

        if (savePanel != null)
            savePanel.SetActive(false);
    }

    public void OpenPanel()
    {
        if (savePanel != null)
            savePanel.SetActive(true);

        RefreshAllSlots();
    }

    public void ClosePanel()
    {
        if (savePanel != null)
            savePanel.SetActive(false);
    }

    public void TogglePanel()
    {
        if (savePanel == null)
            return;

        if (savePanel.activeSelf)
            ClosePanel();
        else
            OpenPanel();
    }

    public void RefreshAllSlots()
    {
        foreach (var slot in slots)
            slot.Refresh();
    }

    public void SetAllSlotsInteractable(bool interactable)
    {
        foreach (var slot in slots)
            slot.SetInteractable(interactable);
    }

    public void ShowToast(string message)
    {
        if (toastCanvasGroup == null)
            return;

        if (toastText != null)
            toastText.text = message;

        if (toastCoroutine != null)
            StopCoroutine(toastCoroutine);

        toastCoroutine = StartCoroutine(ToastRoutine());
    }

    IEnumerator ToastRoutine()
    {
        toastCanvasGroup.blocksRaycasts = false;
        yield return StartCoroutine(Fade(0f, 1f));
        yield return new WaitForSeconds(toastDuration);
        yield return StartCoroutine(Fade(1f, 0f));
        toastCanvasGroup.alpha = 0f;
    }

    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;
        toastCanvasGroup.alpha = from;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            toastCanvasGroup.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        toastCanvasGroup.alpha = to;
    }
}
