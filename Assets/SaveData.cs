using JetBrains.Annotations;
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
    public List<string> handinQuestIDs;

    public int playerGold; // Lưu lượng vàng của player
    public List<ShopInstanceData> shopStates = new();
    public List<CropSaveData> cropSaveData = new();
    public long worldTimeTicks; // Lưu thời gian game (TimeSpan.Ticks)

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
public class ShopInstanceData
{
    public string shopID;
    public List<ShopItemData> stock = new();
}

[System.Serializable]
public class ShopItemData
{
    public int itemID;
    public int quantity;
}

[System.Serializable]
public class ObjectiveSaveData
{
    public string objectiveID;
    public int currentAmount;
}

[System.Serializable]
public class CropSaveData
{
    public string plotID;       // ID duy nhất của ô đất
    public string cropName;     // Tên CropData (dùng để tìm lại ScriptableObject)
    public int state;           // 0=Empty, 1=Growing, 2=ReadyToHarvest
    public float growTimer;     // Thời gian còn lại (giây)
}