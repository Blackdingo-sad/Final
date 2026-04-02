using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    private NPC currentInteractable;


    void Update()
    {
        if (currentInteractable != null && Input.GetKeyDown(KeyCode.F))
        {
            if (currentInteractable.CanInteract())
            {
               // currentInteractable.Interact(gameObject);
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        NPC interactable = collision.GetComponent<NPC>();

        if (interactable != null)
        {
            currentInteractable = interactable;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        NPC interactable = collision.GetComponent<NPC>();

        if (interactable != null && currentInteractable == interactable)
        {
            currentInteractable = null;
        }
    }
}