using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;               

public class InteractPromptUI : MonoBehaviour
{
    public GameObject root;              // panel chứa text
    public TextMeshProUGUI label;        // dòng chữ
    void Awake() { if (!root) root = gameObject; SetVisible(false, null, KeyCode.F); }

    public void SetVisible(bool show, string msg, KeyCode key)
    {
        if (root) root.SetActive(show);
        if (show && label)
        {
            string k = key.ToString().ToUpper();
            label.text = $"[ {k} ] {msg}";
        }
    }
}

