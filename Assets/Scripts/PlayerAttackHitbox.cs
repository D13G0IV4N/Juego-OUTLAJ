using System.Collections.Generic;
using UnityEngine;

public class PlayerAttackHitbox : MonoBehaviour
{
    [SerializeField] private LayerMask targetLayers = ~0;

    private bool attackWindowActive;
    private int currentDamage;
    private readonly HashSet<EnemyHealth> enemiesHitThisWindow = new HashSet<EnemyHealth>();

    public void StartAttackWindow(int damage)
    {
        currentDamage = Mathf.Max(1, damage);
        attackWindowActive = true;
        enemiesHitThisWindow.Clear();
    }

    public void EndAttackWindow()
    {
        attackWindowActive = false;
        currentDamage = 0;
        enemiesHitThisWindow.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryDamage(other);
    }

    private void TryDamage(Collider2D other)
    {
        if (!attackWindowActive || other == null)
            return;

        if (((1 << other.gameObject.layer) & targetLayers.value) == 0)
            return;

        EnemyHealth enemyHealth = other.GetComponent<EnemyHealth>();

        if (enemyHealth == null)
        {
            enemyHealth = other.GetComponentInParent<EnemyHealth>();
        }

        if (enemyHealth == null || enemyHealth.IsDead)
            return;

        if (enemiesHitThisWindow.Contains(enemyHealth))
            return;

        enemiesHitThisWindow.Add(enemyHealth);
        enemyHealth.OnPlayerAttackHit(currentDamage);

        Debug.Log($"Player hit {enemyHealth.name} for {currentDamage} damage.");
    }
}