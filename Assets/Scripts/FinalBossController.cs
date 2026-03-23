using System.Collections;
using UnityEngine;


[RequireComponent(typeof(EnemyHealth))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class FinalBossController : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private Transform playerTarget;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.25f;
    [SerializeField] private float activationRange = 12f;
    [SerializeField] private float followDistance = 5f;
    [SerializeField] private float retreatDistance = 1.75f;
    [SerializeField] private float attackDistance = 3f;
    [SerializeField] private float stopTolerance = 0.1f;
    [SerializeField] private float minXLimit = -100f;
    [SerializeField] private float maxXLimit = 100f;

    [Header("Attack Timing")]
    [SerializeField] private float attackInterval = 1.6f;
    [SerializeField] private float attackWindUp = 0.2f;
    [SerializeField] private float attackActiveDuration = 0.2f;
    [SerializeField] private FinalBossEnemyDamage[] enemyDamageWindows;

    [Header("Animator")]
    [SerializeField] private string idleStateName = "BossXMan_Idle";
    [SerializeField] private string hitStateName = "BossXMan_Hit";
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private float hitRecoverDuration = 0.35f;

    private EnemyHealth enemyHealth;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D rb;

    private bool isDead;
    private bool isAttacking;
    private bool isInHitReaction;
    private float nextAttackTime;

    private void Awake()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Damaged += OnDamaged;
            enemyHealth.Died += OnDied;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Damaged -= OnDamaged;
            enemyHealth.Died -= OnDied;
        }
    }

    private void Update()
    {
        if (isDead || playerTarget == null)
            return;

        FacePlayer();

        if (isInHitReaction || isAttacking)
            return;

        float distanceToPlayer = Mathf.Abs(playerTarget.position.x - transform.position.x);

        if (distanceToPlayer > activationRange)
        {
            StopMoving();
            PlayIdle();
            return;
        }

        if (distanceToPlayer <= attackDistance && Time.time >= nextAttackTime)
        {
            StartCoroutine(AttackRoutine());
            return;
        }

        if (distanceToPlayer > followDistance)
        {
            MoveTowardsPlayer();
        }
        else if (distanceToPlayer < retreatDistance)
        {
            MoveAwayFromPlayer();
        }
        else
        {
            StopMoving();
            PlayIdle();
        }
    }

    private void FacePlayer()
    {
        if (playerTarget.position.x < transform.position.x)
        {
            spriteRenderer.flipX = true;
        }
        else
        {
            spriteRenderer.flipX = false;
        }
    }

    private void MoveTowardsPlayer()
    {
        float direction = Mathf.Sign(playerTarget.position.x - transform.position.x);
        MoveHorizontal(direction);
    }

    private void MoveAwayFromPlayer()
    {
        float direction = -Mathf.Sign(playerTarget.position.x - transform.position.x);
        MoveHorizontal(direction);
    }

    private void MoveHorizontal(float direction)
    {
        float targetX = transform.position.x + direction * moveSpeed * Time.deltaTime;
        targetX = Mathf.Clamp(targetX, minXLimit, maxXLimit);

        float delta = targetX - transform.position.x;

        if (Mathf.Abs(delta) <= stopTolerance)
        {
            StopMoving();
            PlayIdle();
            return;
        }

        rb.velocity = new Vector2(direction * moveSpeed, rb.velocity.y);
        PlayIdle();
    }

    private void StopMoving()
    {
        if (rb != null)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + attackInterval;
        StopMoving();

        if (animator != null && !string.IsNullOrWhiteSpace(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        if (attackWindUp > 0f)
            yield return new WaitForSeconds(attackWindUp);

        SetDamageWindows(true);

        if (attackActiveDuration > 0f)
            yield return new WaitForSeconds(attackActiveDuration);

        SetDamageWindows(false);

        isAttacking = false;
        PlayIdle();
    }

    private void SetDamageWindows(bool active)
    {
        if (enemyDamageWindows == null)
            return;

        for (int i = 0; i < enemyDamageWindows.Length; i++)
        {
            if (enemyDamageWindows[i] == null)
                continue;

            if (active)
                enemyDamageWindows[i].BeginAttackWindow();
            else
                enemyDamageWindows[i].EndAttackWindow();
        }
    }

    private void OnDamaged(EnemyHealth _)
    {
        if (isDead)
            return;

        StartCoroutine(HitReactionRoutine());
    }

    private IEnumerator HitReactionRoutine()
    {
        isInHitReaction = true;
        StopMoving();

        if (animator != null && !string.IsNullOrWhiteSpace(hitStateName))
        {
            animator.Play(hitStateName, 0, 0f);
        }

        yield return new WaitForSeconds(hitRecoverDuration);
        isInHitReaction = false;
        PlayIdle();
    }

    private void OnDied(EnemyHealth _)
    {
        isDead = true;
        isAttacking = false;
        isInHitReaction = false;
        StopMoving();
        SetDamageWindows(false);
    }

    private void PlayIdle()
    {
        if (animator == null || string.IsNullOrWhiteSpace(idleStateName) || isDead)
            return;

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        if (!state.IsName(idleStateName) && !isAttacking && !isInHitReaction)
        {
            animator.Play(idleStateName, 0, 0f);
        }
    }
}