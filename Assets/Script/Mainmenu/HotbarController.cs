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

    private void Awake()
    {
        itemDictionary = ResolveItemDictionary();
    }

    private ItemDictionary ResolveItemDictionary()
    {
        if (itemDictionary != null) return itemDictionary;

        itemDictionary = Object.FindAnyObjectByType<ItemDictionary>();
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

    private void Start()
    {
        InitializeHotbar();
    }

    // Tạo slot hotbar
    private void InitializeHotbar()
    {
        for (int i = hotbarPanel.transform.childCount - 1; i >= 0; i--)
        {
            DestroyImmediate(hotbarPanel.transform.GetChild(i).gameObject);
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

            if (slot.currentItem != null)
            {
                Item item = slot.currentItem.GetComponent<Item>();
                if (item == null) continue;

                data.Add(new InventorySaveData
                {
                    itemID = item.ID,
                    slotIndex = slotTransform.GetSiblingIndex(),
                    quantity = item.quantity
                });
            }
        }

        return data;
    }

    // Load lại hotbar từ save
    public void SetHotbarItems(List<InventorySaveData> data)
    {
        InitializeHotbar();

        if (data == null || data.Count == 0)
        {
            return;
        }

        if (itemDictionary == null)
        {
            itemDictionary = ResolveItemDictionary();
            if (itemDictionary == null)
            {
                Debug.LogWarning("ItemDictionary not found when loading hotbar.");
                return;
            }
        }

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
                    Item sourceItem = prefab.GetComponent<Item>();
                    GameObject prefabToInstantiate = sourceItem != null && sourceItem.uiPrefab != null ? sourceItem.uiPrefab : prefab;

                    GameObject item = Instantiate(prefabToInstantiate, slot.transform);

                    RectTransform rect = item.GetComponent<RectTransform>();
                    if (rect != null)
                    {
                        rect.anchoredPosition = Vector2.zero;
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
                        itemComponent.quantity = itemData.quantity;
                        itemComponent.UpdateQuantityDisplay();
                    }

                    slot.currentItem = item;
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

            if (slot.currentItem == null)
            {
                Item sourceItem = itemPrefab.GetComponent<Item>();
                GameObject prefabToInstantiate = sourceItem != null && sourceItem.uiPrefab != null ? sourceItem.uiPrefab : itemPrefab;
                GameObject item = Instantiate(prefabToInstantiate, slot.transform);

                RectTransform rect = item.GetComponent<RectTransform>();
                if (rect != null)
                {
                    rect.anchoredPosition = Vector2.zero;
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
                if (itemComponent != null && sourceItem != null)
                {
                    itemComponent.ID = sourceItem.ID;
                    itemComponent.quantity = sourceItem.quantity;
                    itemComponent.UpdateQuantityDisplay();
                }

                slot.currentItem = item;
                return;
            }
        }

        Debug.Log("Hotbar is full!");
    }
}