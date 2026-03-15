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

            int randomAttack = Random.Range(0, 2);

            if (randomAttack == 0)
            {
                enemyAnimator.SetTrigger("Attack");
            }
            else
            {
                enemyAnimator.SetTrigger("Attack2");
            }
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
