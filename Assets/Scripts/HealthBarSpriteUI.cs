using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBarSpriteUI : MonoBehaviour
{
    [SerializeField] private Image targetImage;
    [SerializeField] private Sprite[] healthSprites; // de lleno a vacío

    private void Awake()
    {
        if (targetImage == null)
        {
            targetImage = GetComponent<Image>();
        }
    }

    public void SetHealth(int currentHealth, int maxHealth)
    {
        if (targetImage == null)
            return;

        if (healthSprites == null || healthSprites.Length == 0)
            return;

        if (maxHealth <= 0)
            return;

        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);

        float normalized = (float)currentHealth / maxHealth;

        int spriteIndex = Mathf.RoundToInt((1f - normalized) * (healthSprites.Length - 1));
        spriteIndex = Mathf.Clamp(spriteIndex, 0, healthSprites.Length - 1);

        targetImage.sprite = healthSprites[spriteIndex];
    }
}