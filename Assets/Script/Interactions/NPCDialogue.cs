using System.Collections;
using UnityEngine;

[CreateAssetMenu(fileName = "NewNPCDialogue", menuName = "NPC Dialogue")]
public class NPCDialogue : ScriptableObject
{
    public string npcName;
    public Sprite npcPortrait;
    public string[] dialogueLines;
    public bool[] autoProgressLines;    
    public float autoProgressDelay = 1.5f;
    public float typingSpeed = 0.5f; // Time each dialogue line is displayed       
    public AudioClip[] VoiceSound; // Array of voice clips corresponding to each dialogue line
    public float VoicePitch = 1f;
}
