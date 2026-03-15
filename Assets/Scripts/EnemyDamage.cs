using UnityEngine;

public class EnemyDamage : MonoBehaviour
{
    [Header("Damage")]
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
        {
            return;
        }

        Debug.Log("El enemigo hizo daño");

        LifeManager lifeManager = FindObjectOfType<LifeManager>();
        if (lifeManager != null)
        {
            lifeManager.LoseLife();
        }

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
