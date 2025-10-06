using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    string PromptMessage { get; }
    void Interact(GameObject interactor);
}


[RequireComponent(typeof(Collider2D))]
public class Interactable2D : MonoBehaviour, IInteractable
{
    [TextArea] public string prompt = "Tương tác";
    public bool oneShot = false;           // dùng 1 lần rồi ẩn
    public UnityEvent onInteract;          // kéo thả sự kiện trong Inspector

    Collider2D col;

    public string PromptMessage => prompt;

    void Reset()
    {
        col = GetComponent<Collider2D>();
        col.isTrigger = true;                              // chỉ để bắt gần, không va chạm
        gameObject.layer = LayerMask.NameToLayer("Interactable");
    }

    public void Interact(GameObject interactor)
    {
        onInteract?.Invoke();
        if (oneShot && col) col.enabled = false;
    }
}

