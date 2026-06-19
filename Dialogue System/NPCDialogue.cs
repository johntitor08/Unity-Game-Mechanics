using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDialogue : MonoBehaviour, IPointerClickHandler
{
    public DialogueNode startNode;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (DialogueManager.Instance == null || DialogueManager.Instance.State != DialogueState.Idle)
            return;

        DialogueManager.Instance.StartDialogue(startNode);
    }
}
