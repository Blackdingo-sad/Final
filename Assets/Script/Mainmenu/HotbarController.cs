using System.Collections.Generic;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject hotbarPanel;
    [SerializeField] private GameObject slotPrefab;

    [Header("Settings")]
    [SerializeField] private int slotCount = 8;

    [Header("Lantern")]
    [SerializeField] private GameObject lanternLight;


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
        if (Keyboard.current == null) return;

        // Mảng key số tương ứng slot 1’9
        var digitKeys = new[]
        {
            Keyboard.current.digit1Key,
            Keyboard.current.digit2Key,
            Keyboard.current.digit3Key,
            Keyboard.current.digit4Key,
            Keyboard.current.digit5Key,
            Keyboard.current.digit6Key,
            Keyboard.current.digit7Key,
            Keyboard.current.digit8Key,
            Keyboard.current.digit9Key,
        };

        for (int i = 0; i < slotCount && i < digitKeys.Length; i++)
        {
            if (digitKeys[i].wasPressedThisFrame)
            {
                Debug.Log($"[Hotbar] Key {i + 1} → slot {i}");
                selectedSlotIndex = i;
                HighlightSelectedSlot();
                UpdateLanternLight();
                DebugSelectedItem();
                return;
            }
        }

        // Cuộn chuột (vẫn dùng old Input vì scroll chưa có API mới tiện)
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll > 0f)
        {
            selectedSlotIndex = (selectedSlotIndex + 1) % slotCount;
            HighlightSelectedSlot();
            UpdateLanternLight();
            DebugSelectedItem();
        }
        else if (scroll < 0f)
        {
            selectedSlotIndex = (selectedSlotIndex - 1 + slotCount) % slotCount;
            HighlightSelectedSlot();
            UpdateLanternLight();
            DebugSelectedItem();
        }
    }

    private void HighlightSelectedSlot()
    {
        if (hotbarPanel == null) return;
        for (int i = 0; i < hotbarPanel.transform.childCount; i++)
        {
            Image img = hotbarPanel.transform.GetChild(i).GetComponent<Image>();
            if (img != null)
                img.color = (i == selectedSlotIndex) ? new Color(1f, 1f, 0.4f, 1f) : Color.white;
        }
    }

    private void UpdateLanternLight()
    {
        if (lanternLight == null)
        {
            Debug.LogWarning("[Hotbar] lanternLight is NULL — drag Spot Light 2D into the Lantern Light field on HotbarController");
            return;
        }
        Item selected = GetSelectedItem();
        bool shouldActivate = selected != null && selected.itemType == ItemType.Lantern;
        lanternLight.SetActive(shouldActivate);
        Debug.Log($"[Hotbar] LanternLight → {(shouldActivate ? "ON" : "OFF")} (selected: {selected?.Name ?? "none"})");
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
            Debug.Log($"[Hotbar] Slot {selectedSlotIndex + 1} → {selected.Name} (Type: {selected.itemType}, ID: {selected.ID})");
        else
            Debug.Log($"[Hotbar] Slot {selectedSlotIndex + 1} → trống");
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