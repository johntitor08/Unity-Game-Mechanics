using UnityEngine;

public class NPCDialogue : MonoBehaviour
{
    public DialogueNode startNode;
    public DialogueManager dialogueManager;

    void OnMouseDown()
    {
        dialogueManager.StartDialogue(startNode);
    }
}
