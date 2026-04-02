using System.Collections.Generic;
using UnityEngine;

public class HotbarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private GameObject slotPrefab;

    [Header("Settings")]
    [SerializeField] private int slotCount = 8;

    private ItemDictionary itemDictionary;

    private void Start()
    {
        itemDictionary = Object.FindAnyObjectByType<ItemDictionary>();
        InitializeHotbar();
    }

    // Tạo slot hotbar
    private void InitializeHotbar()
    {
        foreach (Transform child in hotbarPanel.transform)
        {
            Destroy(child.gameObject);
        }

        for (int i = 0; i < slotCount; i++)
        {
            Instantiate(slotPrefab, hotbarPanel.transform);
        }
    }

    // Lấy dữ liệu hotbar để save
    public List<InventorySaveData> GetHotbarItems()
    {
        List<InventorySaveData> data = new List<InventorySaveData>();

        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot.curentItem != null)
            {
                Item item = slot.curentItem.GetComponent<Item>();

                data.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex()
                });
            }
        }

        return data;
    }

    // Load lại hotbar từ save
    public void SetHotbarItems(List<InventorySaveData> data)
    {
        InitializeHotbar();

        foreach (InventorySaveData itemData in data)
        {
            if (itemData.slotIndex < slotCount)
            {
                Slot slot = hotbarPanel.transform
                    .GetChild(itemData.slotIndex)
                    .GetComponent<Slot>();

                GameObject prefab = itemDictionary.GetItemPrefab(itemData.itemID);

                if (prefab != null)
                {
                    GameObject item = Instantiate(prefab, slot.transform);
                    item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                    slot.curentItem = item;
                }
            }
        }
    }

    // Hàm để đưa item từ inventory xuống hotbar
    public void AddItemToHotbar(GameObject itemPrefab)
    {
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();

            if (slot.curentItem == null)
            {
                GameObject item = Instantiate(itemPrefab, slot.transform);
                item.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

                slot.curentItem = item;
                return;
            }
        }

        Debug.Log("Hotbar is full!");
    }
}