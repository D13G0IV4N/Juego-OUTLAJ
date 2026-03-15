using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyDetection : MonoBehaviour
{
    private Animator enemyAnimator;

    private void Start()
    {
        enemyAnimator = GetComponentInParent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jugador detectado");
            enemyAnimator.SetTrigger("Attack");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Jugador salió del rango");
        }
    }
}