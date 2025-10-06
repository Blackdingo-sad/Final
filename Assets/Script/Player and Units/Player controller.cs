using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
    [SerializeField] float boxCastDistance = 0.08f;   // quét xuống thêm 8cm
    Collider2D col;
    [Header("Move")]
    public float moveSpeed = 8f;
    public float acceleration = 20f;    // tăng tốc
    public float deceleration = 30f;    // giảm tốc khi thả phím
    public float airControl = 0.6f;     // % điều khiển khi đang ở trên không

    [Header("Jump")]
    public float jumpForce = 13f;
    public int extraJumps = 0;          // 0 = chỉ nhảy 1 lần, >0 = double jump
    public float coyoteTime = 0.1f;     // “níu mép” nhảy muộn 0.1s sau khi rời đất
    public float jumpBuffer = 0.1f;     // bấm nhảy sớm 0.1s sẽ được “đệm” lại

    [Header("Ground Check")]
    public Transform groundCheck;       // đặt 1 empty dưới chân
    public float groundRadius = 0.15f;
    public LayerMask groundLayer;       // CHỌN Layer = Blockobject trong Inspector
    public bool drawGizmos = true;

    [Header("Optional")]
    public SpriteRenderer sprite;
    public Animator animator;

    private Rigidbody2D rb;
    private float inputX;
    private bool grounded;
    private bool jumpPressed;
    private float coyoteCounter;
    private float jumpBufferCounter;
    private int jumpsLeft;
    private bool facingRight = true;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        // Nên gán PhysicsMaterial2D (friction ~0) cho collider để tránh "dính tường"
        if (groundCheck == null)
            Debug.LogWarning("[PlayerController2D] groundCheck = NULL (hãy kéo 1 empty dưới chân vào).");
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
        if (!animator) animator = GetComponent<Animator>();   
    }

    bool IsGrounded()
    {
        // Ưu tiên OverlapCircle tại điểm groundCheck
        if (groundCheck && Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer))
            return true;

        // kiểm tra va chạm trực tiếp của collider với groundLayer
        if (col && col.IsTouchingLayers(groundLayer))
            return true;

        // BoxCast mỏng từ đáy chân xuống một đoạn nhỏ
        if (col)
        {
            var b = col.bounds;
            var size = new Vector2(b.size.x * 0.95f, 0.12f);
            var origin = new Vector2(b.center.x, b.min.y + size.y * 0.5f);
            var hit = Physics2D.BoxCast(origin, size, 0f, Vector2.down, boxCastDistance, groundLayer);
            if (hit.collider) return true;
        }
        return false;
    }

    void Update()
    {
        // Input
        inputX = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) jumpPressed = true;

        grounded = IsGrounded();

        if (grounded)
        {
            coyoteCounter = coyoteTime;
            jumpsLeft = extraJumps;
        }
        else
        {
            coyoteCounter -= Time.deltaTime;
        }

        // Jump buffer
        if (jumpPressed) jumpBufferCounter = jumpBuffer;
        else jumpBufferCounter -= Time.deltaTime;

        // Lật mặt
        if (inputX > 0 && !facingRight) Flip();
        else if (inputX < 0 && facingRight) Flip();

        // Animator 
        if (animator)
        {
            animator.SetFloat("speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("isGrounded", grounded);
            animator.SetFloat("vy", rb.velocity.y);
        }
        // theo dõi bấm phím
        if (Input.GetButtonDown("Jump")) { jumpPressed = true; Debug.Log("[Jump axis]"); }
        if (Input.GetKeyDown(KeyCode.Space)) Debug.Log("[Space]");
        if (Input.GetKeyDown(KeyCode.W)) Debug.Log("[W]");
        if (Input.GetKeyDown(KeyCode.UpArrow)) Debug.Log("[UpArrow]");
        if (Input.GetKeyDown(KeyCode.A)) Debug.Log("[A]");
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Debug.Log("[LeftArrow]");
        if (Input.GetKeyDown(KeyCode.D)) Debug.Log("[D]");
        if (Input.GetKeyDown(KeyCode.RightArrow)) Debug.Log("[RightArrow]");

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log($"[TryJump] grounded={grounded} coyote={coyoteCounter:0.00} jumpsLeft={jumpsLeft} velY={rb.velocity.y:0.00} body={rb.bodyType} grav={rb.gravityScale} constraints={rb.constraints}");
        }


    }

    void FixedUpdate()
    {
        // Di chuyển ngang với tăng/giảm tốc
        float target = inputX * moveSpeed;
        float accel = grounded
            ? (Mathf.Abs(target) > 0.01f ? acceleration : deceleration)
            : acceleration * airControl;

        float vx = Mathf.MoveTowards(rb.velocity.x, target, accel * Time.fixedDeltaTime);
        rb.velocity = new Vector2(vx, rb.velocity.y);

        // Xử lý nhảy (coyote + buffer + extra jumps)
        if (jumpBufferCounter > 0f)
        {
            if (coyoteCounter > 0f || jumpsLeft > 0)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0f); // reset Y để nhảy đều
                rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);

                if (!(coyoteCounter > 0f)) jumpsLeft--;      // trừ extra jump nếu nhảy trên không
                jumpBufferCounter = 0f;
            }
        }

        // Clear flag
        jumpPressed = false;
    }

    void Flip()
    {
        facingRight = !facingRight;
        if (sprite == null) sprite = GetComponentInChildren<SpriteRenderer>();
        if (sprite != null) sprite.flipX = !facingRight;
        else
        {
            Vector3 s = transform.localScale;
            s.x *= -1f;
            transform.localScale = s;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (!drawGizmos || groundCheck == null) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundRadius);
    }
}

