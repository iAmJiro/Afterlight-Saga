using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkSpeed = 1.5f;
    public float runSpeed = 3f;
    public float jumpForce = 4f;
    
    public Vector3 targetPosition;
    private Rigidbody2D rb;
    private bool isRunning;
    private Vector2 moveInput;
    private Animator animator;
    private bool facingRight = true;
    public bool isGrounded;
    private bool isAttacking = false; // Track whether the player is attacking
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.gravityScale = 1; // Make sure gravity is enabled
        animator = GetComponent<Animator>();
    }
    
    void Update()
    {
        if (isGrounded)
        {
            if (Input.GetButtonDown("Jump"))
            {
                Jump();
            }
        }

        if (Input.GetKey(KeyCode.W))
        {
            transform.position += new Vector3(0, 0, 1) * Time.deltaTime;
            animator.SetFloat("xVelocity", 1);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            transform.position += new Vector3(0, 0, -1) * Time.deltaTime;
            animator.SetFloat("xVelocity", 1);
        }
        
        moveInput.x = Input.GetAxisRaw("Horizontal");
        moveInput.y = Input.GetAxisRaw("Vertical");

        isRunning = Input.GetKey(KeyCode.LeftShift);
        MovePlayer();

        // Allow attack input even when moving
        if (!isAttacking)
        {
            Attack();
        }
    }

    void FixedUpdate()
    {
        isGrounded = Physics2D.CircleCast(transform.position, 0.5f, Vector2.down, 0.05f);
        animator.SetFloat("xVelocity", Mathf.Abs(rb.velocity.x));
        animator.SetFloat("yVelocity", Mathf.Abs(rb.velocity.y));
    }

    void MovePlayer()
    {
        float speed = isRunning ? runSpeed : walkSpeed;
        rb.velocity = new Vector2(moveInput.x * speed, rb.velocity.y); // Keep the Y velocity for jumping

        if (moveInput.x > 0 && facingRight)
        {
            Flip();
        }
        else if (moveInput.x < 0 && !facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        Vector3 scaler = transform.localScale;
        scaler.x *= -1;
        transform.localScale = scaler;
    }

    void Jump()
    {
        rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    void Attack()
    {
        // Trigger the attack once when the attack button is pressed
        if (Input.GetKeyDown(KeyCode.J) && !isAttacking)
        {
            isAttacking = true; // Set the attacking flag
            animator.SetBool("isHeavyAttack", true); // Start the attack animation
            
            // Start a coroutine to handle attack end after the animation completes
            StartCoroutine(EndAttack());
        }
    }

    // Coroutine to reset the attack state after the animation is done
    IEnumerator EndAttack()
    {
        // Wait for the attack animation to complete (you may want to fine-tune the wait time)
        yield return new WaitForSeconds(animator.GetCurrentAnimatorStateInfo(0).length);

        animator.SetBool("isHeavyAttack", false); // Reset the attack animation
        isAttacking = false; // Allow attacking again
    }
}
