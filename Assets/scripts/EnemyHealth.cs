using UnityEngine;
using UnityEngine.UI;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 10;
    public string enemyName = "Enemy"; // Name to display
    public bool enableSimpleAI = true; // Toggle for default Dino behavior
    private int currentHealth;
    
    // Attack Cooldown
    private float lastAttackTime = 0;

    void Awake()
    {
        // 1. Purge ANY existing colliders
        foreach (var oldCol in GetComponents<Collider2D>())
        {
            if (oldCol != null) Destroy(oldCol);
        }

        // 2. Set up Rigidbody
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb == null) rb = gameObject.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // 3. Add a fresh, tiny BoxCollider2D (Trigger)
        BoxCollider2D col = gameObject.AddComponent<BoxCollider2D>();
        col.size = new Vector2(0.2f, 0.5f); 
        col.offset = Vector2.zero;
        col.isTrigger = true; 
    }

    void Start()
    {
        currentHealth = maxHealth;
    }
    
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        
        // Show/Update Top-Right Health Bar
        if (HealthUI.Instance != null)
        {
            HealthUI.Instance.UpdateEnemyHealth((float)currentHealth / maxHealth);
            HealthUI.Instance.SetEnemyName(enemyName);
        }

        Debug.Log($"{gameObject.name} current health: {currentHealth}");

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
             // Optional: Trigger specific hit reaction if script exists
             MushroomEnemy me = GetComponent<MushroomEnemy>();
             if (me != null) me.TakeDamageEffect();
        }
    }

    void Die()
    {
        // Hide bar when dead
        if (HealthUI.Instance != null)
        {
            HealthUI.Instance.HideEnemyHealth();
        }
        
        MushroomEnemy me = GetComponent<MushroomEnemy>();
        if (me != null)
        {
            me.DieEffect();
            // Let animation finish before destroy? 
            // The DieEffect disables collisions, so we can destroy after some time.
            Destroy(gameObject, 2f); 
        }
        else
        {
            Debug.Log(gameObject.name + " destroyed!");
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!enableSimpleAI) return; // Skip if external script handles AI
        
        if (collision.CompareTag("Player"))
        {
            float dist = Vector2.Distance(transform.position, collision.transform.position);
            
            // Attack logic: 1.3 distance
            if (dist < 1.3f) 
            {
                // Show enemy health bar when player is close (contact)
                if (HealthUI.Instance != null)
                {
                     HealthUI.Instance.UpdateEnemyHealth((float)currentHealth / maxHealth);
                     HealthUI.Instance.SetEnemyName(enemyName);
                }
                
                // Deal damage every 1 second (cooldown)
                if (Time.time - lastAttackTime > 1.0f)
                {
                    PlayerControls player = collision.GetComponent<PlayerControls>();
                    if (player != null)
                    {
                        player.TakeDamage(1);
                        lastAttackTime = Time.time;
                        Debug.Log("Dino attacked Player!");
                    }
                }
            }
        }
    }
}
// test