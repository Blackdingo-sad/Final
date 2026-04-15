using TMPro;
using UnityEngine.UI;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public class NPC : MonoBehaviour, IInteractable
{
    public NPCDialogue dialogueData;
    private DialogueController dialogueUI;
    private int dialogueIndex;
    private bool isTyping, isDialogueActive;


    private enum QuestState { NotStarted, InProgress, Completed }
    private QuestState questState = QuestState.NotStarted; 
    private void Start()
    {
        dialogueUI = DialogueController.Instance;
    }

    public string PromptMessage => throw new System.NotImplementedException();

    public bool CanInteract()
    {
        return !isDialogueActive;
    }

    public void Interact()
    {
        if (dialogueUI == null)
        {
            dialogueUI = DialogueController.Instance;
            if (dialogueUI == null)
            {
                Debug.LogError("DialogueController.Instance is null. Ensure DialogueController exists in the scene.");
                return;
            }
        }

        if (dialogueData == null)
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
        // sync with quest data 
        SyncQuestState();
        // set dialogue line based on quest state
        if (questState == QuestState.NotStarted) 
        {
            dialogueIndex = 0; 
        }
        else if (questState == QuestState.InProgress) 
        {
            dialogueIndex = dialogueData.questInProgressIndex; 
        }
        else if (questState == QuestState.Completed) 
        {
            dialogueIndex = dialogueData.questCompletedIndex;
        }

        if (dialogueData.dialogueLines == null || dialogueData.dialogueLines.Length == 0)
        {
            Debug.LogWarning("Dialogue data has no dialogue lines.");
            return;
        }

        isDialogueActive = true;

        dialogueUI.SetNPCInfo(dialogueData.npcName, dialogueData.npcPortrait);
        dialogueUI.ShowDialogueUI(true);
        PauseController.SetPause(true);

        DisplayCurrentLines();
    }

    private void SyncQuestState()
    {
        if (dialogueData.quest == null) return;

        string questID = dialogueData.quest.questID;
        if (QuestController.Instance.IsQuestCompleted(questID) || QuestController.Instance.IsQuestHandedIn(questID))
        {
            questState = QuestState.Completed;
        }
        else if (QuestController.Instance.IsQuestActive(questID))
        {
            questState = QuestState.InProgress;
        }
        else
        {
            questState = QuestState.NotStarted;
        }
    }

    void NextLine()
    {
        if (isTyping)
        {
            //Skip typing and show full line
            StopAllCoroutines();
            dialogueUI.SetDialogueText(dialogueData.dialogueLines[dialogueIndex]);
            isTyping = false;
        }

        //clear choices
        dialogueUI.ClearChoices();
        //check for endDialogue
        if (dialogueData.endDialogueLines != null && dialogueData.endDialogueLines.Length > dialogueIndex && dialogueData.endDialogueLines[dialogueIndex])
        {
            EndDialogue();
            return;
        }

        //check if have dialogue choices
        foreach (DialogueChoice dialogueChoice in dialogueData.choices)
        {
            if (dialogueChoice.dialogueIndex == dialogueIndex)
            {
                DisplayChoices(dialogueChoice);
                return;
            }

        }

        if (++dialogueIndex < dialogueData.dialogueLines.Length)
        {
            DisplayCurrentLines();
        }
        else
        {
            EndDialogue();
        }
    }

    IEnumerator TypeLine()
    {
        isTyping = true;
        dialogueUI.SetDialogueText("");

        string currentLine = dialogueData.dialogueLines[dialogueIndex];
        foreach (char letter in dialogueData.dialogueLines[dialogueIndex])
        {
            dialogueUI.SetDialogueText(dialogueUI.dialogueText.text += letter);
            yield return new WaitForSeconds(dialogueData.typingSpeed);
        }
        isTyping = false;

        if (dialogueData.autoProgressLines != null && dialogueData.autoProgressLines.Length > dialogueIndex && dialogueData.autoProgressLines[dialogueIndex])
        {
            yield return new WaitForSeconds(dialogueData.autoProgressDelay);
            NextLine();
        }
    }

    void DisplayChoices(DialogueChoice choice)
    {
        for (int i = 0; i < choice.choices.Length; i++)
        {
            int nextIndex = choice.nextDialogueIndexes[i];
            bool giveQuest = choice.giveQuest[i];
            dialogueUI.CreateChoiceButton(choice.choices[i], () => ChooseOption(nextIndex, giveQuest));
        }
    }

    void ChooseOption(int nextIndex, bool givesQuest)
    {
        if (givesQuest)
        {
            QuestController.Instance.AcceptQuest(dialogueData.quest);
            questState = QuestState.InProgress;
        }

        dialogueIndex = nextIndex;
        dialogueUI.ClearChoices();
        DisplayCurrentLines();
    }

    void DisplayCurrentLines()
    {
        StopAllCoroutines();
        StartCoroutine(TypeLine());
    }

    public void EndDialogue()
    {
        if(questState == QuestState.Completed && QuestController.Instance.IsQuestHandedIn(dialogueData.quest.questID))
        {
            HandleQuestCompletion(dialogueData.quest);
        }

        StopAllCoroutines();
        isDialogueActive = false;
        dialogueUI.SetDialogueText(string.Empty);
        dialogueUI.ShowDialogueUI(false);
        PauseController.SetPause(false);
    }

    void HandleQuestCompletion(Quest quest)
    {
        QuestController.Instance.HandInQuest(quest.questID);
    }

    void Update()
    {
        if (isDialogueActive && Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            {
                var selected = EventSystem.current.currentSelectedGameObject;
                if (selected != null && selected.GetComponent<Button>() != null)
                {
                    return;
                }
            }
            NextLine();
        }
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
