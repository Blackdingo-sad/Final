using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            interactableInRange?.Interact();
            if (interactableInRange != null && !interactableInRange.CanInteract())
            {
                interactionIcon.SetActive(false);
            }
        } 
    }

    private IInteractable FindInteractable(Collider2D collision)
    {
        if (collision.TryGetComponent(out IInteractable interactable))
            return interactable;

        interactable = collision.GetComponentInParent<MonoBehaviour>() as IInteractable
                    ?? collision.GetComponentInChildren<MonoBehaviour>() as IInteractable;

        return interactable;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        IInteractable interactable = FindInteractable(collision);
        if (interactable != null && interactable.CanInteract())
        {
            interactableInRange = interactable;
            interactionIcon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = FindInteractable(collision);
        if (interactable != null && interactable == interactableInRange)
        {
            interactableInRange = null;
            interactionIcon.SetActive(false);
        }
    }
}
