using UnityEngine;

public class ItemScaleStatus : MonoBehaviour
{
    [Header("Scale Settings")]
    public float worldScale = 8f;
    public float inventoryScale = 1f;

    private RectTransform rectTransform;
    private Transform cachedTransform;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        cachedTransform = transform;
        canvasGroup = GetComponent<CanvasGroup>();
    }

    void Update()
    {
        UpdateState();
    }

    void UpdateState()
    {
        // Nếu item nằm trong Canvas (Inventory/UI)
        if (GetComponentInParent<Canvas>() != null)
        {
            SetInventory();
        }
        else
        {
            SetWorld();
        }
    }

    void SetInventory()
    {
        cachedTransform.localScale = Vector3.one * inventoryScale;

        if (rectTransform != null)
        {
            bool isDragging = canvasGroup != null && !canvasGroup.blocksRaycasts;
            if (!isDragging)
            {
                rectTransform.anchoredPosition = Vector2.zero;
            }
        }
    }

    void SetWorld()
    {
        cachedTransform.localScale = Vector3.one * worldScale;
    }
}
