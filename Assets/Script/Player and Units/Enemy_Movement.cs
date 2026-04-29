using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    public float moveSpeed = 2f;
    Transform target;
    Rigidbody2D rb;
    Vector2 moveDirection;

    [Header("Stats")]
    public float maxHealth = 10f;
    float health;
    
    [Header("Visual Feedback")]
    public bool flashOnHit = true;
    public Color hitColor = Color.red;
    private SpriteRenderer spriteRenderer;
    private Color originalColor;

    private int facingDirection = -1; // -1 = nhìn trái (default), 1 = nhìn ph?i
    private CircleCollider2D detectionCollider;
    private Vector2 originalColliderOffset; // l?u offset g?c ?? flip ?úng

    private bool playerInRange = false; // ch? ?u?i khi player trong vùng phát hi?n
    private bool hasBeenHit = false; // m?t khi b? damage, lu\u00f4n ?u?i player

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.freezeRotation = true;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
            originalColor = spriteRenderer.color;

        detectionCollider = GetComponent<CircleCollider2D>();
        if (detectionCollider != null)
        {
            detectionCollider.isTrigger = true; // ??m b?o là trigger ?? detect player
            originalColliderOffset = detectionCollider.offset;
        }
    }

    void Start()
    {
        // Không t? tìm player n?a, ch? player b??c vào CircleCollider2D
        // target = GameObject.FindGameObjectWithTag("Player").transform;
        health = maxHealth;
    }

    void Update()
    {
        if (PauseController.IsGamePaused) return;

        if (target != null && (playerInRange || hasBeenHit))
        {
            Vector3 direction = (target.position - transform.position).normalized;
            moveDirection = direction;

            // Flip sprite theo h??ng player
            if (target.position.x > transform.position.x)
                facingDirection = 1;
            else
                facingDirection = -1;

            if (spriteRenderer != null)
                spriteRenderer.flipX = (facingDirection == 1);

            // Flip offset c?a CircleCollider2D theo h??ng nhìn
            if (detectionCollider != null)
                detectionCollider.offset = new Vector2(Mathf.Abs(originalColliderOffset.x) * facingDirection, originalColliderOffset.y);
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    private void FixedUpdate()
    {
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = (playerInRange || hasBeenHit) ? moveDirection * moveSpeed : Vector2.zero;
    }

    // Player b??c vào vùng CircleCollider2D ? b?t ??u ?u?i
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            target = collision.transform;
            playerInRange = true;
            Debug.Log($"<color=orange>[{gameObject.name}] Detected player — starting chase!</color>");
        }
    }

    // Player ra kh?i vùng CircleCollider2D ? d?ng ?u?i (ch? khi ch?a b? damage)
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (!hasBeenHit)
            {
                target = null;
                moveDirection = Vector2.zero;
                Debug.Log($"<color=grey>[{gameObject.name}] Lost player — stopping chase.</color>");
            }
        }
    }

    public void TakeDamage(float damage)
    {
        if (!hasBeenHit)
        {
            hasBeenHit = true;
            // T?m player n?u ch?a c?
            if (target == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null) target = player.transform;
            }
            Debug.Log($"<color=red>[{gameObject.name}] First hit! Now chasing player forever!</color>");
        }
        health -= damage;
        Debug.Log($"<color=red>{gameObject.name} took {damage} damage. Health: {health}/{maxHealth}</color>");
        
        if (flashOnHit && spriteRenderer != null)
            StartCoroutine(FlashHit());
        
        if (health <= 0)
        {
            Debug.Log($"<color=yellow>{gameObject.name} died!</color>");
            Destroy(gameObject);
        }
    }
    
    private System.Collections.IEnumerator FlashHit()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}
