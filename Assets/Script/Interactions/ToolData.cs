using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Tools/Tool Data")]
public class ToolData : ScriptableObject
{
    public ToolType toolType;
    public Sprite icon;
}
