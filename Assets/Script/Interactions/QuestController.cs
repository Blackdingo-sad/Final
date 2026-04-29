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

    public bool IsQuestActive(string questID) => activateQuests.Exists(q => q?.quest != null && q.quest.questID == questID);

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
        QuestProgress quest = activateQuests.Find(q => q?.quest != null && q.quest.questID == questID);
        return quest != null && quest.objectives.TrueForAll(o => o.IsCompleted);
    }

    public void HandInQuest(string questID)
    {
        if (!RemoveRequiredItemsForQuest(questID))
            return;

        QuestProgress quest = activateQuests.Find(q => q.quest.questID == questID);
        if (quest != null)
        {
            handinQuestIDs.Add(questID);
            activateQuests.Remove(quest);
            GiveRewards(quest.quest);
            questUI?.UpdateQuestUI();
        }
    }

    private void GiveRewards(Quest quest)
    {
        if (quest.rewards == null || quest.rewards.Count == 0)
        {
            Debug.Log($"[Quest] '{quest.questName}' handed in. No rewards.");
            return;
        }

        System.Text.StringBuilder rewardLog = new System.Text.StringBuilder();
        rewardLog.Append($"[Quest] '{quest.questName}' handed in. Rewards: ");

        foreach (QuestReward reward in quest.rewards)
        {
            if (reward.rewardType == QuestRewardType.Currency)
            {
                CurrencyController.Instance?.AddGold(reward.goldAmount);
                rewardLog.Append($"{reward.goldAmount} Gold, ");
            }
            else if (reward.rewardType == QuestRewardType.Item)
            {
                bool added = InventoryController.Instance?.AddItemToInventory(reward.itemID, reward.itemQuantity) ?? false;
                if (!added)
                {
                    Debug.LogWarning($"[Quest] Failed to give item ID={reward.itemID} x{reward.itemQuantity}");
                }
                else
                {
                    GameObject prefab = Object.FindFirstObjectByType<ItemDictionary>()?.GetItemPrefab(reward.itemID);
                    string itemName = prefab?.GetComponent<Item>()?.Name ?? $"ItemID:{reward.itemID}";
                    rewardLog.Append($"{reward.itemQuantity}x {itemName}, ");
                }
            }
        }

        Debug.Log(rewardLog.ToString().TrimEnd(',', ' '));
    }

    public bool IsQuestHandedIn(string questID)
    {
        return handinQuestIDs.Contains(questID);
    }

    public bool RemoveRequiredItemsForQuest(string questID)
    {
        QuestProgress quest = activateQuests.Find(q => q?.quest != null && q.quest.questID == questID);
        if (quest == null) return false;

        // Build list of (targetItemIDs, requiredAmount) per objective
        List<(HashSet<int> itemIDs, int required)> objectiveRequirements = new List<(HashSet<int>, int)>();

        foreach (QuestObjectives objectives in quest.objectives)
        {
            if (objectives.type != ObjectiveType.CollectItem) continue;
            if (objectives.targetItemIDs == null || objectives.targetItemIDs.Count == 0) continue;

            objectiveRequirements.Add((new HashSet<int>(objectives.targetItemIDs), objectives.requiredAmount));
        }

        // Check if player has enough items for each objective
        Dictionary<int, int> itemCounts = InventoryController.Instance.GetItemCounts();
        foreach (var (itemIDs, required) in objectiveRequirements)
        {
            int total = 0;
            foreach (int id in itemIDs)
                total += itemCounts.GetValueOrDefault(id, 0);

            if (total < required)
            {
                Debug.LogWarning($"[Quest] Cannot hand in '{questID}': need {required} of [{string.Join(",", itemIDs)}], only have {total}");
                return false;
            }
        }

        // Remove items slot by slot (left to right) for each objective
        System.Text.StringBuilder removeLog = new System.Text.StringBuilder();
        removeLog.Append($"[Quest] Items removed for '{questID}': ");

        foreach (var (itemIDs, required) in objectiveRequirements)
        {
            int remaining = required;
            foreach (Transform slotTransform in InventoryController.Instance.inventoryPanel.transform)
            {
                if (remaining <= 0) break;

                Slot slot = slotTransform.GetComponent<Slot>();
                if (slot == null || slot.currentItem == null) continue;

                Item item = slot.currentItem.GetComponent<Item>();
                if (item == null || !itemIDs.Contains(item.ID)) continue;

                int removeAmount = Mathf.Min(remaining, item.quantity);
                removeLog.Append($"{removeAmount}x {item.Name}(ID:{item.ID}), ");
                item.RemoveFromStack(removeAmount);
                remaining -= removeAmount;

                if (item.quantity <= 0)
                {
                    Object.Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }

        InventoryController.Instance.RebuildItemCounts();
        Debug.Log(removeLog.ToString().TrimEnd(',', ' '));
        return true;
    }

    public void LoadQuestProgress(List<QuestSaveData> savedQuests)
    {
        // Luôn xóa state hi?n t?i tr??c
        activateQuests = new List<QuestProgress>();
        handinQuestIDs = new List<string>();

        // Làm m?i reference questUI vì có th? b? null khi load in-game
        if (questUI == null)
            questUI = Object.FindFirstObjectByType<QuestUI>();

        if (savedQuests == null || savedQuests.Count == 0)
        {
            questUI?.UpdateQuestUI();
            return;
        }

        if (QuestDictionary.Instance == null)
        {
            Debug.LogWarning("QuestDictionary.Instance is null. Cannot load quest progress.");
            questUI?.UpdateQuestUI();
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
                        objective.currentAmount = objData.currentAmount;
                }
            }

            activateQuests.Add(progress);
        }

        CheckInventoryForQuests();
        questUI?.UpdateQuestUI();
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
