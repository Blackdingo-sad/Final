using Cinemachine;
using NUnit.Framework;
using System.Collections.Generic;
using System.IO;
using UnityEditor.Overlays;
using UnityEngine;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests; 

    // Start is called once before the first execution of Update after the MonoBehaviour is created


    void Start()
    {
        InitializeComponents();
        LoadGame();
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = Object.FindFirstObjectByType<InventoryController>();
        hotbarController = Object.FindFirstObjectByType<HotbarController>();
        chests = Object.FindObjectsByType<Chest>(FindObjectsSortMode.None);
    }

    public void SaveGame()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogError("Player not found!");
            return;
        }

        CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
        if (confiner == null)
        {
            Debug.LogError("CinemachineConfiner not found!");
            return;
        }

        if (confiner.m_BoundingShape2D == null)
        {
            Debug.LogError("BoundingShape2D is NULL!");
            return;
        }
        SaveData saveData = new SaveData
        {
            playerPosition = GameObject.FindGameObjectWithTag("Player").transform.position,
            mapBoundary = FindFirstObjectByType<CinemachineConfiner2D>().m_BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestStates(), 

        };
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    private List<ChestSaveData> GetChestStates()
    {
        List<ChestSaveData> chestStates = new List<ChestSaveData>();
        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = new ChestSaveData
            {
                ChestID = chest.ChestID,
                isOpen = chest.IsOpen,
            };
            chestStates.Add(chestSaveData);
        }
        return chestStates;
    }

    public void LoadGame()
    {
        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            player.transform.position = saveData.playerPosition;
            CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            confiner.m_BoundingShape2D = GameObject.Find(saveData.mapBoundary).GetComponent<PolygonCollider2D>();

            inventoryController.SetInventoryItems(saveData.inventorySaveData);
            hotbarController.SetHotbarItems(saveData.hotbarSaveData);

            LoadChestStates(saveData.chestSaveData);
        }
        else
        {
            inventoryController.SetInventoryItems(new List<InventorySaveData>());
            hotbarController.SetHotbarItems(new List<InventorySaveData>());

            Debug.LogWarning("No save file found!");
            SaveGame();
        }
    }

    private void LoadChestStates(List<ChestSaveData> chestSates)
    {
        foreach (Chest chest in chests)
        {
            ChestSaveData chestSaveData = chestSates.Find(c => c.ChestID == chest.ChestID);

            if (chestSaveData != null)
            {
                chest.SetOpened(chestSaveData.isOpen);
            }
        }
    }
}
