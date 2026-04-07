using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;    
    public bool[] endDialogueLines;    //Marks lines that end the dialogue, allowing for multiple endings based on player choices
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.5f; // Time each dialogue line is displayed       
    public AudioClip[] VoiceSound; // Array of voice clips corresponding to each dialogue line
    public float VoicePitch = 1f;

    public DialogueChoice[] choices;

    public int questInProgressIndex;
    public int questCompletedIndex;
    public Quest quest;
}

[System.Serializable]
public class DialogueChoice
{
    public int dialogueIndex; 
    public string[] choices;
    public int[] nextDialogueIndexes; // Indices of the next dialogue lines based on player choice
    public bool[] giveQuest; 
}
