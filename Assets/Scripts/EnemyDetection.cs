using System.Collections;
using UnityEngine;

public class EnemyDetection : MonoBehaviour
{
    [Header("Attack Timing")]
    [SerializeField] private float attackInterval = 1.1f;
    [SerializeField] private float attackWindUp = 0.2f;
    [SerializeField] private float attackActiveDuration = 0.25f;

    [Header("Damage Sources")]
    [SerializeField] private EnemyDamage[] enemyDamageWindows;

    private Animator enemyAnimator;
    private EnemyHealth enemyHealth;
    private Coroutine attackLoopRoutine;
    private bool playerInRange;

    private void Awake()
    {
        enemyAnimator = GetComponentInParent<Animator>();
        enemyHealth = GetComponentInParent<EnemyHealth>();

        if (enemyDamageWindows == null || enemyDamageWindows.Length == 0)
        {
            enemyDamageWindows = GetComponentsInChildren<EnemyDamage>(true);
            if (enemyDamageWindows.Length == 0)
            {
                enemyDamageWindows = GetComponentsInParent<EnemyDamage>(true);
            }
        }
    }

    private void OnDisable()
    {
        StopAttackLoop();
        SetDamageWindow(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = true;
        if (attackLoopRoutine == null)
        {
            attackLoopRoutine = StartCoroutine(AttackLoop());
        }

        Debug.Log("Jugador detectado");
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
        {
            return;
        }

        playerInRange = false;
        StopAttackLoop();
        SetDamageWindow(false);

        Debug.Log("Jugador salió del rango");
    }

    private IEnumerator AttackLoop()
    {
        while (playerInRange && (enemyHealth == null || !enemyHealth.IsDead))
        {
            TriggerRandomAttackAnimation();

            if (attackWindUp > 0f)
            {
                yield return new WaitForSeconds(attackWindUp);
            }

            SetDamageWindow(true);

            if (attackActiveDuration > 0f)
            {
                yield return new WaitForSeconds(attackActiveDuration);
            }

            SetDamageWindow(false);

            float remainingTime = Mathf.Max(0f, attackInterval - attackWindUp - attackActiveDuration);
            if (remainingTime > 0f)
            {
                yield return new WaitForSeconds(remainingTime);
            }
            else
            {
                yield return null;
            }
        }

        attackLoopRoutine = null;
    }

    private void TriggerRandomAttackAnimation()
    {
        if (enemyAnimator == null)
        {
            return;
        }

        int randomAttack = Random.Range(0, 2);
        if (randomAttack == 0)
        {
            enemyAnimator.SetTrigger("Attack");
        }
        else
        {
            enemyAnimator.SetTrigger("Attack2");
        }
    }

    private void StopAttackLoop()
    {
        if (attackLoopRoutine == null)
        {
            return;
        }

        StopCoroutine(attackLoopRoutine);
        attackLoopRoutine = null;
    }

    private void SetDamageWindow(bool isActive)
    {
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
}
