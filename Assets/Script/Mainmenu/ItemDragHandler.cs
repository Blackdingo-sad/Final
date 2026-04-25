using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerDownHandler, IPointerClickHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    Canvas rootCanvas;
    Vector3 dragOffset;

    public float minDropDistance = 5f; 
    public float maxDropDistance = 8f; 


    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        
        Canvas parentCanvas = GetComponentInParent<Canvas>();
        if (parentCanvas != null)
        {
            rootCanvas = parentCanvas.rootCanvas;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        if (rectTransform == null)
        {
            rectTransform = GetComponent<RectTransform>();
        }
        if (rootCanvas == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null) rootCanvas = parentCanvas.rootCanvas;
        }

        // Đảm bảo root Image nhận raycast để drag hoạt động
        // Tắt raycastTarget trên child TMP_Text (text số lượng) để tránh chặn click
        // KHÔNG tắt child Image (icon) vì nó cần bubble event lên ItemDragHandler
        UnityEngine.UI.Graphic rootGraphic = GetComponent<UnityEngine.UI.Graphic>();
        if (rootGraphic != null)
        {
            rootGraphic.raycastTarget = true;
        }
        foreach (TMPro.TMP_Text text in GetComponentsInChildren<TMPro.TMP_Text>(true))
        {
            text.raycastTarget = false;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"[ItemDragHandler] PointerDown on: {gameObject.name}, pointerEnter: {eventData.pointerEnter?.name}, pointerPress: {eventData.pointerPress?.name}");
        if (eventData.pointerEnter != null)
        {
            Debug.Log($"[ItemDragHandler] pointerEnter hierarchy: {GetHierarchy(eventData.pointerEnter.transform)}");
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // Seed selection mode: click bất kỳ item nào để chọn trồng
        if (SeedSelectionManager.Instance != null && SeedSelectionManager.Instance.IsSelecting)
        {
            Item item = GetComponent<Item>();
            if (item != null)
                SeedSelectionManager.Instance.TrySelectItem(item);
            return;
        }

        if (!Input.GetKey(KeyCode.LeftControl) && !Input.GetKey(KeyCode.RightControl))
            return;

        Slot currentSlot = transform.parent.GetComponent<Slot>();
        if (currentSlot == null) return;

        // Xác định item đang ở inventory hay hotbar
        InventoryController inventory = InventoryController.Instance;
        HotbarController hotbar = Object.FindFirstObjectByType<HotbarController>();

        if (inventory == null || hotbar == null) return;

        bool isInInventory = transform.parent.parent == inventory.inventoryPanel.transform;

        if (isInInventory)
        {
            // Di chuyển từ inventory → hotbar slot trống đầu tiên
            Slot targetSlot = hotbar.FindFirstEmptySlot();
            if (targetSlot == null)
            {
                Debug.Log("[Ctrl+Click] Hotbar is full!");
                return;
            }
            MoveToSlot(currentSlot, targetSlot);
            Debug.Log($"[Ctrl+Click] Moved {gameObject.name} from Inventory → Hotbar");
        }
        else
        {
            // Di chuyển từ hotbar → inventory slot trống đầu tiên
            Slot targetSlot = FindFirstEmptySlotInPanel(inventory.inventoryPanel.transform);
            if (targetSlot == null)
            {
                Debug.Log("[Ctrl+Click] Inventory is full!");
                return;
            }
            MoveToSlot(currentSlot, targetSlot);
            Debug.Log($"[Ctrl+Click] Moved {gameObject.name} from Hotbar → Inventory");
        }
    }

    void MoveToSlot(Slot fromSlot, Slot toSlot)
    {
        transform.SetParent(toSlot.transform);
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        toSlot.currentItem = gameObject;
        fromSlot.currentItem = null;

        if (InventoryController.Instance != null)
            InventoryController.Instance.RebuildItemCounts();
    }

    Slot FindFirstEmptySlotInPanel(Transform panel)
    {
        foreach (Transform child in panel)
        {
            Slot slot = child.GetComponent<Slot>();
            if (slot != null && slot.currentItem == null)
                return slot;
        }
        return null;
    }

    string GetHierarchy(Transform t)
    {
        string path = t.name;
        while (t.parent != null)
        {
            t = t.parent;
            path = t.name + "/" + path;
        }
        return path;
    }

    // Update is called once per frame
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;

        if (rootCanvas == null)
        {
            Canvas parentCanvas = GetComponentInParent<Canvas>();
            if (parentCanvas != null) rootCanvas = parentCanvas.rootCanvas;
            if (rootCanvas == null) return;
        }

        // Lưu lại vị trí thực tế trước khi đổi parent
        Vector3 startPosition = rectTransform.position;

        // Chuyển ra canvas ngoài cùng (tránh bị kẹt trong nested Canvas/Panel)
        transform.SetParent(rootCanvas.transform, true);
        transform.SetAsLastSibling();

        // Ép lại vị trí cũ ngay lập tức để tránh layout group làm nhảy vị trí phút chót
        rectTransform.position = startPosition;

        Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventData.pressEventCamera;

        // Tính toán khoảng cách (offset) giữa con trỏ chuột và tâm của UI Item
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rectTransform, eventData.position, eventCamera, out Vector3 globalMousePos))
        {
            dragOffset = rectTransform.position - globalMousePos;
        }
        else
        {
            dragOffset = Vector3.zero;
        }

        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        if (rootCanvas == null || rectTransform == null) return;

        Camera eventCamera = rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : eventData.pressEventCamera;

        // Cập nhật vị trí trực tiếp theo tọa độ thế giới (world position) của Canvas
        if (RectTransformUtility.ScreenPointToWorldPointInRectangle(rootCanvas.transform as RectTransform, eventData.position, eventCamera, out Vector3 globalMousePos))
        {
            rectTransform.position = globalMousePos + dragOffset;
        }
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        RectTransform rect = rectTransform != null ? rectTransform : GetComponent<RectTransform>();
        Slot originalSlot = originalParent != null ? originalParent.GetComponent<Slot>() : null;

        // 1. Tìm slot bằng cách kiểm tra bounds - không phụ thuộc vào raycastTarget
        Slot dropSlot = null;
        foreach (Slot slot in Object.FindObjectsOfType<Slot>())
        {
            RectTransform slotRect = slot.GetComponent<RectTransform>();
            if (slotRect != null && RectTransformUtility.RectangleContainsScreenPoint(slotRect, eventData.position))
            {
                dropSlot = slot;
                Debug.Log($"[ItemDragHandler] Drop target slot: {slot.gameObject.name}");
                break;
            }
        }
        if (dropSlot == null)
        {
            Debug.Log("[ItemDragHandler] No slot found under cursor on EndDrag");
        }

        // 2. Nếu thả vào slot hợp lệ
        if (dropSlot != null && originalSlot != null)
        {
            // thả vào chính slot cũ
            if (dropSlot == originalSlot)
            {
                transform.SetParent(originalParent);
                rect.anchoredPosition = Vector2.zero;
                return;
            }

            // slot trống -> move
            if (dropSlot.currentItem == null)
            {
                transform.SetParent(dropSlot.transform);
                rect.anchoredPosition = Vector2.zero;

                dropSlot.currentItem = gameObject;
                originalSlot.currentItem = null;
                return;
            }

            // slot có item -> swap
            GameObject targetItem = dropSlot.currentItem;
            dropSlot.currentItem = gameObject;
            originalSlot.currentItem = targetItem;

            if (targetItem != null)
            {
                targetItem.transform.SetParent(originalSlot.transform);
                RectTransform targetRect = targetItem.GetComponent<RectTransform>();
                if (targetRect != null)
                {
                    targetRect.anchoredPosition = Vector2.zero;
                }
            }

            transform.SetParent(dropSlot.transform);
            rect.anchoredPosition = Vector2.zero;
            return;
        }

        // 3. Không thả vào slot nào: nếu ra ngoài inventory thì drop xuống đất, còn không thì trả về
        if (!IsWithinInventory(eventData.position))
        {
            if (originalSlot != null)
            {
                DropItem(originalSlot);
            }
            return;
        }

        transform.SetParent(originalParent);
        rect.anchoredPosition = Vector2.zero;
    }

    bool IsWithinInventory(Vector2 mousePosition )
    {
        if (originalParent == null || originalParent.parent == null) return false;

        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return inventoryRect != null && RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);

    }

    void DropItem(Slot originalSlot)
    {
        Item item = GetComponent<Item>();
        if (item == null)
        {
            transform.SetParent(originalParent);
            RectTransform rect = rectTransform != null ? rectTransform : GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            return;
        }

        // FindPlayer
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Missing 'player' tag");
            transform.SetParent(originalParent);
            RectTransform rect = rectTransform != null ? rectTransform : GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            return;
        }

        // Resolve world drop prefab
        GameObject worldDropPrefab = item.worldPrefab;
        if (worldDropPrefab == null)
        {
            ItemDictionary dictionary = Object.FindAnyObjectByType<ItemDictionary>();
            if (dictionary != null)
            {
                GameObject sourcePrefab = dictionary.GetItemPrefab(item.ID);
                if (sourcePrefab != null)
                {
                    Item sourceItem = sourcePrefab.GetComponent<Item>();
                    worldDropPrefab = sourceItem != null && sourceItem.worldPrefab != null ? sourceItem.worldPrefab : sourcePrefab;
                }
            }
        }

        if (worldDropPrefab == null)
        {
            Debug.LogWarning($"Cannot drop item ID {item.ID}: world prefab is missing.");
            transform.SetParent(originalParent);
            RectTransform rect = rectTransform != null ? rectTransform : GetComponent<RectTransform>();
            rect.anchoredPosition = Vector2.zero;
            return;
        }

        // Random drop position around player
        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        GameObject dropped = Instantiate(worldDropPrefab, dropPosition, Quaternion.identity);
        Item droppedItem = dropped.GetComponent<Item>();
        if (droppedItem != null)
        {
            droppedItem.ID = item.ID;
            droppedItem.quantity = item.quantity;
            droppedItem.UpdateQuantityDisplay();
        }

        originalSlot.currentItem = null;
        Destroy(gameObject);

        InventoryController.Instance.RebuildItemCounts();
    }
}
