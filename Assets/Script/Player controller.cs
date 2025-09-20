using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{
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
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        // Nên gán PhysicsMaterial2D (friction ~0) cho collider để tránh "dính tường"
    }

    void Update()
    {
        // Input
        inputX = Input.GetAxisRaw("Horizontal");
        if (Input.GetButtonDown("Jump")) jumpPressed = true;

        // Ground check (đứng trên Blockobject)
        grounded = Physics2D.OverlapCircle(groundCheck.position, groundRadius, groundLayer);
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

        // Animator (nếu có)
        if (animator)
        {
            animator.SetFloat("speed", Mathf.Abs(rb.velocity.x));
            animator.SetBool("isGrounded", grounded);
            animator.SetFloat("vy", rb.velocity.y);
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
