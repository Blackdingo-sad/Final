using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public Vector3 playerPosition;
    public string mapBoundary; //Tên Boundary mà player đang đứng
    public List<InventorySaveData> inventorySaveData;
    public List<InventorySaveData> hotbarSaveData;
    public List<ChestSaveData> chestSaveData; 
    public List<QuestSaveData> questProgressData; 

}

[System.Serializable]
public class  ChestSaveData
{
    public string ChestID;
    public bool isOpen;
}

[System.Serializable]
public class QuestSaveData
{
    public string questID;
    public List<ObjectiveSaveData> objectives;
}

[System.Serializable]
public class ObjectiveSaveData
{
    public string objectiveID;
    public int currentAmount;
}