using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractor2D : MonoBehaviour
{
    public float radius = 1.2f;               // bán kính bắt gần
    public LayerMask interactableMask;        // chỉ Layer = Interactable
    public KeyCode interactKey = KeyCode.F;
    public InteractPromptUI promptUI;         // gán UI hiển thị nhắc bấm F (bên dưới)

    IInteractable current;

    void Update()
    {
        // Tâm dò (2D)
        Vector2 center = transform.position;

        // Tìm interactable gần nhất trong bán kính
        Collider2D[] hits = Physics2D.OverlapCircleAll(center, radius, interactableMask);
        IInteractable nearest = null;
        float best = float.MaxValue;

        foreach (var h in hits)
        {
            var it = h.GetComponentInParent<IInteractable>() ?? h.GetComponent<IInteractable>();
            if (it == null) continue;

            // dùng Collider2D.ClosestPoint -> Vector2
            Vector2 p = h.ClosestPoint(center);
            float d = (p - center).sqrMagnitude;   

            if (d < best) { best = d; nearest = it; }
        }

        current = nearest;

        if (promptUI)
            promptUI.SetVisible(current != null, current != null ? current.PromptMessage : null, interactKey);

        if(current != null && Input.GetKeyDown(interactKey))
{
            // LOG kiểm tra
            if (current is Component comp)
                Debug.Log($"[Interact] Pressed {interactKey} on '{comp.gameObject.name}' | Prompt='{current.PromptMessage}'");
            else
                Debug.Log($"[Interact] Pressed {interactKey} | Prompt='{current.PromptMessage}'");

            current.Interact(gameObject);
        }

    }


    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}

