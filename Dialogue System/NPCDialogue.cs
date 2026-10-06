using UnityEngine;
using UnityEngine.EventSystems;

public class NPCDialogue : MonoBehaviour, IPointerClickHandler
{
    public DialogueNode startNode;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (DialogueManager.Instance == null || DialogueManager.WorldClicksBlocked)
            return;

        DialogueManager.Instance.StartDialogue(startNode);
    }
}
