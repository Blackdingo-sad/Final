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

    [Header("Rewards")]
    public List<QuestReward> rewards = new List<QuestReward>();

    public void OnValidate()
    {
        if (string.IsNullOrEmpty(questID))
        {
            questID = Guid.NewGuid().ToString();
        }

        if (objectives != null)
        {
            foreach (var obj in objectives)
            {
                if (obj.targetItemIDs == null)
                {
                    obj.targetItemIDs = new List<int>();
                }
            }
        }
    }   

}

[System.Serializable]

public class QuestObjectives
{
    public string objectiveID;
    public string description;
    public ObjectiveType type;

    public List<int> targetItemIDs;
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
                targetItemIDs = new List<int>(obj.targetItemIDs ?? new List<int>()),
                requiredAmount = obj.requiredAmount,
                currentAmount = 0    
            });
        }
    }

    public bool IsCompleted => objectives.TrueForAll(o => o.IsCompleted);

    public string QuestID => quest.questID;
}

public enum QuestRewardType { Item, Currency }

[System.Serializable]
public class QuestReward
{
    public QuestRewardType rewardType;
    [Tooltip("Dung khi rewardType = Item")]
    public int itemID;
    public int itemQuantity = 1;
    [Tooltip("Dung khi rewardType = Currency")]
    public int goldAmount;
}

#if UNITY_EDITOR
[UnityEditor.CustomPropertyDrawer(typeof(QuestReward))]
public class QuestRewardDrawer : UnityEditor.PropertyDrawer
{
    public override float GetPropertyHeight(UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
    {
        return UnityEditor.EditorGUIUtility.singleLineHeight * 2 + UnityEditor.EditorGUIUtility.standardVerticalSpacing;
    }

    public override void OnGUI(UnityEngine.Rect position, UnityEditor.SerializedProperty property, UnityEngine.GUIContent label)
    {
        UnityEditor.EditorGUI.BeginProperty(position, label, property);
        float lineH   = UnityEditor.EditorGUIUtility.singleLineHeight;
        float spacing = UnityEditor.EditorGUIUtility.standardVerticalSpacing;

        var typeProp = property.FindPropertyRelative("rewardType");
        var idProp   = property.FindPropertyRelative("itemID");
        var qtyProp  = property.FindPropertyRelative("itemQuantity");
        var goldProp = property.FindPropertyRelative("goldAmount");

        UnityEngine.Rect typeRect  = new UnityEngine.Rect(position.x, position.y, position.width, lineH);
        UnityEngine.Rect fieldRect = new UnityEngine.Rect(position.x, position.y + lineH + spacing, position.width, lineH);

        UnityEditor.EditorGUI.PropertyField(typeRect, typeProp, new UnityEngine.GUIContent("Reward Type"));

        // 0 = Item, 1 = Currency
        if (typeProp.enumValueIndex == 1)
        {
            UnityEditor.EditorGUI.PropertyField(fieldRect, goldProp, new UnityEngine.GUIContent("Gold Amount"));
        }
        else
        {
            float half = (position.width - 4f) / 2f;
            UnityEngine.Rect idRect  = new UnityEngine.Rect(position.x, fieldRect.y, half, lineH);
            UnityEngine.Rect qtyRect = new UnityEngine.Rect(position.x + half + 4f, fieldRect.y, half, lineH);
            UnityEditor.EditorGUI.PropertyField(idRect,  idProp,  new UnityEngine.GUIContent("Item ID"));
            UnityEditor.EditorGUI.PropertyField(qtyRect, qtyProp, new UnityEngine.GUIContent("Qty"));
        }
        UnityEditor.EditorGUI.EndProperty();
    }
}
#endif

public class QuestDictionary : MonoBehaviour
{
    public static QuestDictionary Instance { get; private set; }

    public List<Quest> quests;

    private Dictionary<string, Quest> questLookup;

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

        questLookup = new Dictionary<string, Quest>();
        foreach (Quest quest in quests)
        {
            if (quest != null && !questLookup.ContainsKey(quest.questID))
            {
                questLookup[quest.questID] = quest;
            }
        }
    }

    public Quest GetQuest(string questID)
    {
        if (questLookup == null || !questLookup.TryGetValue(questID, out Quest quest))
        {
            Debug.LogWarning($"Quest with ID {questID} not found in QuestDictionary.");
            return null;
        }
        return quest;
    }
}
