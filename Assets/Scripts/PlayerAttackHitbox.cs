using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class PlayerAttackHitbox : MonoBehaviour
{
    [Header("Targets")]
    [SerializeField] private LayerMask targetLayers = ~0;

    private readonly HashSet<Collider2D> hitTargets = new HashSet<Collider2D>();
    private readonly HashSet<EnemyHealth> hitEnemyHealthTargets = new HashSet<EnemyHealth>();
    private Collider2D hitboxCollider;
    private bool isWindowActive;
    private int currentDamage = 1;

    private void Awake()
    {
        hitboxCollider = GetComponent<Collider2D>();
        hitboxCollider.isTrigger = true;
        hitboxCollider.enabled = false;
    }

    public void StartAttackWindow(int damage)
    {
        currentDamage = Mathf.Max(0, damage);
        hitTargets.Clear();
        hitEnemyHealthTargets.Clear();
        isWindowActive = true;
        hitboxCollider.enabled = true;
    }

    public void EndAttackWindow()
    {
        isWindowActive = false;
        hitboxCollider.enabled = false;
        hitTargets.Clear();
        hitEnemyHealthTargets.Clear();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isWindowActive)
        {
            return;
        }

        if ((targetLayers.value & (1 << other.gameObject.layer)) == 0)
        {
            return;
        }

        EnemyHealth enemyHealth = other.GetComponentInParent<EnemyHealth>();
        if (enemyHealth != null)
        {
            if (!hitEnemyHealthTargets.Add(enemyHealth))
            {
                return;
            }

            enemyHealth.OnPlayerAttackHit(currentDamage);
            return;
        }

        if (!hitTargets.Add(other))
        {
            return;
        }

        other.SendMessage("OnPlayerAttackHit", currentDamage, SendMessageOptions.DontRequireReceiver);
        Debug.Log($"Player attack hit: {other.name} for {currentDamage}");
    }
}
