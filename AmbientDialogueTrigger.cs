using UnityEngine;
using UnityEngine.UI;

public class AmbientDialogueTrigger : MonoBehaviour
{
    public AmbientDialoguePool pool;
    public bool autoHook = true;
    private DialogueNode runtimeNode;
    private bool isHooked;

    void OnEnable()
    {
        if (!autoHook || isHooked)
            return;

        if (TryGetComponent<Button>(out var button))
            button.onClick.AddListener(Play);

        if (TryGetComponent<UIHoverRegion>(out var hover))
            hover.OnRegionClicked += Play;

        isHooked = true;
    }

    void OnDisable()
    {
        if (!isHooked)
            return;

        if (TryGetComponent<Button>(out var button))
            button.onClick.RemoveListener(Play);

        if (TryGetComponent<UIHoverRegion>(out var hover))
            hover.OnRegionClicked -= Play;

        isHooked = false;
    }

    public void Play()
    {
        if (pool == null || DialogueManager.Instance == null || DialogueManager.Instance.IsInDialogue())
            return;

        if (runtimeNode == null)
        {
            runtimeNode = ScriptableObject.CreateInstance<DialogueNode>();
            runtimeNode.hideFlags = HideFlags.HideAndDontSave;
            runtimeNode.isFinalNode = true;
        }

        runtimeNode.speakerName = pool.speakerName;
        runtimeNode.speakerPortrait = pool.speakerPortrait;
        runtimeNode.speakerNameColor = pool.speakerNameColor;
        runtimeNode.lines = new[] { pool.PickLine() };
        DialogueManager.Instance.StartDialogue(runtimeNode);
    }

    void OnDestroy()
    {
        if (runtimeNode != null)
            Destroy(runtimeNode);
    }
}
