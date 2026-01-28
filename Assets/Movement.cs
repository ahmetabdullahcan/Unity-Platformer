using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class Movement : MonoBehaviour
{
    [SerializeField] private float speed = 2f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private PlayerInput playerInput;

    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundCheckRadius = 0.2f;

    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Jump Settings")]
    [SerializeField] private float fallMultiplier = 2.5f;
    [SerializeField] private float lowJumpMultiplier = 2f;
    [SerializeField] private float coyoteTime = 0.2f;
    [SerializeField] private float jumpBufferTime = 0.2f;

    private bool isGrounded;
    private bool wasGrounded; // Önceki frame'de yerde olup olmadığını tutacak
    private float coyoteTimeCounter;
    private float jumpBufferCounter;

    private bool IsGrounded()
    {
        Collider2D collider = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);
        return collider != null;
    }

    void Update()
    {
        wasGrounded = isGrounded;
        isGrounded = IsGrounded();
        
        animator.SetBool("isGrounded", isGrounded);

        // Coyote time sadece yerden havaya çıktığımızda başlar
        // Zıpladıktan sonra değil
        if (isGrounded)
        {
            coyoteTimeCounter = coyoteTime;
        }
        else if (wasGrounded && !isGrounded)
        {
            // Yerden yeni ayrıldık, coyote time başlat
            coyoteTimeCounter = coyoteTime;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (playerInput.actions["Jump"].triggered)
        {
            jumpBufferCounter = jumpBufferTime;
        }
        else
        {
            jumpBufferCounter -= Time.deltaTime;
        }

        Vector2 movementInput = playerInput.actions["Move"].ReadValue<Vector2>();
        Vector2 movement = new(movementInput.x * speed, rb.linearVelocity.y);
        rb.linearVelocity = movement;

        animator.SetFloat("Velocity X", Mathf.Abs(movementInput.x));
        animator.SetFloat("Velocity Y", rb.linearVelocity.y);

        if (jumpBufferCounter > 0f && coyoteTimeCounter > 0f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            jumpBufferCounter = 0f;
            coyoteTimeCounter = 0f; // BU ÇOK ÖNEMLİ! Zıpladıktan sonra coyote time'ı sıfırla
            Debug.Log("Jumped");
        }

        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += (fallMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        }
        else if (rb.linearVelocity.y > 0 && !playerInput.actions["Jump"].IsPressed())
        {
            rb.linearVelocity += (lowJumpMultiplier - 1) * Physics2D.gravity.y * Time.deltaTime * Vector2.up;
        }

        if (movementInput.x > 0)
        {
            spriteRenderer.flipX = false;
        }
        else if (movementInput.x < 0)
        {
            spriteRenderer.flipX = true;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
        }
    }
}