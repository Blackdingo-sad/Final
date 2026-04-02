using System.Collections.Generic;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    private ItemDictionary itemDictionary;

    public GameObject inventoryPanel;
    public GameObject slotPrefab;
    public int slotCount;
    public GameObject[] itemPrefabs; // Array of item prefabs to populate the inventory
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        itemDictionary = Object.FindAnyObjectByType<ItemDictionary>();
        //for (int i = 0; i < slotCount; i++)
        //{
        //    Slot slot = Instantiate(slotPrefab, inventoryPanel.transform).GetComponent<Slot>();
        //    if (i < itemPrefabs.Length)
        //    {
        //        GameObject item = Instantiate(itemPrefabs[i], slot.transform);
        //        item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Center the item in the slot
        //        slot.curentItem = item;
        //    }
        //}

    }

    public bool AddItem(GameObject itemPrefab)
    {
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.curentItem == null)
            {
                GameObject newItem = Instantiate(itemPrefab, slot.transform);
                newItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Center the item in the slot
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
        foreach(Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot.curentItem != null)
            {
               Item item = slot.curentItem.GetComponent<Item>();
                invData.Add(new InventorySaveData { itemID = item.ID, slotIndex = slotTransform.GetSiblingIndex() });
            }
         
        }
        return invData;
    }

    public void SetInventoryItems(List<InventorySaveData> invData)
    {
        foreach(Transform child in inventoryPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for(int i=0; i < slotCount; i++)
        { 
            Instantiate(slotPrefab, inventoryPanel.transform);
        }

        foreach(InventorySaveData data in invData)
        {
            if (data.slotIndex < slotCount)
            {
                Slot slot = inventoryPanel.transform.GetChild(data.slotIndex).GetComponent<Slot>();
                GameObject itemPrefab = itemDictionary.GetItemPrefab(data.itemID);
                if (itemPrefab != null)
                {
                    GameObject item = Instantiate(itemPrefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Center the item in the slot
                    slot.curentItem = item;
                }
            }
        }
    }

    public void SetInventoryScale(float scale)
    {
        foreach (Transform slotTransform in inventoryPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot != null && slot.curentItem != null)
            {
                RectTransform rect = slot.curentItem.GetComponent<RectTransform>();
                rect.localScale = Vector3.one * scale;
            }
        }
    }
}
