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
        
        if (InventoryController.Instance != null)
        {
            InventoryController.Instance.OnInventoryChanged += CheckInventoryForQuests;
        }
        else
        {
            Debug.LogWarning("InventoryController.Instance not found when QuestController initialized");
        }
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
        CheckInventoryForQuests();

        if (questUI != null)
        {
            questUI.UpdateQuestUI();
        }
    }

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q.quest.questID == questID);

    public void CheckInventoryForQuests()
    {
        if (InventoryController.Instance == null)
        {
            Debug.LogWarning("InventoryController.Instance is null");
            return;
        }

        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
        bool anyChanged = false;

        foreach (QuestProgress quest in activateQuests)
        {
            if (quest?.objectives == null) continue;
            
            foreach (QuestObjectives questObjective in quest.objectives)
            {
                if (questObjective == null) continue;
                if (questObjective.type != ObjectiveType.CollectItem) continue;

                int newAmount = 0;
                if (questObjective.targetItemIDs != null && questObjective.targetItemIDs.Count > 0)
                {
                    foreach (int itemID in questObjective.targetItemIDs)
                    {
                        if (itemCounts.TryGetValue(itemID, out int count))
                        {
                            newAmount += count;
                        }
                    }
                }
                
                if (questObjective.currentAmount != newAmount)
                {
                    string itemIDsStr = questObjective.targetItemIDs != null ? string.Join(",", questObjective.targetItemIDs) : "None";
                    Debug.Log($"Quest: {quest.quest.questName} | Objective: {questObjective.description} | ItemIDs: {itemIDsStr} | Updated: {questObjective.currentAmount} ? {newAmount}");
                    questObjective.currentAmount = newAmount;
                    anyChanged = true;
                }
            }
        }

        if (anyChanged && questUI != null)
        {
            questUI.UpdateQuestUI();
        }
    }
    public void LoadQuestProgress(List<QuestProgress> savedQuests)
    {
        activateQuests = savedQuests ?? new();

        CheckInventoryForQuests();
        questUI.UpdateQuestUI();
    }
}
