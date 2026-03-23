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
    [SerializeField] private float followDistance = 3f;
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
    private float desiredHorizontalVelocity;

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
        {
            desiredHorizontalVelocity = 0f;
            return;
        }

        float directionToPlayer = playerTarget.position.x - transform.position.x;
        float distanceToPlayer = Mathf.Abs(directionToPlayer);

        FacePlayer(directionToPlayer);

        if (isInHitReaction || isAttacking)
        {
            desiredHorizontalVelocity = 0f;
            return;
        }

        if (distanceToPlayer > activationRange)
        {
            desiredHorizontalVelocity = 0f;
            PlayIdle();
            return;
        }

        float stoppingDistance = Mathf.Max(followDistance, attackDistance);
        if (distanceToPlayer <= stoppingDistance + stopTolerance)
        {
            desiredHorizontalVelocity = 0f;

            if (distanceToPlayer <= attackDistance && Time.time >= nextAttackTime)
            {
                StartCoroutine(AttackRoutine());
                return;
            }

            PlayIdle();
            return;
        }

        MoveTowardsPlayer(directionToPlayer);
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        float nextXPosition = Mathf.Clamp(
            rb.position.x + desiredHorizontalVelocity * Time.fixedDeltaTime,
            minXLimit,
            maxXLimit);

        float clampedVelocity = (nextXPosition - rb.position.x) / Time.fixedDeltaTime;
        rb.velocity = new Vector2(clampedVelocity, rb.velocity.y);
    }

    private void FacePlayer(float directionToPlayer)
    {
        if (Mathf.Abs(directionToPlayer) <= stopTolerance)
            return;

        spriteRenderer.flipX = directionToPlayer < 0f;
    }

    private void MoveTowardsPlayer(float directionToPlayer)
    {
        float direction = Mathf.Sign(directionToPlayer);

        if (Mathf.Abs(directionToPlayer) <= stopTolerance)
        {
            desiredHorizontalVelocity = 0f;
            PlayIdle();
            return;
        }

        desiredHorizontalVelocity = direction * moveSpeed;
        PlayIdle();
    }

    private void StopMoving()
    {
        desiredHorizontalVelocity = 0f;

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
