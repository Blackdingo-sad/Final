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
            if (questProgress?.quest == null) continue;

            GameObject entry = Instantiate(questEntryPrefab, questListContent);

            Transform questNameTransform = entry.transform.Find("QuestNameText");
            if (questNameTransform == null)
            {
                Debug.LogWarning($"QuestEntryPrefab is missing 'QuestNameText' child.");
                continue;
            }

            Transform objectiveList = entry.transform.Find("ObjectiveList");
            if (objectiveList == null)
            {
                Debug.LogWarning($"QuestEntryPrefab is missing 'ObjectiveList' child.");
                continue;
            }

            TMP_Text questNameText = questNameTransform.GetComponent<TMP_Text>();
            if (questNameText == null)
            {
                Debug.LogWarning($"'QuestNameText' is missing TMP_Text component.");
                continue;
            }

            questNameText.text = questProgress.quest.questName;

            foreach (var objective in questProgress.objectives)
            {
                if (objective == null) continue;

                GameObject objTextGO = Instantiate(objectiveTextPrefab, objectiveList);
                TMP_Text objText = objTextGO.GetComponent<TMP_Text>();
                if (objText == null)
                {
                    Debug.LogWarning("ObjectiveTextPrefab is missing TMP_Text component.");
                    continue;
                }
                objText.text = $"{objective.description} ({objective.currentAmount}/{objective.requiredAmount})";
            }
        }
    }
}
