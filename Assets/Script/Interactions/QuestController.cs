using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuests = new();

    private QuestUI questUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        questUI = Object.FindFirstObjectByType<QuestUI>();
    }

    public void AcceptQuest(Quest quest)
    {
        if (quest == null)
        {
            return;
        }

        if (IsQuestActive(quest.questID))
        {
            Debug.LogWarning($"Quest with ID {quest.questID} is already active.");
            return;
        }

        activateQuests.Add(new QuestProgress(quest));

        if (questUI != null)
        {
            questUI.UpdateQuestUI();
        }
    }

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.quest.questID == questID);
}
