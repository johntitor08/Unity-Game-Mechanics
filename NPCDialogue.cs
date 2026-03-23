using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public DialogueNode startNode;

    void OnMouseDown()
    {
        if (DialogueManager.Instance != null)
            DialogueManager.Instance.StartDialogue(startNode);
    }
}
