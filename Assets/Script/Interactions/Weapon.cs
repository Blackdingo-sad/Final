using UnityEngine;

public class Weapon : MonoBehaviour
{
    public float damage = 2f;
    
    [Header("Target Filter")]
    [Tooltip("Gán tag 'Enemy' cho Enemy object trong Unity, rồi điền vào đây")]
    public string targetTag = "Enemy";

    // [Tooltip("Chọn Layer của Enemy trong Unity (Edit > Project Settings > Physics 2D)")]
    // public LayerMask targetLayer = ~0;

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Bỏ qua nếu không đúng tag
        if (!string.IsNullOrEmpty(targetTag) && !collision.CompareTag(targetTag))
            return;

        Debug.Log($"Weapon hit: {collision.gameObject.name} (Tag: {collision.tag}, Layer: {LayerMask.LayerToName(collision.gameObject.layer)})");

        // [OLD layer check]:
        // if ((targetLayer.value & (1 << collision.gameObject.layer)) == 0) return;

        Enemy_Movement enemy = collision.GetComponent<Enemy_Movement>();
        if (enemy == null) enemy = collision.GetComponentInParent<Enemy_Movement>();
        if (enemy == null) enemy = collision.GetComponentInChildren<Enemy_Movement>();

        if (enemy != null)
        {
            Debug.Log($"<color=green>Dealing {damage} damage to {enemy.gameObject.name}</color>");
            enemy.TakeDamage(damage);
        }
        else
        {
            // Debug.Log($"Ignored non-enemy object: {collision.gameObject.name}");
        }
    }
}
