using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class FinalBossEnemyDamage : MonoBehaviour
{
    [Header("Damage")]
    [SerializeField] private int damage = 1;
    [SerializeField] private float damageCooldown = 1f;
    [SerializeField] private bool attackWindowActiveOnStart;

    private bool canDamage = true;
    private bool isAttackWindowActive;

    private void Awake()
    {
        isAttackWindowActive = attackWindowActiveOnStart;
    }

    public void BeginAttackWindow()
    {
        isAttackWindowActive = true;
        canDamage = true;
    }

    public void EndAttackWindow()
    {
        isAttackWindowActive = false;
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(ResetDamage));
        isAttackWindowActive = false;
        canDamage = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDealDamage(other);
    }

    private void TryDealDamage(Collider2D other)
    {
        if (!isAttackWindowActive || !canDamage || !other.CompareTag("Player"))
            return;

        FinalBossPlayerHealth playerHealth = other.GetComponent<FinalBossPlayerHealth>();
        if (playerHealth == null)
        {
            playerHealth = other.GetComponentInParent<FinalBossPlayerHealth>();
        }

        if (playerHealth == null || playerHealth.IsDead)
            return;

        Debug.Log("Boss damaged player");
        playerHealth.TakeDamage(damage);

        canDamage = false;

        if (damageCooldown <= 0f)
        {
            ResetDamage();
        }
        else
        {
            Invoke(nameof(ResetDamage), damageCooldown);
        }
    }

    private void ResetDamage()
    {
        canDamage = true;
    }
}