using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MultipleChoicePuzzleUI : MonoBehaviour
{
    public static MultipleChoicePuzzleUI I;

    [Header("UI Refs")]
    public GameObject panel;          // PuzzlePanel
    public TMP_Text questionText;     // QuestionText
    public TMP_Text feedbackText;     // FeedbackText
    public Button[] buttons;          // size 3 (BtnA, BtnB, BtnC)

    private int _correctIndex;
    private Action _onSolved;

    private void Awake()
    {
        I = this;
        if (panel) panel.SetActive(false);
        if (feedbackText) feedbackText.text = "";
    }

    public bool IsOpen => panel != null && panel.activeSelf;

    public void Open(string question, string[] options, int correctIndex, Action onSolved)
    {
        _correctIndex = correctIndex;
        _onSolved = onSolved;

        panel.SetActive(true);
        feedbackText.text = "";
        questionText.text = question;

        for (int i = 0; i < buttons.Length; i++)
        {
            int idx = i;
            buttons[i].onClick.RemoveAllListeners();
            buttons[i].GetComponentInChildren<TMP_Text>().text = options[i];
            buttons[i].onClick.AddListener(() => Pick(idx));
        }
    }

    private void Pick(int picked)
    {
        if (picked == _correctIndex)
        {
            feedbackText.text = "Đúng rồi!";
            _onSolved?.Invoke();
            Close();
        }
        else
        {
            feedbackText.text = "Sai rồi, thử lại nhé.";
        }
    }

    public void Close()
    {
        panel.SetActive(false);
        _onSolved = null;
    }
}
