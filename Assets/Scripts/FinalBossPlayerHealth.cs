using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class FinalBossPlayerHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int maxHealth = 15;

    private int currentHealth;
    private bool isDead;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDead => isDead;

    public event Action<int, int> HealthChanged;
    public event Action Died;

    private void Awake()
    {
        currentHealth = Mathf.Max(1, maxHealth);
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;

        int finalDamage = Mathf.Max(0, damage);
        if (finalDamage <= 0) return;

        currentHealth = Mathf.Max(0, currentHealth - finalDamage);
        Debug.Log($"Player took {finalDamage} damage. HP: {currentHealth}/{maxHealth}");

        HealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            isDead = true;
            Died?.Invoke();
        }
    }

    public void RestoreFullHealth()
    {
        isDead = false;
        currentHealth = maxHealth;
        HealthChanged?.Invoke(currentHealth, maxHealth);
    }
}