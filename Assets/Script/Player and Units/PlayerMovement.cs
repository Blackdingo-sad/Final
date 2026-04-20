using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    public int numCarrotSeed = 0;

    public Transform Aim;
    bool isWalking = false;
    Vector2 lastDirection = Vector2.right;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

    }

    // Update is called once per frame
    void Update()
    {
        if (PauseController.IsGamePaused)
        {
            if (rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = Vector2.zero;
                StopMovementAnimations();
            }
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
        animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0);

        // C?p nh?t h??ng nhìn cu?i cùng (4 h??ng: lên/xu?ng/trái/ph?i)
        if (moveInput.magnitude > 0.1f)
        {
            if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y))
                lastDirection = moveInput.x > 0 ? Vector2.right : Vector2.left;
            else
                lastDirection = moveInput.y > 0 ? Vector2.up : Vector2.down;
        }

        // Xoay Aim theo h??ng nhân v?t
        if (Aim != null)
        {
            float angle = Mathf.Atan2(lastDirection.y, lastDirection.x) * Mathf.Rad2Deg;
            Aim.rotation = Quaternion.Euler(0f, 0f, angle);
        }
    }
    private void FixedUpdate()
    {
    
    }

    void StopMovementAnimations()
    {
        animator.SetBool("isWalking", false);
        animator.SetFloat("LastInputX", moveInput.x);
        animator.SetFloat("LastInputY", moveInput.y);
    }

    public void Move(InputAction.CallbackContext context)
    {
        if (context.canceled)
        {
            StopMovementAnimations();
        }   
        moveInput = context.ReadValue<Vector2>();
        animator.SetFloat("InputX", moveInput.x);
        animator.SetFloat("InputY", moveInput.y);

    }
}
