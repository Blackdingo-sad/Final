using TMPro;
using UnityEngine.UI;
using UnityEngine;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    public GameObject dialoguePanel;
    public TMP_Text dialogueText, nameText;
    public Image portraitImage;

    private int dialogueIndex;
    private bool isTyping, isDialogueActive;

    public string PromptMessage => throw new System.NotImplementedException();

    public bool CanInteract()
    { 
        return !isDialogueActive;
    }

    public void Interact(GameObject interactor)
    {
        if (dialogueData == null)
        {
            Debug.LogWarning("No dialogue data assigned to NPC.");
            return;
        }
    }
}
