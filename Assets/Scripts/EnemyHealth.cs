using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 5;

    [Header("Hit Reaction")]
    [SerializeField] private string hitTriggerName = "Hit";
    [SerializeField] private string hitStateName = "";

    [Header("Death Animation")]
    [SerializeField] private string deathTriggerName = "Lose";
    [SerializeField] private string deathStateName = "Enemy_Kwesi_Lose";
    [SerializeField] private float disableCollidersDelay = 0.05f;

    [Header("Disable On Death")]
    [SerializeField] private Collider2D[] collidersToDisable;
    [SerializeField] private Behaviour[] behavioursToDisable;
    [SerializeField] private Rigidbody2D targetRigidbody;

    private Animator enemyAnimator;
    private EnemyDetection[] detectionScripts;
    private EnemyDamage[] damageScripts;
    private Collider2D[] autoColliders;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    private void Awake()
    {
        enemyAnimator = GetComponent<Animator>();

        if (targetRigidbody == null)
        {
            targetRigidbody = GetComponent<Rigidbody2D>();
        }

        detectionScripts = GetComponentsInChildren<EnemyDetection>(true);
        damageScripts = GetComponentsInChildren<EnemyDamage>(true);
        autoColliders = GetComponentsInChildren<Collider2D>(true);

        currentHealth = Mathf.Max(1, maxHealth);
    }

    public void OnPlayerAttackHit(int damage)
    {
        if (isDead)
        {
            return;
        }

        int finalDamage = Mathf.Max(0, damage);
        if (finalDamage <= 0)
        {
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        Debug.Log($"{name} took {finalDamage} damage. HP: {currentHealth}/{maxHealth}");

        if (currentHealth == 0)
        {
            Die();
            return;
        }

        PlayHitReaction();
    }

    private void Die()
    {
        if (isDead)
        {
            return;
        }

        isDead = true;

        DisableOffensiveBehaviour();

        if (targetRigidbody != null)
        {
            targetRigidbody.velocity = Vector2.zero;
            targetRigidbody.angularVelocity = 0f;
            targetRigidbody.bodyType = RigidbodyType2D.Kinematic;
            targetRigidbody.simulated = false;
        }

        PlayDeathAnimation();

        if (disableCollidersDelay <= 0f)
        {
            DisableColliders();
        }
        else
        {
            Invoke(nameof(DisableColliders), disableCollidersDelay);
        }
    }

    private void PlayDeathAnimation()
    {
        if (enemyAnimator == null)
        {
            return;
        }

        if (HasAnimatorTrigger(deathTriggerName))
        {
            enemyAnimator.SetTrigger(deathTriggerName);
        }

        if (!string.IsNullOrWhiteSpace(deathStateName))
        {
            enemyAnimator.Play(deathStateName, 0, 0f);
        }

        Debug.Log($"{name} died.");
    }

    private void PlayHitReaction()
    {
        if (enemyAnimator == null)
        {
            return;
        }

        if (HasAnimatorTrigger(hitTriggerName))
        {
            enemyAnimator.SetTrigger(hitTriggerName);
        }

        if (!string.IsNullOrWhiteSpace(hitStateName))
        {
            enemyAnimator.Play(hitStateName, 0, 0f);
        }
    }

    private bool HasAnimatorTrigger(string triggerName)
    {
        if (enemyAnimator == null || string.IsNullOrWhiteSpace(triggerName))
        {
            return false;
        }

        AnimatorControllerParameter[] parameters = enemyAnimator.parameters;
        for (int i = 0; i < parameters.Length; i++)
        {
            if (parameters[i].type == AnimatorControllerParameterType.Trigger && parameters[i].name == triggerName)
            {
                return true;
            }
        }

        return false;
    }

    private void DisableOffensiveBehaviour()
    {
        for (int i = 0; i < detectionScripts.Length; i++)
        {
            if (detectionScripts[i] != null)
            {
                detectionScripts[i].enabled = false;
            }
        }

        for (int i = 0; i < damageScripts.Length; i++)
        {
            if (damageScripts[i] != null)
            {
                damageScripts[i].enabled = false;
            }
        }

        for (int i = 0; i < behavioursToDisable.Length; i++)
        {
            if (behavioursToDisable[i] != null)
            {
                behavioursToDisable[i].enabled = false;
            }
        }
    }

    private void DisableColliders()
    {
        if (collidersToDisable != null && collidersToDisable.Length > 0)
        {
            for (int i = 0; i < collidersToDisable.Length; i++)
            {
                if (collidersToDisable[i] != null)
                {
                    collidersToDisable[i].enabled = false;
                }
            }

            return;
        }

        for (int i = 0; i < autoColliders.Length; i++)
        {
            if (autoColliders[i] != null)
            {
                autoColliders[i].enabled = false;
            }
        }
    }
}
