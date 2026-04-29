using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs; // Array of item prefabs to populate the inventory

    public static InventoryController Instance { get; private set; }
    Dictionary<int, int> itemsCountCache = new();
    public event Action OnInventoryChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        itemDictionary = ResolveItemDictionary();
        RebuildItemCounts();
        EnsureSlotsInitialized();
    }

    private ItemDictionary ResolveItemDictionary()
    {
        if (itemDictionary != null) return itemDictionary;

        itemDictionary = FindAnyObjectByType<ItemDictionary>();
        if (itemDictionary != null) return itemDictionary;

        ItemDictionary[] all = Resources.FindObjectsOfTypeAll<ItemDictionary>();
        foreach (ItemDictionary dict in all)
        {
            if (dict != null && dict.gameObject.scene.IsValid())
            {
                itemDictionary = dict;
                return itemDictionary;
            }
        }

        return null;
    }

    public void RebuildItemCounts()
    {
        itemsCountCache.Clear();

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item != null)
                {
                    itemsCountCache[item.ID] = itemsCountCache.GetValueOrDefault(item.ID, 0) + item.quantity;
                }
            }
        }

        OnInventoryChanged?.Invoke();
    }

    public Dictionary<int, int> GetItemCounts() => itemsCountCache;
    
    private void EnsureSlotsInitialized()
    {
        if (inventoryPanel == null || slotPrefab == null || slotCount <= 0)
        {
            return;
        }

        if (inventoryPanel.transform.childCount > 0)
        {
            return;
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }
    }

    public bool AddItem(GameObject itemPrefab)
    {
        if (inventoryPanel == null)
        {
            Debug.LogWarning("Inventory panel is not assigned!");
            return false;
        }

        Item itemToAdd = itemPrefab.GetComponent<Item>();
        if (itemToAdd == null) return false; //Not a valid item prefab

        int addAmount = Mathf.Max(1, itemToAdd.quantity);

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem != null)
            {
                Item SlotItem = slot.currentItem.GetComponent<Item>();
                if (SlotItem != null && SlotItem.ID == itemToAdd.ID)
                {
                    SlotItem.AddToStack(addAmount);
                    RebuildItemCounts();
                    return true; 
                }
            }
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
            {
                GameObject prefabForInventory = itemToAdd.uiPrefab != null ? itemToAdd.uiPrefab : itemPrefab;
                GameObject newItem = Instantiate(prefabForInventory, slot.transform);

                RectTransform rect = newItem.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero; // Center the item in the slot
                }

                if (newItem.GetComponent<CanvasGroup>() == null)
                {
                    newItem.AddComponent<CanvasGroup>();
                }
                if (newItem.GetComponent<ItemDragHandler>() == null)
                {
                    newItem.AddComponent<ItemDragHandler>();
                }

                Item newItemComponent = newItem.GetComponent<Item>();
                if (newItemComponent != null)
                {
                    newItemComponent.ID = itemToAdd.ID;
                    newItemComponent.quantity = addAmount;
                    newItemComponent.UpdateQuantityDisplay();
                }

                slot.currentItem = newItem;
                RebuildItemCounts();
                return true;
            }
        }
        Debug.Log("Inventory full! Cannot add item: " + itemPrefab.name);
        return false; // Inventory full
    }

    public bool AddItemToInventory(int itemID, int quantity)
    {
        ItemDictionary dict = UnityEngine.Object.FindAnyObjectByType<ItemDictionary>();
        if (dict == null) return false;

        GameObject prefab = dict.GetItemPrefab(itemID);
        if (prefab == null) return false;

        // Tìm slot ?ã có item cùng ID ?? stack
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot?.currentItem == null) continue;
            Item slotItem = slot.currentItem.GetComponent<Item>();
            if (slotItem != null && slotItem.ID == itemID)
            {
                slotItem.AddToStack(quantity);
                RebuildItemCounts();
                return true;
            }
        }

        // Không có stack ? t?o m?i trong slot tr?ng
        Item src = prefab.GetComponent<Item>();
        GameObject uiPrefab = (src != null && src.uiPrefab != null) ? src.uiPrefab : prefab;

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot == null || slot.currentItem != null) continue;

            GameObject newObj = Instantiate(uiPrefab, slot.transform);
            newObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            if (newObj.GetComponent<CanvasGroup>() == null) newObj.AddComponent<CanvasGroup>();
            if (newObj.GetComponent<ItemDragHandler>() == null) newObj.AddComponent<ItemDragHandler>();

            Item newItem = newObj.GetComponent<Item>();
            if (newItem != null)
            {
                newItem.ID = itemID;
                newItem.quantity = quantity;
                newItem.UpdateQuantityDisplay();
            }
            slot.currentItem = newObj;
            RebuildItemCounts();
            return true;
        }

        Debug.LogWarning($"[QuestReward] Inventory full, cannot give item ID {itemID}");
        return false;
    }
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item == null) continue;

                invData.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }

        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> invData)
    {
        for (int i = inventoryPanel.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(inventoryPanel.transform.GetChild(i).gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        if (invData == null || invData.Count == 0)
        {
            return;
        }

        if (itemDictionary == null)
        {
            itemDictionary = ResolveItemDictionary();
            if (itemDictionary == null)
            {
                Debug.LogWarning("ItemDictionary not found when loading inventory.");
                return;
            }
        }

        foreach (InventorySaveData data in invData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    Item sourceItem = itemPrefab.GetComponent<Item>();
                    GameObject prefabToInstantiate = sourceItem != null && sourceItem.uiPrefab != null ? sourceItem.uiPrefab : itemPrefab;

                    GameObject item = Instantiate(prefabToInstantiate, slot.transform);

                    RectTransform rect = item.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector2.zero; // Center the item in the slot
                    }

                    if (item.GetComponent<CanvasGroup>() == null)
                    {
                        item.AddComponent<CanvasGroup>();
                    }
                    if (item.GetComponent<ItemDragHandler>() == null)
                    {
                        item.AddComponent<ItemDragHandler>();
                    }

                    Item itemComponent = item.GetComponent<Item>();
                    if (itemComponent != null)
                    {
                        if (sourceItem != null)
                        {
                            itemComponent.ID = sourceItem.ID;
                        }
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = item;

                }
            }
        }

        RebuildItemCounts();
    }
    public void RemoveItemsFromInventory(int itemID, int amountToRemove)
    {
        foreach (Transform slotTranform in inventoryPanel.transform)
        {
            if (amountToRemove <= 0) break;

            Slot slot = slotTranform.GetComponent<Slot>();
            if (slot == null || slot.currentItem == null) continue;

            Item item = slot.currentItem.GetComponent<Item>();
            if (item != null && item.ID == itemID)
            {
                int removed = Mathf.Min(amountToRemove, item.quantity);
                item.RemoveFromStack(removed);
                amountToRemove -= removed;

                if (item.quantity <= 0)
                {
                    Destroy(slot.currentItem);
                    slot.currentItem = null;
                }
            }
        }
        RebuildItemCounts();
    }
}
