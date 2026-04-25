using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionDetector : MonoBehaviour
{
    private IInteractable interactableInRange = null;
    public GameObject interactionIcon;

    // Track all interactables currently in range to pick the nearest one
    private readonly List<IInteractable> _inRangeList = new List<IInteractable>();

    void Start()
    {
        interactionIcon.SetActive(false);
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        RefreshNearest();

        if (interactableInRange == null)
        {
            Debug.Log("[InteractionDetector] E pressed but no interactable in range.");
            return;
        }

        Debug.Log($"[InteractionDetector] Interacting with: {(interactableInRange as MonoBehaviour)?.gameObject.name}");
        interactableInRange.Interact();

        if (!interactableInRange.CanInteract())
            interactionIcon.SetActive(false);
    }

    // Pick the nearest interactable from the list
    void RefreshNearest()
    {
        _inRangeList.RemoveAll(i => i == null || !(i as MonoBehaviour));

        IInteractable nearest = null;
        float nearestDist = float.MaxValue;

        foreach (IInteractable i in _inRangeList)
        {
            if (!i.CanInteract()) continue;
            MonoBehaviour mb = i as MonoBehaviour;
            if (mb == null) continue;

            float dist = Vector2.Distance(transform.position, mb.transform.position);
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = i;
            }
        }

        interactableInRange = nearest;
        interactionIcon.SetActive(interactableInRange != null);

        if (interactableInRange != null)
            Debug.Log($"[InteractionDetector] Nearest interactable: {(interactableInRange as MonoBehaviour)?.gameObject.name} ({nearestDist:F2}m)");
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
        if (interactable == null) return;

        if (!_inRangeList.Contains(interactable))
        {
            _inRangeList.Add(interactable);
            Debug.Log($"[InteractionDetector] Entered range: {(interactable as MonoBehaviour)?.gameObject.name}");
        }

        RefreshNearest();
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        IInteractable interactable = FindInteractable(collision);
        if (interactable == null) return;

        _inRangeList.Remove(interactable);
        Debug.Log($"[InteractionDetector] Left range: {(interactable as MonoBehaviour)?.gameObject.name}");

        RefreshNearest();
    }
}
