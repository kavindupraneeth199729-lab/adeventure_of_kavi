using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerControls : MonoBehaviour
{  public float speed = 5f; // Horizontal movement speed
    public float jumpForce = 10f; // Jump force

    // HEALTH SYSTEM
    public int maxHealth = 20;
    private int currentHealth;
    private bool attack1Triggered = false;
private bool attack2Triggered = false;
private bool attack3Triggered = false;


    private Rigidbody2D rb;
    private Animator animator; // Animator component reference
    public Transform attackPoint; // Point from which to detect enemies
    public float attackRange = 1.5f; // Range of the attack
    public LayerMask enemyLayers = ~0; // Layers to consider as enemies (Everything by default)
    private bool isJumping = false; // To prevent double jumps
    private float moveDirection = 0f; // Tracks left (-1), right (1), or no input (0)
    private bool facingRight = true; // Tracks the current facing direction

    private bool isAttacking = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>(); // Get the Animator component
        
        // Initialize Health
        currentHealth = maxHealth;
        
        // Ensure UI exists
        if (HealthUI.Instance == null)
        {
             GameObject uiObj = new GameObject("HealthManager");
             uiObj.AddComponent<HealthUI>();
        }
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
        
        // Only update Jump state if NOT attacking to prevent overriding attack animation
        if (!isAttacking) 
        {
            animator.SetBool("isJumping", !IsGrounded());
        }
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
        PerformAttack();
        StartCoroutine(AttackCooldown("attack"));
    }

    public void Attack2()
    {
        attack2Triggered = true;
        PerformAttack();
        StartCoroutine(AttackCooldown("attack2"));
    }

    public void Attack3()
    {
        attack3Triggered = true;
        PerformAttack();
        StartCoroutine(AttackCooldown("attack3"));
    }
    
    IEnumerator AttackCooldown(string stateName)
    {
        isAttacking = true;
        animator.SetBool("isJumping", false); // Force landed state so AnyState->Jump doesn't interrupt
        
        // If in air, FORCE play the animation because transition might not exist
        if (!IsGrounded())
        {
            animator.Play(stateName);
        }
        
        yield return new WaitForSeconds(0.5f); // Duration of attack
        isAttacking = false;
    }

private void PerformAttack()
{
    // Use attackPoint if assigned, otherwise use player position
    Vector3 pos = attackPoint != null ? attackPoint.position : transform.position;
    
    // If no attackPoint, shift the detection forward slightly
    if (attackPoint == null)
    {
        pos += (facingRight ? Vector3.right : Vector3.left) * 0.5f;
    }

    // Detect enemies in range of attack
    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(pos, attackRange, enemyLayers);

    // Damage them
    foreach (Collider2D enemy in hitEnemies)
    {
        EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
        if (enemyHealth != null)
        {
            enemyHealth.TakeDamage(1);
        }
    }
}

// Visualize the attack range in the editor
private void OnDrawGizmosSelected()
{
    Vector3 pos = attackPoint != null ? attackPoint.position : transform.position;
    if (attackPoint == null)
    {
        pos += (facingRight ? Vector3.right : Vector3.left) * 0.5f;
    }
    Gizmos.DrawWireSphere(pos, attackRange);
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

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        Debug.Log("Player Took Damage! Current: " + currentHealth);
        
        if (HealthUI.Instance != null)
        {
             HealthUI.Instance.UpdatePlayerHealth((float)currentHealth / maxHealth);
        }

        if (currentHealth <= 0)
        {
            Debug.Log("PLAYER DIED FOR REAL!");
            Destroy(gameObject);
        }
    }

    }
