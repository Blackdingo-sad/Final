using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary; //Tên Boundary mà player đang đứng
    public List<InventorySaveData> inventorySaveData;

}
