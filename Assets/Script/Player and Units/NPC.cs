using TMPro;
using UnityEngine.UI;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

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

    public void Interact()
    {
        if (dialogueData == null || (PauseController.IsGamePaused && isDialogueActive))
        {
            Debug.LogWarning("No dialogue data assigned to NPC.");
            return;
        }
        if (isDialogueActive)
        {
            NextLine();
        }
        else
        {
            StartDialogue();
        }
    }
    void StartDialogue()
    {
        isDialogueActive = true;
        dialogueIndex = 0;

        nameText.SetText(dialogueData.npcName);
        portraitImage.sprite = dialogueData.npcPortrait;

        dialoguePanel.SetActive(true);
        PauseController.SetPause(true);

        StartCoroutine(TypeLine());
    }

    void NextLine()
    {
        if (isTyping)
        {
            //Skip typing and show full line
            StopAllCoroutines();
            dialogueText.SetText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }
        else if(++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            StartCoroutine(TypeLine());
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueText.SetText(string.Empty);

        string currentLine = dialogueData.dialogueLines[dialogueIndex];
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueText.text += letter;
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;

        if (dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    } 

    public void EndDialogue()
    {
        StopAllCoroutines();
        isDialogueActive = false;
        dialogueText.SetText(string.Empty);
        dialoguePanel.SetActive(false);
        PauseController.SetPause(false);
    }

    private bool playerInRange = false;
    private GameObject currentPlayer;

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("ENTER TRIGGER: " + other.name);

        if (other.CompareTag("Player"))
        {
            Debug.Log("PLAYER IN RANGE");
            playerInRange = true;
            currentPlayer = other.gameObject;
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            currentPlayer = null;
        }
    }

    //void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.F))
    //    {
    //        Debug.Log("F pressed");
    //    }
    //    if (playerInRange && Input.GetKeyDown(KeyCode.F))
    //    {
    //        Interact(currentPlayer);
    //    }
    //}
}
