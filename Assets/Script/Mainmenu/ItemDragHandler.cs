using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    Transform originalParent;
    CanvasGroup canvasGroup;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // Update is called once per frame
public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        transform.SetParent(transform.root); 
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

        Slot dropSlot = eventData.pointerEnter?.GetComponent<Slot>();
        if (dropSlot == null)
        { 
            GameObject dropItem = eventData.pointerEnter;
            if (dropItem != null)
            {
                dropSlot = dropItem.GetComponentInParent<Slot>();
            }
        }
        Slot originalSlot = originalParent.GetComponent<Slot>();

        if (dropSlot != null)
        {
            if (dropSlot.curentItem == null)
            {
                dropSlot.curentItem.transform.SetParent(originalSlot.transform);
                originalSlot.curentItem = dropSlot.curentItem;
                dropSlot.curentItem.GetComponent<RectTransform>().anchoredPosition = Vector2.zero; // Center the item in the new slot
            }
            else
            {
                originalSlot.curentItem = null; 
            }
            transform.SetParent(dropSlot.transform); 
            dropSlot.curentItem = gameObject; 
        }
        else
        {
            transform.SetParent(originalParent); 
        }
        GetComponent<RectTransform>().anchoredPosition = Vector2.zero; 
    }
}
