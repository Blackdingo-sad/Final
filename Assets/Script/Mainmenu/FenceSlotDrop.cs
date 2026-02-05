using UnityEngine;
using UnityEngine.EventSystems;

public class FenceSlotDrop : MonoBehaviour, IDropHandler
{
    [SerializeField] private OrderPuzzleController puzzleController;
    public FencePieceDrag GetPieceInSlot()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var p = transform.GetChild(i).GetComponent<FencePieceDrag>();
            if (p != null) return p;
        }
        return null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        var draggedGO = eventData.pointerDrag;
        if (draggedGO == null) return;

        var draggedPiece = draggedGO.GetComponent<FencePieceDrag>();
        if (draggedPiece == null) return;

        Transform toSlot = transform;
        Transform fromSlot = draggedPiece.fromSlot;

        if (fromSlot == null) return;
        if (toSlot == fromSlot)
        {
          
            draggedPiece.transform.SetParent(fromSlot, false);
            ((RectTransform)draggedPiece.transform).anchoredPosition = Vector2.zero;
            draggedPiece.droppedOnSlot = true;
            return;
        }

       
        FencePieceDrag targetPiece = GetPieceInSlot();
        if (targetPiece != null)
        {
            targetPiece.transform.SetParent(fromSlot, false);
            ((RectTransform)targetPiece.transform).anchoredPosition = Vector2.zero;
            RectTransform rt = (RectTransform)draggedPiece.transform;
            rt.anchoredPosition = Vector2.zero;
            rt.localScale = Vector3.one;
        }

        draggedPiece.transform.SetParent(toSlot, false);
        ((RectTransform)draggedPiece.transform).anchoredPosition = Vector2.zero;

        draggedPiece.droppedOnSlot = true;
        puzzleController.CheckSolved();
    }
}
