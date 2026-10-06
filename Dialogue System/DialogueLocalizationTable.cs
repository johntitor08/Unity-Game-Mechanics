using UnityEngine;

[CreateAssetMenu(menuName = "Dialogue/Localization Table", fileName = "DialogueLocalizationTable")]
public class DialogueLocalizationTable : ScriptableObject
{
    [System.Serializable]
    public class Entry
    {
        public string key;
        public DialogueNode en;
        public DialogueNode tr;
    }

    public Entry[] entries;
}
