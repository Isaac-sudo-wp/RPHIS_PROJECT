using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class CharacterHealth : MonoBehaviour
{
    [Header("Health Stats")]
    public int maxHealth = 100;
    public int currentHealth;

    [Header("UI Setup")]
    public Slider healthBarUI;

    [Header("Animation Setup")]
    public Animator myAnimator;

    [Header("Damage Feedback")]
    [Tooltip("Drag the DamageFeedback script here (or leave empty if not used)")]
    public DamageFeedback damageFeedback;

    private bool isDead = false;

    // Event for when the character dies
    public System.Action OnDeath;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
        isDead = false;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;

        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage! Remaining HP: " + currentHealth);

        UpdateHealthBar();

        if (myAnimator != null)
        {
            myAnimator.SetTrigger("GetHit");
        }

        if (damageFeedback != null)
        {
            damageFeedback.TriggerFlash();
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void UpdateHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.value = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;

        Debug.Log(gameObject.name + " has been defeated!");

        // 🔥 TRIGGER THE DEATH EVENT - EnemyAI will listen to this!
        if (OnDeath != null)
        {
            OnDeath.Invoke();
        }

        // 1. Play the Death Animation
        if (myAnimator != null)
        {
            myAnimator.SetTrigger("Die");
        }

        // 2. Turn off EnemyAI
        MonoBehaviour enemyAIBehaviour = GetComponent("EnemyAI") as MonoBehaviour;
        if (enemyAIBehaviour != null) enemyAIBehaviour.enabled = false;

        // 3. Safely stop NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            agent.enabled = false;
        }

        // 4. Turn off collision
        Collider col = GetComponent<Collider>();
        if (col != null) col.enabled = false;

        // 5. Shut down script
        this.enabled = false;

        // 6. Destroy after 3 seconds
        Destroy(gameObject, 3f);
    }

    public void Heal(int healAmount)
    {
        if (isDead) return;
        
        currentHealth = Mathf.Min(currentHealth + healAmount, maxHealth);
        UpdateHealthBar();
        Debug.Log($"{gameObject.name} healed for {healAmount}! HP: {currentHealth}/{maxHealth}");
    }

    public void ResetHealth()
    {
        currentHealth = maxHealth;
        isDead = false;
        UpdateHealthBar();
    }
}