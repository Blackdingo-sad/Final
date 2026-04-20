using UnityEngine;

/// <summary>
/// Helper script ?? debug combat system. G?n vào Player ho?c Enemy ?? xem thông tin.
/// </summary>
public class CombatDebugger : MonoBehaviour
{
    [Header("Debug Options")]
    public bool showGizmos = true;
    public Color gizmoColor = Color.red;
    
    private Collider2D[] colliders;
    private Rigidbody2D rb;

    private void Start()
    {
        colliders = GetComponentsInChildren<Collider2D>();
        rb = GetComponent<Rigidbody2D>();
        
        LogSetupInfo();
    }

    void LogSetupInfo()
    {
        Debug.Log($"=== {gameObject.name} Combat Setup ===");
        Debug.Log($"Tag: {tag}");
        Debug.Log($"Layer: {LayerMask.LayerToName(gameObject.layer)}");
        
        if (rb != null)
        {
            Debug.Log($"Rigidbody2D: Body Type = {rb.bodyType}");
        }
        else
        {
            Debug.LogWarning("No Rigidbody2D found!");
        }
        
        Debug.Log($"Found {colliders.Length} collider(s):");
        foreach (var col in colliders)
        {
            Debug.Log($"  - {col.GetType().Name} on '{col.gameObject.name}': IsTrigger={col.isTrigger}, Enabled={col.enabled}");
        }
        
        // Check for combat scripts
        var enemy = GetComponent<MonoBehaviour>();
        if (enemy != null && enemy.GetType().Name == "Enemy") 
            Debug.Log("? Enemy script found");
        
        var weapon = GetComponentInChildren<MonoBehaviour>();
        if (weapon != null && weapon.GetType().Name == "Weapon") 
            Debug.Log("? Weapon script found");
        
        var attack = GetComponent<MonoBehaviour>();
        if (attack != null && attack.GetType().Name == "Attack") 
            Debug.Log("? Attack script found");
        
        Debug.Log("===================\n");
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos) return;
        
        Collider2D[] cols = GetComponentsInChildren<Collider2D>();
        Gizmos.color = gizmoColor;
        
        foreach (var col in cols)
        {
            if (col is BoxCollider2D box)
            {
                Gizmos.matrix = col.transform.localToWorldMatrix;
                Vector3 center = box.offset;
                Vector3 size = box.size;
                
                if (box.isTrigger)
                    Gizmos.DrawWireCube(center, size);
                else
                    Gizmos.DrawCube(center, size);
            }
            else if (col is CircleCollider2D circle)
            {
                Gizmos.DrawWireSphere(col.transform.position + (Vector3)circle.offset, circle.radius * col.transform.lossyScale.x);
            }
        }
    }
}
