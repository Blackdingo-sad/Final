using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs; // Array of item prefabs to populate the inventory

    void Awake()
    {
        itemDictionary = Object.FindAnyObjectByType<ItemDictionary>();
        EnsureSlotsInitialized();
    }

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
            if (slot != null && slot.curentItem != null)
            {
                Item SlotItem = slot.curentItem.GetComponent<Item>();
                if (SlotItem != null && SlotItem.ID == itemToAdd.ID)
                {
                    SlotItem.AddToStack(addAmount);
                    return true;
                }
            }
        }

        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.curentItem == null)
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
                    newItemComponent.quantity = addAmount;
                    newItemComponent.UpdateQuantityDisplay();
                }

                slot.curentItem = newItem;
                return true;
            }
        }
        Debug.Log("Inventory full! Cannot add item: " + itemPrefab.name);
        return false; // Inventory full
    }
    public List<InventorySaveData> GetInventoryItems()
    {
        List<InventorySaveData> invData = new List<InventorySaveData>();
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.curentItem != null)
            {
                Item item = slot.curentItem.GetComponent<Item>();
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
        foreach (Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
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
            itemDictionary = Object.FindAnyObjectByType<ItemDictionary>();
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
                        itemComponent.quantity = data.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }

                    slot.curentItem = item;

                }
            }
        }
    }
}
