using UnityEngine;
using UnityEngine;

public enum EnemyMoveType
{
    ChasePlayer, // ?u?i player khi phát hi?n — dùng cho enemy spawn phá cây
    Roam         // Di chuy?n random quanh map — dùng cho enemy s?n có ngoài map
}

public class Enemy_Movement : MonoBehaviour, IDamageable
{
    [Header("Movement Type")]
    [Tooltip("ChasePlayer: ?u?i player khi phát hi?n.\nRoam: ?i loanh quanh, ?u?i player khi ch?m vùng phát hi?n.")]
    public EnemyMoveType movementType = EnemyMoveType.ChasePlayer;

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

    [Header("Roam Settings (ch? dùng khi Roam)")]
    [Tooltip("Bán kính t?i ?a tính t? v? trí spawn mà enemy có th? ?i loanh quanh")]
    public float roamRadius = 4f;
    [Tooltip("Th?i gian ??ng ch? t?i m?i ?i?m tr??c khi ch?n ?i?m m?i")]
    public float roamWaitTime = 2f;

    private int facingDirection = -1;
    private CircleCollider2D detectionCollider;
    private Vector2 originalColliderOffset;

    private bool playerInRange = false;
    private bool hasBeenHit = false;

    // Roam state
    private enum RoamState { Moving, Waiting }
    private RoamState roamState = RoamState.Waiting;
    private Vector2 roamTarget;
    private float roamWaitTimer;
    private Vector2 spawnPosition;
    private const float ROAM_REACH_DIST = 0.25f;

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
            detectionCollider.isTrigger = true;
            originalColliderOffset = detectionCollider.offset;
        }

        health = maxHealth;
    }

    void Start()
    {
        spawnPosition = transform.position;

        if (movementType == EnemyMoveType.Roam)
        {
            roamWaitTimer = roamWaitTime;
            roamState = RoamState.Waiting;
        }
    }

    void Update()
    {
        if (PauseController.IsGamePaused) return;

        if (movementType == EnemyMoveType.Roam)
            UpdateRoam();
        else
            UpdateChase();
    }

    // ?? ChasePlayer mode ????????????????????????????????????????

    private void UpdateChase()
    {
        if (target != null && (playerInRange || hasBeenHit))
        {
            Vector3 direction = (target.position - transform.position).normalized;
            moveDirection = direction;
            FlipSprite(direction.x);
            FlipColliderOffset();
        }
        else
        {
            moveDirection = Vector2.zero;
        }
    }

    // ?? Roam mode ????????????????????????????????????????????????

    private void UpdateRoam()
    {
        // Khi phát hi?n ho?c ?ã b? hit ? ?u?i player nh? bình th??ng
        if (playerInRange || hasBeenHit)
        {
            if (target != null)
            {
                Vector3 direction = (target.position - transform.position).normalized;
                moveDirection = direction;
                FlipSprite(direction.x);
                FlipColliderOffset();
            }
            return;
        }

        // Roam logic
        switch (roamState)
        {
            case RoamState.Waiting:
                moveDirection = Vector2.zero;
                roamWaitTimer -= Time.deltaTime;
                if (roamWaitTimer <= 0f)
                {
                    PickNewRoamTarget();
                    roamState = RoamState.Moving;
                }
                break;

            case RoamState.Moving:
                Vector2 toTarget = roamTarget - (Vector2)transform.position;
                if (toTarget.magnitude <= ROAM_REACH_DIST)
                {
                    // ??n n?i ? ch? r?i ch?n ?i?m m?i
                    moveDirection = Vector2.zero;
                    roamWaitTimer = roamWaitTime;
                    roamState = RoamState.Waiting;
                }
                else
                {
                    moveDirection = toTarget.normalized;
                    FlipSprite(moveDirection.x);
                }
                break;
        }
    }

    private void PickNewRoamTarget()
    {
        Vector2 randomDir = Random.insideUnitCircle.normalized;
        float randomDist = Random.Range(roamRadius * 0.4f, roamRadius);
        roamTarget = spawnPosition + randomDir * randomDist;
    }

    // ?? FixedUpdate (dùng chung c? 2 mode) ??????????????????????

    private void FixedUpdate()
    {
        if (PauseController.IsGamePaused)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        rb.linearVelocity = moveDirection * moveSpeed;
    }

    // ?? CircleCollider2D detection ???????????????????????????????

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            target = collision.transform;
            playerInRange = true;
            Debug.Log($"<color=orange>[{gameObject.name}] Detected player — starting chase!</color>");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            playerInRange = false;
            if (!hasBeenHit)
            {
                target = null;
                moveDirection = Vector2.zero;

                // Roam mode: ti?p t?c ?i loanh quanh sau khi player ra kh?i vùng
                if (movementType == EnemyMoveType.Roam)
                {
                    roamWaitTimer = roamWaitTime;
                    roamState = RoamState.Waiting;
                }

                Debug.Log($"<color=grey>[{gameObject.name}] Lost player — stopping chase.</color>");
            }
        }
    }

    // ?? Damage / Death ???????????????????????????????????????????

    public void TakeDamage(float damage)
    {
        if (!hasBeenHit)
        {
            hasBeenHit = true;

            RaidEnemy raidBehavior = GetComponent<RaidEnemy>();
            if (raidBehavior != null)
                raidBehavior.SwitchToChaseMode();

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
            QuestController.Instance?.OnEnemyKilled(gameObject);
            Destroy(gameObject);
        }
    }

    // ?? Helpers ??????????????????????????????????????????????????

    private void FlipSprite(float directionX)
    {
        if (directionX > 0) facingDirection = 1;
        else if (directionX < 0) facingDirection = -1;

        if (spriteRenderer != null)
            spriteRenderer.flipX = (facingDirection == 1);
    }

    private void FlipColliderOffset()
    {
        if (detectionCollider != null)
            detectionCollider.offset = new Vector2(
                Mathf.Abs(originalColliderOffset.x) * facingDirection,
                originalColliderOffset.y);
    }

    private System.Collections.IEnumerator FlashHit()
    {
        spriteRenderer.color = hitColor;
        yield return new WaitForSeconds(0.1f);
        spriteRenderer.color = originalColor;
    }
}
