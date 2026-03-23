using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class PlayerHealthBarBinder : MonoBehaviour
{
    [SerializeField] private FinalBossPlayerHealth playerHealth;
    [SerializeField] private HealthBarSpriteUI healthBarUI;

    private void Start()
    {
        if (playerHealth == null)
            playerHealth = FindObjectOfType<FinalBossPlayerHealth>();

        if (healthBarUI == null)
            healthBarUI = GetComponent<HealthBarSpriteUI>();

        if (playerHealth != null && healthBarUI != null)
        {
            playerHealth.HealthChanged += OnHealthChanged;
            healthBarUI.SetHealth(playerHealth.CurrentHealth, playerHealth.MaxHealth);
        }
    }

    private void OnDestroy()
    {
        if (playerHealth != null)
            playerHealth.HealthChanged -= OnHealthChanged;
    }

    private void OnHealthChanged(int current, int max)
    {
        if (healthBarUI != null)
            healthBarUI.SetHealth(current, max);
    }
}