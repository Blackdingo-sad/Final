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

        // 1. DROP nếu kéo ra ngoài inventory
        if (!IsWithinInventory(eventData.position))
        {
            if (originalSlot != null)
            {
                DropItem(originalSlot);
            }
            return;
        }

        // 2. Tìm slot thả vào
        Slot dropSlot = null;

        if (eventData.pointerEnter != null)
        {
            dropSlot = eventData.pointerEnter.GetComponent<Slot>();

            if (dropSlot == null)
            {
                dropSlot = eventData.pointerEnter.GetComponentInParent<Slot>();
            }
        }

        // 3. Nếu không hợp lệ → trả về slot cũ
        if (dropSlot == null || originalSlot == null)
        {
            transform.SetParent(originalParent);
            rect.anchoredPosition = Vector2.zero;
            return;
        }

        // 4. Nếu thả vào chính slot cũ → reset
        if (dropSlot == originalSlot)
        {
            transform.SetParent(originalParent);
            rect.anchoredPosition = Vector2.zero;
            return;
        }

        // 5. SLOT TRỐNG → MOVE
        if (dropSlot.currentItem == null)
        {
            transform.SetParent(dropSlot.transform);
            rect.anchoredPosition = Vector2.zero;

            dropSlot.currentItem = gameObject;
            originalSlot.currentItem = null;
            return;
        }

        // 6. SLOT CÓ ITEM → SWAP
        GameObject targetItem = dropSlot.currentItem;

        // swap reference
        dropSlot.currentItem = gameObject;
        originalSlot.currentItem = targetItem;

        // move item cũ về slot ban đầu
        if (targetItem != null)
        {
            targetItem.transform.SetParent(originalSlot.transform);

            RectTransform targetRect = targetItem.GetComponent<RectTransform>();
            if (targetRect != null)
            {
                targetRect.anchoredPosition = Vector2.zero;
            }
        }

        // move item hiện tại sang slot mới
        transform.SetParent(dropSlot.transform);
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
        originalSlot.currentItem = null;

        // FindPlayer
        Transform playerTransform = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (playerTransform == null)
        {
            Debug.LogError("Missing 'player' tag");
            return;
        }
        // Random drop position around player
        Vector2 dropOffset = Random.insideUnitCircle.normalized * Random.Range(minDropDistance, maxDropDistance);
        Vector2 dropPosition = (Vector2)playerTransform.position + dropOffset;

        //Destroy the UI one
        Item item = GetComponent<Item>();

        if (item != null && item.worldPrefab != null)
        {
            Instantiate(item.worldPrefab, dropPosition, Quaternion.identity);
        }
        Destroy(gameObject);

        InventoryController.Instance.RebuildItemCounts();
    }
}
