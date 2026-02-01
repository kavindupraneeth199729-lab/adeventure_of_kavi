using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControls : MonoBehaviour
{  public float speed = 5f; // Horizontal movement speed
    public float jumpForce = 10f; // Jump force
    private bool attack1Triggered = false;
private bool attack2Triggered = false;
private bool attack3Triggered = false;


    private Rigidbody2D rb;
    private Animator animator; // Animator component reference
    private bool isJumping = false; // To prevent double jumps
    private float moveDirection = 0f; // Tracks left (-1), right (1), or no input (0)
    private bool facingRight = true; // Tracks the current facing direction

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Get the Animator component
    }

    void Update()
    {
        // Update horizontal velocity
        rb.linearVelocity = new Vector2(moveDirection * speed, rb.linearVelocity.y);

        // Jump logic
        if (isJumping && Mathf.Abs(rb.linearVelocity.y) < 0.001f)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
            isJumping = false; // Reset jump
        }

        // Flip the character when changing directions
        if (moveDirection > 0 && !facingRight)
        {
            Flip();
        }
        else if (moveDirection < 0 && facingRight)
        {
            Flip();
        }

        // Update animator parameters
        animator.SetFloat("xVelocity", Mathf.Abs(rb.linearVelocity.x)); // Absolute value for horizontal movement
        animator.SetFloat("yVelocity", rb.linearVelocity.y); // Vertical velocity
        animator.SetBool("isJumping", !IsGrounded()); // Set isJumping based on ground status
    }

    // Methods for button events
    public void MoveLeft()
    {
        moveDirection = -1f; // Start moving left
    }

    public void MoveRight()
    {
        moveDirection = 1f; // Start moving right
    }

    public void StopMoving()
    {
        moveDirection = 0f; // Stop horizontal movement when the button is released
    }

    public void Jump()
    {
        if (IsGrounded()) // Check if the player is on the ground
        {
            isJumping = true;
        }
    }

    // Flip the character's direction instantly
    private void Flip()
    {
        facingRight = !facingRight; // Toggle the facing direction
        Vector3 scale = transform.localScale;
        scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x); // Ensure consistent flipping
        transform.localScale = scale; // Apply the updated scale
    }

    // Check if the player is on the ground
    private bool IsGrounded()
    {
        // Simple ground check using the player's vertical velocity
        return Mathf.Abs(rb.linearVelocity.y) < 0.001f;
    }
   public void Attack1()
{
    attack1Triggered = true;
}

public void Attack2()
{
    attack2Triggered = true;
}

public void Attack3()
{
    attack3Triggered = true;
}

void FixedUpdate()
{
    if (attack1Triggered)
    {
        animator.SetTrigger("Attack1");
        attack1Triggered = false;
    }
    if (attack2Triggered)
    {
        animator.SetTrigger("Attack2");
        attack2Triggered = false;
    }
    if (attack3Triggered)
    {
        animator.SetTrigger("Attack3");
        attack3Triggered = false;
    }
}

    }
