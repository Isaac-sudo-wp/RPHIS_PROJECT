using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Class Setup")]
    public EnemyWeapon weaponType;
    public enum EnemyWeapon { Punch, Knife, Pistol }

    [Header("Targeting & Ranges")]
    public Transform player;
    public float detectionRange = 10f;
    private float attackRange;

    [Header("Combat Stats")]
    public float timeBetweenAttacks = 1.5f;
    private float lastAttackTime;

    [Header("Coin Reward")]
    public int coinReward = 5; // 🔥 ONLY PLACE FOR COIN REWARD

    private NavMeshAgent agent;
    private CharacterHealth health; // Reference to the enemy's health

    [Header("Animation Setup")]
    public Animator enemyAnim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        health = GetComponent<CharacterHealth>(); // Get health component

        // 🔥 SUBSCRIBE TO DEATH EVENT
        if (health != null)
        {
            health.OnDeath += OnEnemyDeath;
        }

        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        // Set attack range based on weapon
        if (weaponType == EnemyWeapon.Pistol) attackRange = 15f;
        else if (weaponType == EnemyWeapon.Knife) attackRange = 3.5f;
        else attackRange = 2.5f;

        agent.stoppingDistance = attackRange - 0.2f;
    }

    // 🔥 THIS IS CALLED WHEN THE ENEMY DIES
    private void OnEnemyDeath()
    {
        // Award coins when enemy dies
        AwardCoins();
        
        // Unsubscribe to prevent memory leaks
        if (health != null)
        {
            health.OnDeath -= OnEnemyDeath;
        }
    }

    void Update()
    {
        // Check if enemy is dead
        if (health != null && health.currentHealth <= 0)
        {
            agent.isStopped = true;
            return;
        }

        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            if (distanceToPlayer <= attackRange)
            {
                agent.isStopped = true;
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));
                Attack();
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
        else
        {
            agent.isStopped = true;
        }

        // Running animation logic
        if (enemyAnim != null)
        {
            bool isMoving = !agent.isStopped && agent.velocity.magnitude > 0.1f;
            enemyAnim.SetBool("IsRunning", isMoving);
        }
    }

    void Attack()
    {
        if (Time.time >= lastAttackTime + timeBetweenAttacks)
        {
            if (enemyAnim != null)
            {
                if (weaponType == EnemyWeapon.Knife)
                {
                    enemyAnim.SetTrigger("Knife");
                }
                else
                {
                    enemyAnim.SetTrigger("Punch");
                }
            }

            CharacterHealth playerHealth = player.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                int damageToDeal = (weaponType == EnemyWeapon.Knife) ? 25 : 10;
                playerHealth.TakeDamage(damageToDeal);
            }

            lastAttackTime = Time.time;
        }
    }

    // ==========================================
    // 🪙 COIN REWARD SYSTEM (ONLY HERE)
    // ==========================================
    public void AwardCoins()
    {
        Debug.Log($"🪙 AwardCoins() called on {gameObject.name}");
        
        CoinManager coinManager = FindObjectOfType<CoinManager>();
        
        if (coinManager != null)
        {
            coinManager.AddCoins(coinReward);
            Debug.Log($"🪙 +{coinReward} coins awarded! Total now: {coinManager.GetTotalCoins()}");
        }
        else
        {
            Debug.LogWarning("⚠️ CoinManager not found in scene!");
        }
    }

    void OnDestroy()
    {
        // Clean up event subscription
        if (health != null)
        {
            health.OnDeath -= OnEnemyDeath;
        }
    }
}