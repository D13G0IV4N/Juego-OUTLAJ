using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyDetection : MonoBehaviour
{
    public bool playerInRange = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            Debug.Log("Jugador detectado");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            Debug.Log("Jugador salió del rango");
        }
    }
}
