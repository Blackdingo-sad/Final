using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolManager : MonoBehaviour
{
    public static ToolManager I;

    [SerializeField] private ToolSlot[] slots;
    [SerializeField] private ToolData defaultTool;

    public ToolData CurrentTool { get; private set; }
    private int currentIndex = -1;

    private void Start()
    {
        if (slots.Length > 0)
        {
            SelectSlot(0);
        }
    }

    private void Awake()
    {
        I = this;
    }

    private void Update()
    {
        HandleNumberInput();
    }

    void HandleNumberInput()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                SelectSlot(i);
            }
        }
    }

    public void SelectSlot(int index)
    {
        if (index < 0 || index >= slots.Length) return;

        currentIndex = index;
        CurrentTool = slots[index].Tool;

        UpdateSlotHighlight();

        Debug.Log("Selected slot: " + index);
    }

    public void SetToolFromClick(ToolData tool, int index)
    {
        currentIndex = index;
        CurrentTool = tool;
        UpdateSlotHighlight();
    }

    void UpdateSlotHighlight()
    {
        for (int i = 0; i < slots.Length; i++)
        {
            slots[i].SetHighlight(i == currentIndex);
        }
    }
}