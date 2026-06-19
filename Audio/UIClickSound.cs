using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class UIClickSound : MonoBehaviour
{
    void Start()
    {
        if (TryGetComponent<Button>(out var b))
            b.onClick.AddListener(() => {
                if (GameAudioManager.Instance != null)
                    GameAudioManager.Instance.PlayUiClick();
            });
    }

    public static void AddToChildren(Transform root)
    {
        foreach (var btn in root.GetComponentsInChildren<Button>(true))
            if (btn.GetComponent<UIClickSound>() == null)
                btn.gameObject.AddComponent<UIClickSound>();
    }
}
