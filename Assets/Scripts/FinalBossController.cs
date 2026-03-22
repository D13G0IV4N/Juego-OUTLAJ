using System.Collections;
using UnityEngine;

[RequireComponent(typeof(EnemyHealth))]
public class FinalBossController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private EnemyDamage[] attackDamageWindows;
    [SerializeField] private Transform leftLimit;
    [SerializeField] private Transform rightLimit;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float activationRange = 12f;
    [SerializeField] private float followDistance = 4.5f;
    [SerializeField] private float retreatDistance = 1.75f;
    [SerializeField] private float attackDistance = 2.6f;

    [Header("Attack Timing")]
    [SerializeField] private float attackInterval = 1.8f;
    [SerializeField] private float attackWindUp = 0.25f;
    [SerializeField] private float attackActiveDuration = 0.2f;

    [Header("Animator Parameters")]
    [SerializeField] private string moveBoolName = "IsMoving";
    [SerializeField] private string attackTriggerName = "Attack";

    [Header("Debug")]
    [SerializeField] private bool drawDebugGizmos = true;

    private Coroutine attackRoutine;
    private float baseScaleX;
    private float nextAttackTime;
    private bool isActive;
    private bool isAttacking;
    private bool isDead;
    private float currentMoveDirection;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }

        if (enemyHealth == null)
        {
            enemyHealth = GetComponent<EnemyHealth>();
        }

        if (attackDamageWindows == null || attackDamageWindows.Length == 0)
        {
            attackDamageWindows = GetComponentsInChildren<EnemyDamage>(true);
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        baseScaleX = Mathf.Abs(transform.localScale.x);
        if (baseScaleX <= 0.001f)
        {
            baseScaleX = 1f;
        }

        SetAttackWindow(false);
    }

    private void OnEnable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Died += OnBossDied;
        }
    }

    private void OnDisable()
    {
        if (enemyHealth != null)
        {
            enemyHealth.Died -= OnBossDied;
        }

        StopAllBossActions();
    }

    private void Update()
    {
        if (isDead || enemyHealth == null || enemyHealth.IsDead)
        {
            return;
        }

        if (player == null)
        {
            TryFindPlayer();
            SetMoving(false);
            return;
        }

        FacePlayer();

        float horizontalDistance = Mathf.Abs(player.position.x - transform.position.x);
        isActive = horizontalDistance <= activationRange;

        if (!isActive)
        {
            StopMovement();
            SetMoving(false);
            return;
        }

        if (isAttacking)
        {
            StopMovement();
            SetMoving(false);
            return;
        }

        if (horizontalDistance <= attackDistance && Time.time >= nextAttackTime)
        {
            attackRoutine = StartCoroutine(AttackRoutine());
            return;
        }

        currentMoveDirection = CalculateMoveDirection(horizontalDistance);
        SetMoving(Mathf.Abs(currentMoveDirection) > 0.01f);
    }

    private void FixedUpdate()
    {
        if (isDead || !isActive || isAttacking)
        {
            StopMovement();
            return;
        }

        if (Mathf.Abs(currentMoveDirection) <= 0.01f)
        {
            StopMovement();
            return;
        }

        float nextX = transform.position.x + (currentMoveDirection * moveSpeed * Time.fixedDeltaTime);

        if (leftLimit != null)
        {
            nextX = Mathf.Max(nextX, leftLimit.position.x);
        }

        if (rightLimit != null)
        {
            nextX = Mathf.Min(nextX, rightLimit.position.x);
        }

        Vector2 nextPosition = new Vector2(nextX, rb != null ? rb.position.y : transform.position.y);

        if (rb != null && rb.bodyType != RigidbodyType2D.Static && rb.simulated)
        {
            rb.MovePosition(nextPosition);
        }
        else
        {
            transform.position = new Vector3(nextPosition.x, transform.position.y, transform.position.z);
        }
    }

    private IEnumerator AttackRoutine()
    {
        isAttacking = true;
        nextAttackTime = Time.time + Mathf.Max(0.01f, attackInterval);
        currentMoveDirection = 0f;
        StopMovement();
        SetMoving(false);

        if (animator != null && !string.IsNullOrWhiteSpace(attackTriggerName))
        {
            animator.SetTrigger(attackTriggerName);
        }

        if (attackWindUp > 0f)
        {
            yield return new WaitForSeconds(attackWindUp);
        }

        if (!isDead)
        {
            SetAttackWindow(true);
        }

        if (attackActiveDuration > 0f)
        {
            yield return new WaitForSeconds(attackActiveDuration);
        }

        SetAttackWindow(false);
        isAttacking = false;
        attackRoutine = null;
    }

    private float CalculateMoveDirection(float horizontalDistance)
    {
        if (player == null)
        {
            return 0f;
        }

        if (horizontalDistance > followDistance)
        {
            return Mathf.Sign(player.position.x - transform.position.x);
        }

        if (horizontalDistance < retreatDistance)
        {
            return -Mathf.Sign(player.position.x - transform.position.x);
        }

        return 0f;
    }

    private void FacePlayer()
    {
        if (player == null)
        {
            return;
        }

        float deltaX = player.position.x - transform.position.x;
        if (Mathf.Abs(deltaX) <= 0.01f)
        {
            return;
        }

        float facingSign = Mathf.Sign(deltaX);
        transform.localScale = new Vector3(baseScaleX * facingSign, transform.localScale.y, transform.localScale.z);
    }

    private void StopAllBossActions()
    {
        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
            attackRoutine = null;
        }

        isAttacking = false;
        currentMoveDirection = 0f;
        StopMovement();
        SetMoving(false);
        SetAttackWindow(false);
    }

    private void StopMovement()
    {
        currentMoveDirection = 0f;

        if (rb != null && rb.simulated)
        {
            rb.velocity = new Vector2(0f, rb.velocity.y);
        }
    }

    private void SetAttackWindow(bool enabled)
    {
        if (attackDamageWindows == null)
        {
            return;
        }

        for (int i = 0; i < attackDamageWindows.Length; i++)
        {
            EnemyDamage damageWindow = attackDamageWindows[i];
            if (damageWindow == null)
            {
                continue;
            }

            if (enabled)
            {
                damageWindow.BeginAttackWindow();
            }
            else
            {
                damageWindow.EndAttackWindow();
            }
        }
    }

    private void SetMoving(bool moving)
    {
        if (animator != null && !string.IsNullOrWhiteSpace(moveBoolName))
        {
            animator.SetBool(moveBoolName, moving);
        }
    }

    private void OnBossDied(EnemyHealth defeatedEnemy)
    {
        if (defeatedEnemy != enemyHealth)
        {
            return;
        }

        isDead = true;
        StopAllBossActions();
    }

    private void TryFindPlayer()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (!drawDebugGizmos)
        {
            return;
        }

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, activationRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, followDistance);

        Gizmos.color = new Color(1f, 0.5f, 0f);
        Gizmos.DrawWireSphere(transform.position, retreatDistance);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackDistance);
    }
}
