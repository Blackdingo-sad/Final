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

    public static HotbarController Instance { get; private set; }

    private int selectedSlotIndex = 0;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else if (Instance != this) { Destroy(gameObject); return; }

        itemDictionary = ResolveItemDictionary();
        InitializeHotbar();
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
        HighlightSelectedSlot();
        DebugSelectedItem();
    }

    private void Update()
    {
        HandleHotbarInput();
    }

    private void HandleHotbarInput()
    {
        // Số 1-8 để chọn slot
        for (int i = 0; i < slotCount; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                selectedSlotIndex = i;
                HighlightSelectedSlot();
                DebugSelectedItem();
            }
        }
        // Cuộn chuột
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedSlotIndex = (selectedSlotIndex + 1) % slotCount;
            HighlightSelectedSlot();
            DebugSelectedItem();
        }
        else if (scroll < 0f)
        {
            selectedSlotIndex = (selectedSlotIndex - 1 + slotCount) % slotCount;
            HighlightSelectedSlot();
            DebugSelectedItem();
        }
    }

    private void HighlightSelectedSlot()
    {
        // Có thể thêm hiệu ứng highlight UI ở đây nếu muốn
        // Hiện tại để trống
    }

    public Item GetSelectedItem()
    {
        if (hotbarPanel == null || selectedSlotIndex < 0 || selectedSlotIndex >= hotbarPanel.transform.childCount)
            return null;
        var slot = hotbarPanel.transform.GetChild(selectedSlotIndex).GetComponent<Slot>();
        if (slot != null && slot.currentItem != null)
        {
            return slot.currentItem.GetComponent<Item>();
        }
        return null;
    }

    private void DebugSelectedItem()
    {
        Item selected = GetSelectedItem();
        if (selected != null)
        {
            Debug.Log($"[Hotbar] Selected Item: {selected.itemType}: {selected.ID}, {selected.Name}");
        }
        else
        {
            Debug.Log("[Hotbar] Selected Item: None");
        }
    }

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
            if (slot == null) continue;

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

                if (prefab == null)
                {
                    Debug.LogWarning($"[Hotbar] Load FAILED: không tìm thấy prefab với ID={itemData.itemID} ở slot {itemData.slotIndex}");
                    continue;
                }

                Item sourceItem = prefab.GetComponent<Item>();
                GameObject prefabToInstantiate = sourceItem != null && sourceItem.uiPrefab != null ? sourceItem.uiPrefab : prefab;

                if (sourceItem != null && sourceItem.uiPrefab == null)
                {
                    Debug.LogWarning($"[Hotbar] Item ID={itemData.itemID} ({sourceItem.Name}) không có uiPrefab → dùng prefab gốc, có thể bị lỗi hiển thị!");
                }

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

    public Slot FindFirstEmptySlot()
    {
        foreach (Transform slotTransform in hotbarPanel.transform)
        {
            Slot slot = slotTransform.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
                return slot;
        }
        return null;
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