using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(menuName = "Quests/Quest")]
public class Quest : ScriptableObject
{
    public string questID;
    public string questName;
    public string description;

    public List<QuestObjectives> objectives;

    public void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = Guid.NewGuid().ToString();
        }
    }   

}

[System.Serializable]

public class QuestObjectives
{
    public string objectiveID;
    public string description;
    public ObjectiveType type;

    public int requiredAmount;
    public int currentAmount;

    public bool IsCompleted => currentAmount >= requiredAmount;
}

public enum ObjectiveType { CollectItem, DefeatEnemy, ReachLocation, TalkNPC, Custom }

[System.Serializable]
public class QuestProgress
{
    public Quest quest;
    public List<QuestObjectives> objectives;

    public QuestProgress(Quest quest)
    {
        this.quest = quest;
        objectives = new List<QuestObjectives>();
        foreach (var obj in quest.objectives)
        {
            objectives.Add(new QuestObjectives
            {
                objectiveID = obj.objectiveID,
                description = obj.description,
                type = obj.type,
                requiredAmount = obj.requiredAmount,
                currentAmount = 0
            });
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);

    public string QuestID => quest.questID;
}
