using UnityEngine;
using UnityEngine.AI; // Required for pathfinding

public class EnemyAI : MonoBehaviour
{
    [Header("Enemy Class Setup")]
    public EnemyWeapon weaponType;
    public enum EnemyWeapon { Punch, Knife, Pistol }

    [Header("Targeting & Ranges")]
    public Transform player;
    public float detectionRange = 10f; // How close player needs to be to trigger chase
    private float attackRange; // Changes automatically based on weapon

    [Header("Combat Stats")]
    public float timeBetweenAttacks = 1.5f;
    private float lastAttackTime;

    private NavMeshAgent agent;

    [Header("Animation Setup")]
    public Animator enemyAnim; // Drag your Enemy's Animator here in the Inspector!

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // Auto-find the player if you forget to drag them into the slot
        if (player == null) player = GameObject.FindGameObjectWithTag("Player").transform;

        // INCREASED THESE NUMBERS to account for the Enemy's 3x Scale!
        if (weaponType == EnemyWeapon.Pistol) attackRange = 15f;
        else if (weaponType == EnemyWeapon.Knife) attackRange = 3.5f;
        else attackRange = 2.5f; // Bumped Punch up to 2.5 meters

        // Tell the AI to hit the brakes right before it crashes into the player
        agent.stoppingDistance = attackRange - 0.2f;
    }

    void Update()
    {
        // Don't do anything if the player is missing
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Check if player is inside the detection bubble
        if (distanceToPlayer <= detectionRange)
        {
            // If close enough to attack, stop moving and hit them!
            if (distanceToPlayer <= attackRange)
            {
                agent.isStopped = true;

                // Look directly at the player while attacking
                transform.LookAt(new Vector3(player.position.x, transform.position.y, player.position.z));

                Attack();
            }
            // If too far to attack but inside detection range, chase them!
            else
            {
                agent.isStopped = false;
                agent.SetDestination(player.position);
            }
        }
        else
        {
            // Player ran away, stop chasing
            agent.isStopped = true;
        }

        // --- NEW: RUNNING ANIMATION LOGIC ---
        if (enemyAnim != null)
        {
            // If the agent is NOT told to stop, AND is physically moving, set IsRunning to true!
            bool isMoving = !agent.isStopped && agent.velocity.magnitude > 0.1f;
            enemyAnim.SetBool("IsRunning", isMoving);
        }
    }

    void Attack()
    {
        if (Time.time >= lastAttackTime + timeBetweenAttacks)
        {
            // 1. Tell the Animator to play the correct attack
            if (enemyAnim != null)
            {
                if (weaponType == EnemyWeapon.Knife)
                {
                    enemyAnim.SetTrigger("Knife"); // Plays knife_enemy
                }
                else
                {
                    enemyAnim.SetTrigger("Punch"); // Plays punch_enemy
                }
            }

            // 2. Deal the damage
            CharacterHealth playerHealth = player.GetComponent<CharacterHealth>();
            if (playerHealth != null)
            {
                int damageToDeal = (weaponType == EnemyWeapon.Knife) ? 25 : 10;
                playerHealth.TakeDamage(damageToDeal);
            }

            lastAttackTime = Time.time;
        }
    }
}