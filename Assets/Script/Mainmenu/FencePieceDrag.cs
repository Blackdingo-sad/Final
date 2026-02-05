using UnityEngine;
using UnityEngine.EventSystems;

public class FencePieceDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int pieceId;

    private Canvas _rootCanvas;
    private CanvasGroup _cg;
    private RectTransform _rt;

    [HideInInspector] public Transform fromSlot;   // slot gốc lúc bắt đầu kéo
    [HideInInspector] public bool droppedOnSlot;   // được slot set true khi thả hợp lệ

    private void Awake()
    {
        _rt = GetComponent<RectTransform>();
        _cg = GetComponent<CanvasGroup>();
        _rootCanvas = GetComponentInParent<Canvas>().rootCanvas;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        droppedOnSlot = false;
        fromSlot = transform.parent;

        // kéo mượt: đưa ra ngoài slot lên Canvas (không bị layout/anchor kéo lại)
        transform.SetParent(_rootCanvas.transform, true);
        transform.SetAsLastSibling();

        // để raycast xuyên qua mảnh đang kéo -> bắt được slot phía dưới
        if (_cg) _cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // theo chuột
        _rt.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (_cg) _cg.blocksRaycasts = true;

        // nếu không thả lên slot nào -> trả về slot cũ
        if (!droppedOnSlot && fromSlot != null)
        {
            transform.SetParent(fromSlot, false);
            _rt.anchoredPosition = Vector2.zero;
        }
        if (droppedOnSlot)
        {
            FindObjectOfType<OrderPuzzleController>()?.CheckSolved();
        }

    }
}
