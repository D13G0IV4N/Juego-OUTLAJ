using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyDamage : MonoBehaviour
{
    private bool canDamage = true;

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && canDamage)
        {
            Debug.Log("El enemigo hizo daño");

            LifeManager lifeManager = FindObjectOfType<LifeManager>();

            if (lifeManager != null)
            {
                lifeManager.LoseLife();
            }

            canDamage = false;
            Invoke(nameof(ResetDamage), 1f);
        }
    }

    void ResetDamage()
    {
        canDamage = true;
    }
}
