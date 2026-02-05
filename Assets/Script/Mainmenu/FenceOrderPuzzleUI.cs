using System;
using System.Linq;
using UnityEngine;
using TMPro;

public class FenceOrderPuzzleUI : MonoBehaviour
{
    public static FenceOrderPuzzleUI I;

    [Header("UI")]
    public GameObject panel;
    public TMP_Text feedbackText;

    [Header("Slots")]
    public Transform slotsParent; // SlotsParent chứa Slot_0..Slot_N

    private FenceSlotDrop[] _slots;
    private Action _onSolved;
    private Action _onClosed;

    private void Awake()
    {
        I = this;
        if (panel) panel.SetActive(false);

        _slots = slotsParent.GetComponentsInChildren<FenceSlotDrop>(true)
                            .OrderBy(s => s.transform.GetSiblingIndex())
                            .ToArray();
    }

    public void Open(Action onSolved, Action onClosed = null)
    {
        _onSolved = onSolved;
        _onClosed = onClosed;

        if (feedbackText) feedbackText.text = "";
        panel.SetActive(true);

        AssignIdsIfMissing();
        ShufflePieces();

        // tránh trường hợp xáo trộn xong lại đúng luôn
        if (IsSolved()) ShufflePieces();
    }

    public void Close()
    {
        panel.SetActive(false);
        _onClosed?.Invoke();
        _onClosed = null;
        _onSolved = null;
    }

    private void AssignIdsIfMissing()
    {
        // Mặc định: slot 0 đúng là mảnh 1, slot 1 đúng là mảnh 2, ...
        for (int i = 0; i < _slots.Length; i++)
        {
            var piece = _slots[i].GetPieceInSlot();
            if (piece == null) continue;

            piece.pieceId = i + 1;

            // nếu có TMP label con -> set text
            var label = piece.GetComponentInChildren<TMP_Text>(true);
            if (label) label.text = piece.pieceId.ToString();
        }
    }

    private void ShufflePieces()
    {
        // Hoán đổi ngẫu nhiên mảnh giữa các slot
        for (int i = 0; i < _slots.Length; i++)
        {
            int j = UnityEngine.Random.Range(0, _slots.Length);
            SwapPieces(_slots[i], _slots[j]);
        }

        // Cập nhật lại label theo pieceId (label đi theo object nên không cần đổi)
    }

    private void SwapPieces(FenceSlotDrop a, FenceSlotDrop b)
    {
        if (a == b) return;

        var pa = a.GetPieceInSlot();
        var pb = b.GetPieceInSlot();
        if (pa == null || pb == null) return;

        Transform sa = pa.transform.parent;
        Transform sb = pb.transform.parent;

        pa.transform.SetParent(sb, false);
        ((RectTransform)pa.transform).anchoredPosition = Vector2.zero;

        pb.transform.SetParent(sa, false);
        ((RectTransform)pb.transform).anchoredPosition = Vector2.zero;
    }

    private bool IsSolved()
    {
        for (int i = 0; i < _slots.Length; i++)
        {
            var p = _slots[i].GetPieceInSlot();
            if (p == null) return false;
            if (p.pieceId != i + 1) return false;
        }
        return true;
    }

    public void CheckSolved()
    {
        if (!panel.activeSelf) return;

        if (IsSolved())
        {
            if (feedbackText) feedbackText.text = "Đúng rồi!";
            _onSolved?.Invoke();
            Close();
        }
        else
        {
            if (feedbackText) feedbackText.text = "";
        }
    }
}
