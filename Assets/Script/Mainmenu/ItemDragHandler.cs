using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;

    public float minDropDistance = 5f; 
    public float maxDropDistance = 8f; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        Canvas canvas = GetComponentInParent<Canvas>();
        transform.SetParent(canvas.transform);
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.6f;
    }
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;

        RectTransform rect = GetComponent<RectTransform>();
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
        if (dropSlot.curentItem == null)
        {
            transform.SetParent(dropSlot.transform);
            rect.anchoredPosition = Vector2.zero;

            dropSlot.curentItem = gameObject;
            originalSlot.curentItem = null;
            return;
        }

        // 6. SLOT CÓ ITEM → SWAP
        GameObject targetItem = dropSlot.curentItem;

        // swap reference
        dropSlot.curentItem = gameObject;
        originalSlot.curentItem = targetItem;

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
        RectTransform inventoryRect = originalParent.parent.GetComponent<RectTransform>();
        return RectTransformUtility.RectangleContainsScreenPoint(inventoryRect, mousePosition);

    }

    void DropItem(Slot originalSlot)
    {
        originalSlot.curentItem = null;

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
        //Instantiate drop item
        Instantiate(gameObject, dropPosition, Quaternion.identity);
        //Destroy the UI one
        Destroy(gameObject);
    }
}
