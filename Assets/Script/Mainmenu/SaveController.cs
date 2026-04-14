using Cinemachine;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

public class SaveController : MonoBehaviour
{
    private string saveLocation;
    private InventoryController inventoryController;
    private HotbarController hotbarController;
    private Chest[] chests;
    private ShopNPC[] shops;

    void Start()
    {
        InitializeComponents();
        LoadGame();
    }

    private void InitializeComponents()
    {
        saveLocation = Path.Combine(Application.persistentDataPath, "saveData.json");
        inventoryController = InventoryController.Instance != null ? InventoryController.Instance : Object.FindFirstObjectByType<InventoryController>();
        hotbarController = Object.FindFirstObjectByType<HotbarController>();
        chests = Object.FindObjectsByType<Chest>(FindObjectsSortMode.None);
        shops = FindObjectsOfType<ShopNPC>();
    }

    public void SaveGame()
    {
        if (inventoryController == null || hotbarController == null)
        {
            InitializeComponents();
        }

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

        if (inventoryController == null || hotbarController == null)
        {
            Debug.LogError("InventoryController or HotbarController not found!");
            return;
        }

        SaveData saveData = new SaveData
        {
            playerPosition = player.transform.position,
            mapBoundary = confiner.m_BoundingShape2D.gameObject.name,
            inventorySaveData = inventoryController.GetInventoryItems(),
            hotbarSaveData = hotbarController.GetHotbarItems(),
            chestSaveData = GetChestStates(),
            questProgressData = QuestController.Instance.GetQuestSaveData(),
            playerGold = CurrencyController.Instance.GetGold(),
            shopStates = GetShopStates()
        };
        File.WriteAllText(saveLocation, JsonUtility.ToJson(saveData));
    }

    private List<ShopInstanceData> GetShopStates()
    {
        List<ShopInstanceData> shopStates = new List<ShopInstanceData>();
        foreach (var shop in shops)
        {
            ShopInstanceData shopData = new ShopInstanceData
            {
                shopID = shop.shopID,
                stock = new List<ShopItemData>()
            };

            foreach (var stockItem in shop.GetCurrentStock())
            {
                shopData.stock.Add(new ShopItemData
                {
                    itemID = stockItem.itemID,
                    quantity = stockItem.quantity
                });
            }
            shopStates.Add(shopData);
        }
        return shopStates;
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
        if (inventoryController == null || hotbarController == null)
        {
            Debug.LogError("InventoryController or HotbarController not found!");
            return;
        }

        if (File.Exists(saveLocation))
        {
            SaveData saveData = JsonUtility.FromJson<SaveData>(File.ReadAllText(saveLocation));
            if (saveData == null)
            {
                inventoryController.SetInventoryItems(new List<InventorySaveData>());
                hotbarController.SetHotbarItems(new List<InventorySaveData>());
                return;
            }

            inventoryController.SetInventoryItems(saveData.inventorySaveData ?? new List<InventorySaveData>());
            hotbarController.SetHotbarItems(saveData.hotbarSaveData ?? new List<InventorySaveData>());

            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                player.transform.position = saveData.playerPosition;
            }

            CinemachineConfiner2D confiner = FindFirstObjectByType<CinemachineConfiner2D>();
            if (confiner != null && !string.IsNullOrEmpty(saveData.mapBoundary))
            {
                GameObject boundaryObj = GameObject.Find(saveData.mapBoundary);
                if (boundaryObj != null)
                {
                    PolygonCollider2D boundary = boundaryObj.GetComponent<PolygonCollider2D>();
                    if (boundary != null)
                    {
                        confiner.m_BoundingShape2D = boundary;
                    }
                }
            }

            LoadChestStates(saveData.chestSaveData ?? new List<ChestSaveData>());

            LoadShopStates(saveData.shopStates);
            CurrencyController.Instance.SetGold(saveData.playerGold);

            QuestController.Instance.LoadQuestProgress(saveData.questProgressData);
        }
        else
        {
            inventoryController.SetInventoryItems(new List<InventorySaveData>());
            hotbarController.SetHotbarItems(new List<InventorySaveData>());

            Debug.LogWarning("No save file found!");
            SaveGame();
        }
    }

    private void LoadShopStates(List<ShopInstanceData> shopStates)
    {
        if (shopStates == null) return;
        foreach (var shop in shops)
        {
            ShopInstanceData shopData = shopStates.FirstOrDefault(s => s.shopID == shop.shopID);

            if (shopData != null)
            {
                List<ShopNPC.ShopStockItem> loadedStock = new List<ShopNPC.ShopStockItem>();

                foreach (var itemData in shopData.stock)
                {
                    loadedStock.Add(new ShopNPC.ShopStockItem
                    {
                        itemID = itemData.itemID,
                        quantity = itemData.quantity
                    });
                }

                shop.SetStock(loadedStock);
            }
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
