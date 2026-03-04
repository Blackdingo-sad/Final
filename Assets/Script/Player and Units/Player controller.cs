using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
public class PlayerController2D : MonoBehaviour
{

    bool isBlocked;
    Vector2 blockNormal;   // HƯỚNG TỪ player -> obstacle (hướng bị chặn)
    float blockFrac;

    [Header("Move")]
    public float moveSpeed = 4f;              // tốc độ chạy
    public float flipDeadZone = 0.05f;        // ngưỡng để đổi hướng nhìn theo trục X

    [Header("Optional")]
    public Animator animator;                 // Animator có 2 state: Idle, Move
    public SpriteRenderer sprite;             // để flipX trái/phải


    Rigidbody2D rb;
    Vector2 input;        // raw WASD/Arrow
    Vector2 moveDir;      // normalized
    int lastFaceSign = 1; // 1 = nhìn phải, -1 = nhìn trái (dùng khi chỉ đi dọc)

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 0f;                  // top-down thì tắt trọng lực
        rb.freezeRotation = true;
        DontDestroyOnLoad(gameObject);
        if (!animator) animator = GetComponentInChildren<Animator>();
        if (!sprite) sprite = GetComponentInChildren<SpriteRenderer>();
    }

    void Update()
    {
        // 1) Lấy input & chuẩn hóa để đi chéo 
        input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        moveDir = input.normalized;

        // 2) Cập nhật hướng nhìn: chỉ đổi khi có X đáng kể
        if (Mathf.Abs(input.x) > flipDeadZone)
            lastFaceSign = input.x > 0 ? 1 : -1;

        if (sprite) sprite.flipX = (lastFaceSign < 0);

        // 3) Cập nhật Animator (chỉ cần 1 tham số speed)
        if (animator)
        {
            float speed = (moveDir * moveSpeed).magnitude; // 0 = idle, >0 = move
            animator.SetFloat("speed", speed);
        }
    }

    void FixedUpdate()
    {
        Vector2 dir = blocked ? Vector2.zero : moveDir;
        rb.MovePosition(rb.position + dir * moveSpeed * Time.fixedDeltaTime);
    }

    bool blocked = false;
    public void SetBlocked(bool b) { blocked = b; }

    public void SetBlock(bool block, Vector2 normal, float frac = 0f)
    {
        isBlocked = block;
        blockNormal = block ? (normal.sqrMagnitude > 0f ? normal.normalized : Vector2.zero) : Vector2.zero;
        blockFrac = block ? frac : 0f;
    }

}


