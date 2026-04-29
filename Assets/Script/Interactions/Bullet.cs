using UnityEngine;

using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Bullet : MonoBehaviour
{
    [Header("Settings")]
    public float speed = 12f;
    public float lifetime = 3f;

    private float _damage;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();
        _rb.gravityScale = 0f;
        _rb.freezeRotation = true;

        CircleCollider2D col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;

        Destroy(gameObject, lifetime);
    }

    /// <summary>Called by Attack.cs right after Instantiate</summary>
    public void Init(Vector2 direction, float damage)
    {
        _damage = damage;
        _rb.linearVelocity = direction * speed;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Ignore own player
        if (other.CompareTag("Player")) return;
        // Ignore other bullets
        if (other.GetComponent<Bullet>() != null) return;
        // Ignore anything that is NOT tagged Enemy
        if (!other.CompareTag("Enemy")) return;

        // Hit enemy via interface
        var enemy = other.GetComponent<IDamageable>();
        if (enemy != null)
        {
            Debug.Log($"[Bullet] Hit enemy '{other.gameObject.name}' for {_damage} damage.");
            enemy.TakeDamage(_damage);
        }
        else
        {
            // Fallback: SendMessage
            other.SendMessage("TakeDamage", _damage, SendMessageOptions.DontRequireReceiver);
            Debug.Log($"[Bullet] Hit '{other.gameObject.name}' for {_damage} damage (SendMessage).");
        }

        Destroy(gameObject);
    }
}
