using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrderPuzzleController : MonoBehaviour
{
    [Header("Slots")]
    [SerializeField] private Transform slotsParent;

    [Header("State")]
    [SerializeField] private bool isSolved = false;
     public bool IsSolved => isSolved;


    public System.Action OnPuzzleSolved;

    private void OnEnable()
    {
        if (!IsSolved)
            Randomize();
    }

    
    void Randomize()
    {
        var pieces = new System.Collections.Generic.List<Transform>();

        foreach (Transform slot in slotsParent)
        {
            if (slot.childCount > 0)
                pieces.Add(slot.GetChild(0));
        }

        // Fisher–Yates
        for (int i = pieces.Count - 1; i > 0; i--)
        {
            int j = Random.Range(0, i + 1);
            (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
        }

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            pieces[i].SetParent(slotsParent.GetChild(i));
            pieces[i].localPosition = Vector3.zero;
        }
    }

    
    public void CheckSolved()
    {
        string orderLog = "[Puzzle Order] ";

        for (int i = 0; i < slotsParent.childCount; i++)
        {
            if (slotsParent.GetChild(i).childCount == 0)
            {
                Debug.LogWarning($"Slot {i} is empty");
                return;
            }

            var piece = slotsParent.GetChild(i).GetChild(0);
            var data = piece.GetComponent<PuzzlePiece>();

            if (data == null)
            {
                Debug.LogError($"Slot {i} has NO PuzzlePiece component!");
                return;
            }

            orderLog += data.Value + " ";

            if (data.Value != i + 1)
            {
                Debug.Log(orderLog);
                Debug.Log($"❌ Not solved: Slot {i} has value {data.Value}, expected {i + 1}");
                return;
            }
        }

       
        Debug.Log(orderLog);
        Debug.Log("✅ Puzzle solved!");

        isSolved = true;
        OnPuzzleSolved?.Invoke();
    }

}
