using System.Collections.Generic;
using UnityEngine;

public class QuestController : MonoBehaviour
{
    public static QuestController Instance { get; private set; }
    public List<QuestProgress> activateQuests = new();

    private QuestUI questUI;
    public List<string> handinQuestIDs = new();

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

    public bool IsQuestCompleted(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.quest.questID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }

    public void HandInQuest(string questID)
    {
        if (!RemoveRequiredItemsForQuest(questID))
        {
            return;
        }
        QuestProgress quest = activateQuests.Find(q => q.quest.questID == questID);
        if (quest != null)
        {
            handinQuestIDs.Add(questID);
            activateQuests.Remove(quest);
            questUI.UpdateQuestUI();
        }
    }

    public bool IsQuestHandedIn(string questID)
    {
        return handinQuestIDs.Contains(questID);
    }

    public bool RemoveRequiredItemsForQuest(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q.quest.questID == questID);
        if (quest == null) return false;

        Dictionary<int, int> requiredItems = new();

        foreach (QuestObjectives objectives in quest.objectives)
        {
            if(objectives.type == ObjectiveType.CollectItem && int.TryParse(objectives.objectiveID, out int itemID))
            {
                requiredItems[itemID] = objectives.requiredAmount;
            }
        }
        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
        foreach (var item in requiredItems)
        {
            if (!itemCounts.TryGetValue(item.Key, out int count) || count < item.Value)
            {
                return false;
            }
        }

        foreach (var itemRequirement in requiredItems)
        {
            InventoryController.Instance.RemoveItemsFromInventory(itemRequirement.Key, itemRequirement.Value); 
        }
        return true;

    }

    public void LoadQuestProgress(List<QuestSaveData> savedQuests)
    {
        activateQuests = new List<QuestProgress>();

        if (savedQuests == null || savedQuests.Count == 0) return;

        if (QuestDictionary.Instance == null)
        {
            Debug.LogWarning("QuestDictionary.Instance is null. Cannot load quest progress.");
            return;
        }

        foreach (QuestSaveData data in savedQuests)
        {
            Quest quest = QuestDictionary.Instance.GetQuest(data.questID);
            if (quest == null) continue;

            QuestProgress progress = new QuestProgress(quest);

            if (data.objectives != null)
            {
                foreach (ObjectiveSaveData objData in data.objectives)
                {
                    QuestObjectives objective = progress.objectives.Find(o => o.objectiveID == objData.objectiveID);
                    if (objective != null)
                    {
                        objective.currentAmount = objData.currentAmount;
                    }
                }
            }

            activateQuests.Add(progress);
        }

        CheckInventoryForQuests();
        if (questUI != null) questUI.UpdateQuestUI();
    }

    public List<QuestSaveData> GetQuestSaveData()
    {
        List<QuestSaveData> saveData = new List<QuestSaveData>();
        foreach (QuestProgress progress in activateQuests)
        {
            if (progress?.quest == null) continue;

            QuestSaveData data = new QuestSaveData
            {
                questID = progress.quest.questID,
                objectives = new List<ObjectiveSaveData>()
            };

            foreach (QuestObjectives obj in progress.objectives)
            {
                data.objectives.Add(new ObjectiveSaveData
                {
                    objectiveID = obj.objectiveID,
                    currentAmount = obj.currentAmount
                });
            }

            saveData.Add(data);
        }
        return saveData;
    }


}
