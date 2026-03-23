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
    [SerializeField] private EnemyDamage[] enemyDamageWindows;

    [Header("Animator")]
    [SerializeField] private string idleStateName = "BossXMan_Idle";
    [SerializeField] private string hitStateName = "BossXMan_Hit";
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private float hitRecoverDuration = 0.35f;

    private Animator bossAnimator;
    private EnemyHealth enemyHealth;
    private Rigidbody2D bossRigidbody;
    private SpriteRenderer spriteRenderer;
    private Coroutine attackRoutine;
    private bool attackInProgress;
    private bool bossActive;
    private float hitRecoverUntilTime;

    public bool IsBossActive => bossActive;
    public bool IsAttackInProgress => attackInProgress;

    private void Awake()
    {
        bossAnimator = GetComponent<Animator>();
        enemyHealth = GetComponent<EnemyHealth>();
        bossRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        if (playerTarget == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                playerTarget = playerObject.transform;
            }
        }

        if (enemyDamageWindows == null || enemyDamageWindows.Length == 0)
        {
            enemyDamageWindows = GetComponentsInChildren<EnemyDamage>(true);
        }

        ClampCurrentPosition();
        SetDamageWindow(false);
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Damaged += OnBossDamaged;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Damaged -= OnBossDamaged;
        }

        StopAttackRoutine();
        SetDamageWindow(false);

        if (bossRigidbody != null)
        {
            bossRigidbody.velocity = Vector2.zero;
        }
    }

    private void Update()
    {
        if (enemyHealth != null && enemyHealth.IsDead)
        {
            bossActive = false;
            return;
        }

        if (playerTarget == null)
        {
            return;
        }

        Vector2 delta = playerTarget.position - transform.position;
        float distanceToPlayer = Mathf.Abs(delta.x);

        UpdateFacing(delta.x);

        bool shouldBeActive = distanceToPlayer <= activationRange;
        bossActive = shouldBeActive;

        if (!bossActive)
        {
            StopAttackRoutine();
            StopHorizontalMovement();
            PlayIdleState();
            return;
        }

        if (Time.time < hitRecoverUntilTime || IsPlayingState(hitStateName))
        {
            StopAttackRoutine();
            StopHorizontalMovement();
            return;
        }

        if (distanceToPlayer <= attackDistance)
        {
            StopHorizontalMovement();

            if (!attackInProgress)
            {
                attackRoutine = StartCoroutine(AttackRoutine());
            }

            return;
        }

        if (attackInProgress)
        {
            StopHorizontalMovement();
            return;
        }

        float moveDirection = 0f;

        if (distanceToPlayer > followDistance)
        {
            moveDirection = Mathf.Sign(delta.x);
        }
        else if (distanceToPlayer < retreatDistance)
        {
            moveDirection = -Mathf.Sign(delta.x);
        }

        if (Mathf.Abs(moveDirection) <= 0f)
        {
            StopHorizontalMovement();
            PlayIdleState();
            return;
        }

        MoveHorizontally(moveDirection);
    }

    private IEnumerator AttackRoutine()
    {
        attackInProgress = true;
        StopHorizontalMovement();

        if (bossAnimator != null && !string.IsNullOrWhiteSpace(attackTriggerName))
        {
            bossAnimator.SetTrigger(attackTriggerName);
        }

        if (attackWindUp > 0f)
        {
            yield return new WaitForSeconds(attackWindUp);
        }

        if (enemyHealth != null && enemyHealth.IsDead)
        {
            attackInProgress = false;
            attackRoutine = null;
            yield break;
        }

        SetDamageWindow(true);

        if (attackActiveDuration > 0f)
        {
            yield return new WaitForSeconds(attackActiveDuration);
        }

        SetDamageWindow(false);

        float recoveryTime = Mathf.Max(0f, attackInterval - attackWindUp - attackActiveDuration);
        if (recoveryTime > 0f)
        {
            yield return new WaitForSeconds(recoveryTime);
        }

        attackInProgress = false;
        attackRoutine = null;
        PlayIdleState();
    }

    private void MoveHorizontally(float direction)
    {
        if (bossRigidbody == null)
        {
            return;
        }

        float nextX = Mathf.Clamp(transform.position.x + (direction * moveSpeed * Time.deltaTime), minXLimit, maxXLimit);
        float velocityX = (nextX - transform.position.x) / Mathf.Max(Time.deltaTime, 0.0001f);

        bossRigidbody.velocity = new Vector2(velocityX, bossRigidbody.velocity.y);

        if (Mathf.Abs(nextX - transform.position.x) <= stopTolerance)
        {
            bossRigidbody.position = new Vector2(nextX, bossRigidbody.position.y);
            bossRigidbody.velocity = new Vector2(0f, bossRigidbody.velocity.y);
        }
    }

    private void StopHorizontalMovement()
    {
        if (bossRigidbody == null)
        {
            return;
        }

        bossRigidbody.velocity = new Vector2(0f, bossRigidbody.velocity.y);
        ClampCurrentPosition();
    }

    private void ClampCurrentPosition()
    {
        if (bossRigidbody != null)
        {
            Vector2 clampedPosition = bossRigidbody.position;
            clampedPosition.x = Mathf.Clamp(clampedPosition.x, minXLimit, maxXLimit);
            bossRigidbody.position = clampedPosition;
            return;
        }

        Vector3 clampedPosition3D = transform.position;
        clampedPosition3D.x = Mathf.Clamp(clampedPosition3D.x, minXLimit, maxXLimit);
        transform.position = clampedPosition3D;
    }

    private void UpdateFacing(float horizontalDelta)
    {
        if (Mathf.Abs(horizontalDelta) <= stopTolerance)
        {
            return;
        }

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = horizontalDelta < 0f;
            return;
        }

        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * (horizontalDelta < 0f ? -1f : 1f);
        transform.localScale = scale;
    }

    private void PlayIdleState()
    {
        if (bossAnimator == null || string.IsNullOrWhiteSpace(idleStateName))
        {
            return;
        }

        AnimatorStateInfo currentState = bossAnimator.GetCurrentAnimatorStateInfo(0);
        if (!currentState.IsName(idleStateName) && !attackInProgress)
        {
            bossAnimator.Play(idleStateName, 0, 0f);
        }
    }

    private void OnBossDamaged(EnemyHealth damagedEnemy)
    {
        if (damagedEnemy != enemyHealth || (enemyHealth != null && enemyHealth.IsDead))
        {
            return;
        }

        hitRecoverUntilTime = Time.time + Mathf.Max(0f, hitRecoverDuration);
        StopAttackRoutine();
        StopHorizontalMovement();
    }

    private bool IsPlayingState(string stateName)
    {
        if (bossAnimator == null || string.IsNullOrWhiteSpace(stateName))
        {
            return false;
        }

        return bossAnimator.GetCurrentAnimatorStateInfo(0).IsName(stateName);
    }

    private void StopAttackRoutine()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        attackInProgress = false;
    }

    private void SetDamageWindow(bool isActive)
    {
        if (enemyDamageWindows == null)
        {
            return;
        }

        for (int i = 0; i < enemyDamageWindows.Length; i++)
        {
            EnemyDamage damageWindow = enemyDamageWindows[i];
            if (damageWindow == null)
            {
                continue;
            }

            if (isActive)
            {
                damageWindow.BeginAttackWindow();
            }
            else
            {
                damageWindow.EndAttackWindow();
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);

        Gizmos.color = Color.green;
        Gizmos.DrawLine(new Vector3(minXLimit, transform.position.y - 2f, 0f), new Vector3(minXLimit, transform.position.y + 2f, 0f));
        Gizmos.DrawLine(new Vector3(maxXLimit, transform.position.y - 2f, 0f), new Vector3(maxXLimit, transform.position.y + 2f, 0f));
    }
}
