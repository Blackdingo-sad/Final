using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestUI : MonoBehaviour
{
    public Transform questListContent;
    public GameObject questEntryPrefab;
    public GameObject objectiveTextPrefab;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        UpdateQuestUI();
    }

    // Update is called once per frame
    public void UpdateQuestUI()
    {
        foreach (Transform child in questListContent)
        {
            Destroy(child.gameObject);
        }

        if (QuestController.Instance == null)
        {
            return;
        }

        foreach (var questProgress in QuestController.Instance.activateQuests)
        {
            GameObject entry = Instantiate(questEntryPrefab, questListContent);
            TMP_Text questNameText = entry.transform.Find("QuestNameText").GetComponent<TMP_Text>();
            Transform objectiveList = entry.transform.Find("ObjectiveList");

            questNameText.text = questProgress.quest.questName;
            foreach (var objective in questProgress.objectives)
            {
                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
}
