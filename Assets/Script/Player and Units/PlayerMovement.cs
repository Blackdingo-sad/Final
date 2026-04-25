using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float interactRange = 1.2f;
    private Rigidbody2D rb;
    private Vector2 moveInput;
    private Animator animator;
    public int numCarrotSeed = 0;

    public Transform Aim;
    bool isWalking = false;
    Vector2 lastDirection = Vector2.right;

    // Auto-move to target
    private Transform _moveTarget;
    private IInteractable _pendingInteract;

    public void MoveToAndInteract(Transform target, IInteractable interactable)
    {
        _moveTarget = target;
        _pendingInteract = interactable;
    }
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

        PlayerFarming farming = GetComponent<PlayerFarming>();
        if (farming != null && farming.IsBusy)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Auto-move to target plot on click
        if (_moveTarget != null)
        {
            Vector2 dir = (Vector2)_moveTarget.position - (Vector2)transform.position;
            if (dir.magnitude <= interactRange)
            {
                _pendingInteract?.Interact();
                _moveTarget = null;
                _pendingInteract = null;
                rb.linearVelocity = Vector2.zero;
            }
            else
            {
                rb.linearVelocity = dir.normalized * moveSpeed;
                animator.SetBool("isWalking", true);
            }
            return;
        }

        // Manual input cancels auto-move
        if (moveInput.magnitude > 0.1f)
        {
            _moveTarget = null;
            _pendingInteract = null;
        }

        rb.linearVelocity = moveInput * moveSpeed;
        animator.SetBool("isWalking", rb.linearVelocity.magnitude > 0);

        // Update last facing direction (4 directions: up/down/left/right)
        if (moveInput.magnitude > 0.1f)
        {
            if (Mathf.Abs(moveInput.x) >= Mathf.Abs(moveInput.y))
                lastDirection = moveInput.x > 0 ? Vector2.right : Vector2.left;
            else
                lastDirection = moveInput.y > 0 ? Vector2.up : Vector2.down;
        }

        // Rotate Aim transform to match facing direction
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
