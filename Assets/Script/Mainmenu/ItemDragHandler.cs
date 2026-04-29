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
        // Seed selection mode
        if (SeedSelectionManager.Instance != null && SeedSelectionManager.Instance.IsSelecting)
        {
            Item item = GetComponent<Item>();
            if (item != null)
                SeedSelectionManager.Instance.TrySelectItem(item);
            return;
        }

        // Ctrl + Click = split stack
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            SplitStack();
            return;
        }
    }

    void SplitStack()
    {
        Item item = GetComponent<Item>();
        if (item == null || item.quantity <= 1) return;

        Slot currentSlot = transform.parent.GetComponent<Slot>();
        if (currentSlot == null) return;

        int splitAmount = item.quantity / 2;
        item.quantity -= splitAmount;
        item.UpdateQuantityDisplay();

        // Tìm slot trống gần nhất để đặt nửa còn lại
        Slot emptySlot = FindEmptySlotAnywhere();
        if (emptySlot == null)
        {
            // Không có slot trống → hoàn tác
            item.quantity += splitAmount;
            item.UpdateQuantityDisplay();
            Debug.Log("[Split] Không có slot trống để đặt stack mới!");
            return;
        }

        SpawnSplitItem(item, splitAmount, emptySlot);
        InventoryController.Instance?.RebuildItemCounts();
        Debug.Log($"[Split] {item.Name}: {item.quantity} | {splitAmount}");
    }

    void SpawnSplitItem(Item sourceItem, int qty, Slot targetSlot)
    {
        ItemDictionary dict = Object.FindAnyObjectByType<ItemDictionary>();
        if (dict == null) return;

        GameObject prefab = dict.GetItemPrefab(sourceItem.ID);
        if (prefab == null) return;

        Item src = prefab.GetComponent<Item>();
        GameObject uiPrefab = (src != null && src.uiPrefab != null) ? src.uiPrefab : prefab;

        GameObject newObj = Instantiate(uiPrefab, targetSlot.transform);
        newObj.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;

        if (newObj.GetComponent<CanvasGroup>() == null) newObj.AddComponent<CanvasGroup>();
        if (newObj.GetComponent<ItemDragHandler>() == null) newObj.AddComponent<ItemDragHandler>();

        Item newItem = newObj.GetComponent<Item>();
        if (newItem != null)
        {
            newItem.ID = sourceItem.ID;
            newItem.quantity = qty;
            newItem.UpdateQuantityDisplay();
        }
        targetSlot.currentItem = newObj;
    }

    Slot FindEmptySlotAnywhere()
    {
        // Tìm trong Inventory trước
        if (InventoryController.Instance != null)
        {
            Slot s = FindFirstEmptySlotInPanel(InventoryController.Instance.inventoryPanel.transform);
            if (s != null) return s;
        }
        // Rồi tìm trong Hotbar
        Slot hs = HotbarController.Instance?.FindFirstEmptySlot();
        return hs;
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

        // Ctrl + Drag = split: tách nửa stack, giữ phần còn lại trong slot gốc
        Item item = GetComponent<Item>();
        if (item != null && item.quantity > 1 &&
            (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            int splitAmount = item.quantity / 2;
            int remaining   = item.quantity - splitAmount;

            Slot origSlot = originalParent.GetComponent<Slot>();
            if (origSlot != null)
            {
                // Tạo item mới với số lượng còn lại, đặt vào slot gốc
                SpawnSplitItem(item, remaining, origSlot);
            }

            // Item hiện tại chỉ còn splitAmount để drag
            item.quantity = splitAmount;
            item.UpdateQuantityDisplay();
            Debug.Log($"[Split-Drag] Dragging {splitAmount}, left {remaining} in slot");
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

            // slot có item
            Item draggedItem = GetComponent<Item>();
            Item targetItem  = dropSlot.currentItem.GetComponent<Item>();

            // Cùng loại → combine stack
            if (draggedItem != null && targetItem != null && draggedItem.ID == targetItem.ID)
            {
                targetItem.AddToStack(draggedItem.quantity);
                originalSlot.currentItem = null;
                Destroy(gameObject);
                InventoryController.Instance?.RebuildItemCounts();
                Debug.Log($"[Combine] {targetItem.Name} x{targetItem.quantity}");
                return;
            }

            // Khác loại → swap
            GameObject targetObj = dropSlot.currentItem;
            dropSlot.currentItem = gameObject;
            originalSlot.currentItem = targetObj;

            if (targetObj != null)
            {
                targetObj.transform.SetParent(originalSlot.transform);
                RectTransform targetRect = targetObj.GetComponent<RectTransform>();
                if (targetRect != null) targetRect.anchoredPosition = Vector2.zero;
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
