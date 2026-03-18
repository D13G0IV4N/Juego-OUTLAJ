using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class HealthPickup : MonoBehaviour
{
    [Header("Pickup Settings")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private LifeManager lifeManager;

    private Collider2D pickupCollider;

    private void Awake()
    {
        pickupCollider = GetComponent<Collider2D>();
        pickupCollider.isTrigger = true;

        if (lifeManager == null)
        {
            lifeManager = FindObjectOfType<LifeManager>();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag) || lifeManager == null)
        {
            return;
        }

        bool restoredLife = lifeManager.RestoreLife();
        if (!restoredLife)
        {
            return;
        }

        Destroy(gameObject);
    }
}
