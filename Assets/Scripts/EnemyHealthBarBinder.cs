using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class EnemyHealthBarBinder : MonoBehaviour
{
    [SerializeField] private EnemyHealth enemyHealth;
    [SerializeField] private HealthBarSpriteUI healthBarUI;

    private void Start()
    {
        if (enemyHealth == null)
        {
            enemyHealth = FindObjectOfType<EnemyHealth>();
        }

        if (healthBarUI == null)
        {
            healthBarUI = GetComponent<HealthBarSpriteUI>();
        }

        if (enemyHealth != null && healthBarUI != null)
        {
            enemyHealth.HealthChanged += OnHealthChanged;
            healthBarUI.SetHealth(enemyHealth.CurrentHealth, enemyHealth.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        if (enemyHealth != null)
        {
            enemyHealth.HealthChanged -= OnHealthChanged;
        }
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthBarUI != null)
        {
            healthBarUI.SetHealth(current, max);
        }
    }
}