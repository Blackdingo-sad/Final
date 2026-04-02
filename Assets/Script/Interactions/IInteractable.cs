using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public interface IInteractable
{
    void Interact();
    bool CanInteract();
}
//    string PromptMessage { get; }
//    void Interact(GameObject interactor);
//}


//[RequireComponent(typeof(Collider2D))]
//public class Interactable2D : MonoBehaviour, IInteractable
//{
//    [TextArea] public string prompt = "Tương tác";
//    public bool oneShot = false;
//    public UnityEvent onInteract;

//    Collider2D col;

//    public string PromptMessage => prompt;

//    public bool CanInteract()
//    {
//        throw new System.NotImplementedException();
//    }

//    public void Interact()
//    {
//        throw new System.NotImplementedException();
//    }

//    public void Interact(GameObject interactor)
//    {
//        throw new System.NotImplementedException();
//    }

//    void Reset()
//    {
//        col = GetComponent<Collider2D>();
//        col.isTrigger = true;
//        gameObject.layer = LayerMask.NameToLayer("Interactable");
//    }

//    //public void Interact(GameObject interactor)
//    //{
//    //    onInteract?.Invoke();
//    //    if (oneShot && col) col.enabled = false;
//    //}
//}

