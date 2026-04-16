using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CanvasGroup))]
public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
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

        // 1. Tìm slot thả vào trước (hỗ trợ kéo giữa Inventory <-> Hotbar)
        Slot dropSlot = null;
        if (eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponent<Slot>()
                       ?? eventData.pointerEnter.GetComponentInParent<Slot>();
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
