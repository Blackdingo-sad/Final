using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ToolSlot : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image highlightBorder;

    public ToolData Tool { get; private set; }
    private int myIndex;

    public void Initialize(int index)
    {
        myIndex = index;
    }

    public void SetTool(ToolData tool)
    {
        Tool = tool;

        if (tool != null)
        {
            iconImage.sprite = tool.icon;
            iconImage.enabled = true;
        }
        else
        {
            iconImage.enabled = false;
        }
    }

    public void SetHighlight(bool active)
    {
        highlightBorder.enabled = active;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        ToolManager.I.SetToolFromClick(Tool, myIndex);
    }

}