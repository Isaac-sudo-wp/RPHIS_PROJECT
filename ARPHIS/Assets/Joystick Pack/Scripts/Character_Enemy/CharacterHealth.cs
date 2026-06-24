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

    [Header("Loot Drops")]
    public GameObject coinPrefab;

    [Header("Animation Setup")]
    public Animator myAnimator;

    [Header("Damage Feedback")]
    [Tooltip("Drag the DamageFeedback script here (or leave empty if not used)")]
    public DamageFeedback damageFeedback;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthBar();
    }

    public void TakeDamage(int damageAmount)
    {
        currentHealth -= damageAmount;
        Debug.Log(gameObject.name + " took " + damageAmount + " damage! Remaining HP: " + currentHealth);

        UpdateHealthBar();

        // GUIDE: Play "Get Hit" animation and trigger Red Flash feedback
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
    } // <--- THIS is the bracket that was missing! It closes the TakeDamage function.

    void UpdateHealthBar()
    {
        if (healthBarUI != null)
        {
            healthBarUI.value = (float)currentHealth / maxHealth;
        }
    }

    void Die()
    {
        Debug.Log(gameObject.name + " has been defeated!");

        // 1. Play the Death Animation
        if (myAnimator != null)
        {
            myAnimator.SetTrigger("Die");
        }

        // 2. Drop the Loot
        if (coinPrefab != null)
        {
            Instantiate(coinPrefab, transform.position + new Vector3(0, 1f, 0), Quaternion.identity);
        }

        // 3. Turn off EnemyAI (Fixes Red NavMesh Errors)
        MonoBehaviour enemyAI = GetComponent("EnemyAI") as MonoBehaviour;
        if (enemyAI != null) enemyAI.enabled = false;

        // 4. Safely stop NavMeshAgent
        UnityEngine.AI.NavMeshAgent agent = GetComponent<UnityEngine.AI.NavMeshAgent>();
        if (agent != null)
        {
            if (agent.isOnNavMesh) agent.isStopped = true;
            agent.enabled = false;
        }

        // 5. Turn off collision
        if (GetComponent<Collider>() != null) GetComponent<Collider>().enabled = false;

        // 6. Shut down script
        this.enabled = false;

        // 7. Destroy after 3 seconds
        Destroy(gameObject, 3f);
    }
}