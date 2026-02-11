using UnityEngine;
using System.Collections;

public class SlimeEnemy : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 2f;
    public float patrolDistance = 3f;
    private float startX;
    private bool movingRight = true;
    
    [Header("Combat")]
    public float detectionRange = 4f;
    public float attackRange = 1.0f;
    public int damage = 1;
    public float attackCooldown = 1.5f;
    private float lastAttackTime;


    
    private bool isAttacking = false;
    private bool isDead = false;
    
    private Animator animator;
    private Transform player;
    private EnemyHealth enemyHealth;
    
    void Start()
    {
        animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            Debug.Log($"SlimeEnemy: Animator found and enabled. Controller: {animator.runtimeAnimatorController?.name}");
        }
        else
        {
            Debug.LogError("SlimeEnemy: No Animator component found!");
        }

        enemyHealth = GetComponent<EnemyHealth>();
        startX = transform.position.x;
        
        if (enemyHealth != null)
        {
            enemyHealth.enableSimpleAI = false;
            enemyHealth.enemyName = "Slime";
        }

        // --- FINAL VISIBILITY FIX (MATCH PLAYER) ---
        // 1. Find Player Depth
        float targetZ = -5f; // Default fallback
        if (player == null)
        {
             GameObject p = GameObject.FindGameObjectWithTag("Player");
             if (p != null) player = p.transform;
        }
        
        if (player != null)
        {
            targetZ = player.position.z - 1.0f; // 1 unit in front of player
            Debug.Log($"SlimeEnemy: Snapped to Player Depth: {targetZ}");
        }

        // 2. Apply Position
        Vector3 pos = transform.position;
        pos.z = targetZ;
        transform.position = pos;

        // 3. Force Sprite Order to Max to win all sorting battles
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null) sr = gameObject.AddComponent<SpriteRenderer>();
        sr.sortingLayerName = "Default"; 
        sr.sortingOrder = 100; // High enough to beat background (usually -10 to 10)
        sr.enabled = true;
        sr.color = Color.white;
        
        // 4. Fix Collider Size (EnemyHealth creates a tiny one 0.2, we need bigger)
        BoxCollider2D col = GetComponent<BoxCollider2D>();
        if (col != null)
        {
            col.size = new Vector2(1.0f, 1.0f); // Make it hit-able!
            col.offset = new Vector2(0, 0.5f);
        }
        
        Debug.Log($"SlimeEnemy STARTED: Pos={transform.position} | Order={sr.sortingOrder}");
        

    }


    




    [Header("Animation Settings")]
    public bool spriteFacesLeft = true; // Set true if the png draws the character facing left.


        
    private void OnDisable() { Debug.Log($"[LIFE] Slime {gameObject.name} was DISABLED at {transform.position}. Trace: {System.Environment.StackTrace}"); }
    private void OnDestroy() { Debug.Log($"[LIFE] Slime {gameObject.name} was DESTROYED at {transform.position}. Trace: {System.Environment.StackTrace}"); }

    private float lastHeartbeatTime = -99f; 
    void Update()
    {
        // 1. Heartbeat FIRST (Diagnostic only)
        if (Time.time > lastHeartbeatTime + 10f)
        {
            lastHeartbeatTime = Time.time;
            Debug.Log($"[HEARTBEAT] Slime {gameObject.name} (Active={gameObject.activeInHierarchy}, Dead={isDead}, Enabled={this.enabled}) at {transform.position}");
        }

        if (isDead) return;
        
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }
        
        if (player == null) return; 
        
        // --- RESTORED COPYCAT LOGIC (VISIBILITY KEEPER) ---
        // Keeps sorting in sync with Dino/Mushroom but DOES NOT touch Z anymore
        if (Time.frameCount % 20 == 0) 
        {
            EnemyHealth[] allEnemies = FindObjectsOfType<EnemyHealth>();
            foreach (var e in allEnemies)
            {
                if (e.gameObject != this.gameObject && (e.name.ToLower().Contains("dino") || e.name.ToLower().Contains("mushroom")))
                {
                    SpriteRenderer theirSR = e.GetComponent<SpriteRenderer>();
                    SpriteRenderer mySR = GetComponent<SpriteRenderer>();
                    
                    if (theirSR != null && mySR != null)
                    {
                         mySR.sortingLayerID = theirSR.sortingLayerID;
                         mySR.sortingOrder = theirSR.sortingOrder;
                         // CAUTION: Removing Z snapping to prevent depth hiding
                    }
                    break; 
                }
            }
        }
        // --------------------------------------------------
        
        float distToPlayer = Vector2.Distance(transform.position, player.position);
        
        if (distToPlayer <= detectionRange)
        {
            // Player detected
            FacePlayer();
            
            if (distToPlayer <= attackRange)
            {
                if (Time.time >= lastAttackTime + attackCooldown)
                {
                    StartCoroutine(PerformAttack());
                }
                else
                {
                    animator.SetBool("isRunning", false); // Idle waiting for cooldown
                }
            }
            else
            {
                // Chase
                MoveTowardsPlayer();
            }
        }
        else
        {
            // Patrol
            Patrol();
        }
    }
    
    void Patrol()
    {
        if (animator != null)
        {
            animator.SetBool("isRunning", true);
        }
        // Debug.Log($"SlimeEnemy: Patrolling. Position: {transform.position.x}, MovingRight: {movingRight}");
        
        if (movingRight)
        {
            transform.Translate(Vector2.right * speed * Time.deltaTime);
            if (transform.position.x >= startX + patrolDistance)
            {
                movingRight = false;
                Flip();
            }
        }
        else
        {
            transform.Translate(Vector2.left * speed * Time.deltaTime);
            if (transform.position.x <= startX - patrolDistance)
            {
                movingRight = true;
                Flip();
            }
        }
    }
    
    void MoveTowardsPlayer()
    {
        animator.SetBool("isRunning", true);
        float step = speed * Time.deltaTime;
        transform.position = Vector2.MoveTowards(transform.position, new Vector2(player.position.x, transform.position.y), step);
    }

    void FacePlayer()
    {
        if (player.position.x > transform.position.x && !movingRight)
        {
            movingRight = true;
            Flip();
        }
        else if (player.position.x < transform.position.x && movingRight)
        {
            movingRight = false;
            Flip();
        }
    }
    
    void Flip()
    {
        // LOGIC:
        // If Sprite Faces RIGHT (default):
        //   Move Right -> Scale.x = +1
        //   Move Left  -> Scale.x = -1
        // If Sprite Faces LEFT:
        //   Move Right -> Scale.x = -1 (Flip it to look Right)
        //   Move Left  -> Scale.x = +1 (Keep it Left)

        Vector3 scale = transform.localScale;
        
        if (spriteFacesLeft)
        {
             scale.x = movingRight ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
        }
        else
        {
             scale.x = movingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        }
        
        transform.localScale = scale;
    }
    
    IEnumerator PerformAttack()
    {
        isAttacking = true;
        lastAttackTime = Time.time;
        
        if (animator != null)
        {
            animator.SetBool("isRunning", false);
            animator.SetTrigger("Attack");
        }
        Debug.Log("SlimeEnemy: Attacking!");
        
        // Enable Health Bar on Attack
        if (HealthUI.Instance != null && enemyHealth != null)
        {
             HealthUI.Instance.UpdateEnemyHealth((float)enemyHealth.maxHealth / enemyHealth.maxHealth); // Just ensure it shows
             HealthUI.Instance.SetEnemyName(enemyHealth.enemyName);
        }
        
        // Wait for animation impact point (approx 0.5s or adjusting based on sprite)
        yield return new WaitForSeconds(0.4f);
        
        // Check range again
        if (player != null && Vector2.Distance(transform.position, player.position) <= attackRange + 0.5f)
        {
            PlayerControls pc = player.GetComponent<PlayerControls>();
            if (pc != null)
            {
                pc.TakeDamage(damage);
            }
        }
        
        yield return new WaitForSeconds(0.4f); // Finish animation
        isAttacking = false;
    }

    // Called by EnemyHealth via SendMessage or we can modify EnemyHealth to call this
    public void TakeDamageEffect() 
    {
        if (isDead) return;
        animator.SetTrigger("Hit");
         // Show bar
         if (HealthUI.Instance != null && enemyHealth != null)
         {
             HealthUI.Instance.SetEnemyName(enemyHealth.enemyName);
         }
    }
    
    public void DieEffect()
    {
        isDead = true;
        animator.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false; // Disable collision
        this.enabled = false; // specific script
    }

    // Called by EnemySpawner to update patrol center after random spawn
    public void SetPatrolCenter(float x)
    {
        startX = x;
        this.enabled = true;
        gameObject.SetActive(true);
        Debug.Log($"SlimeEnemy {gameObject.name}: Patrol center updated to {x}");
    }

    // Ensures clones are completely reset
    public void ResetEnemy()
    {
        isDead = false;
        isAttacking = false;
        lastAttackTime = 0;
        
        // Force Visibility
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = Color.white;
            sr.sortingOrder = 105; // Slightly higher than default 100
        }

        // Force Animator Reset
        if (animator == null) animator = GetComponent<Animator>();
        if (animator != null)
        {
            animator.enabled = true;
            animator.SetBool("isRunning", false);
            if (animator.runtimeAnimatorController != null)
            {
                animator.Play("Idle", 0, 0);
            }
        }

        if (GetComponent<Collider2D>()) GetComponent<Collider2D>().enabled = true;
        
        // REVIVE!
        EnemyHealth eh = GetComponent<EnemyHealth>();
        if (eh != null) eh.ResetHealth();

        this.enabled = true;
        Debug.Log($"SlimeEnemy {gameObject.name}: Fully RESET for spawning.");
    }
}
