using System.Collections;
using UnityEngine;

public class PlayerCombat : MonoBehaviour
{
    [System.Serializable]
    public class AttackData
    {
        public string animatorTrigger;
        public int damage = 1;
        public float windUp = 0.08f;
        public float activeTime = 0.12f;
        public float recovery = 0.12f;
        public float cooldown = 0.25f;
    }

    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAttackHitbox[] hitboxes;

    [Header("Attacks")]
    [SerializeField] private AttackData punch1 = new AttackData { animatorTrigger = "Punch1" };
    [SerializeField] private AttackData punch2 = new AttackData { animatorTrigger = "Punch2" };
    [SerializeField] private AttackData kick = new AttackData { animatorTrigger = "Kick", cooldown = 0.35f };
    [SerializeField] private AttackData uppercut = new AttackData { animatorTrigger = "Uppercut", cooldown = 0.45f };

    private bool useFirstPunch = true;
    private bool isAttacking;
    private float nextAttackAllowedTime;
    private Coroutine attackRoutine;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

        if (playerMovement == null)
        {
            playerMovement = GetComponent<PlayerMovement>();
        }

        if (hitboxes == null || hitboxes.Length == 0)
        {
            hitboxes = GetComponentsInChildren<PlayerAttackHitbox>(true);
        }
    }

    private void Update()
    {
        if (!CanReadInput())
        {
            return;
        }

        if (Input.GetMouseButtonDown(0))
        {
            AttackData punch = useFirstPunch ? punch1 : punch2;
            if (TryStartAttack(punch))
            {
                useFirstPunch = !useFirstPunch;
            }
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            TryStartAttack(kick);
            return;
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            TryStartAttack(uppercut);
        }
    }

    private bool CanReadInput()
    {
        if (!enabled || !gameObject.activeInHierarchy)
        {
            return false;
        }

        if (playerMovement != null && !playerMovement.enabled)
        {
            return false;
        }

        return true;
    }

    private bool TryStartAttack(AttackData attack)
    {
        if (attack == null || string.IsNullOrEmpty(attack.animatorTrigger))
        {
            return false;
        }

        if (isAttacking || Time.time < nextAttackAllowedTime)
        {
            return false;
        }

        nextAttackAllowedTime = Time.time + Mathf.Max(0f, attack.cooldown);

        if (attackRoutine != null)
        {
            StopCoroutine(attackRoutine);
        }

        attackRoutine = StartCoroutine(AttackRoutine(attack));
        return true;
    }

    private IEnumerator AttackRoutine(AttackData attack)
    {
        isAttacking = true;

        if (animator != null)
        {
            animator.SetTrigger(attack.animatorTrigger);
        }

        if (attack.windUp > 0f)
        {
            yield return new WaitForSeconds(attack.windUp);
        }

        ToggleHitboxes(true, attack.damage);

        if (attack.activeTime > 0f)
        {
            yield return new WaitForSeconds(attack.activeTime);
        }

        ToggleHitboxes(false, 0);

        if (attack.recovery > 0f)
        {
            yield return new WaitForSeconds(attack.recovery);
        }

        isAttacking = false;
        attackRoutine = null;
    }

    private void ToggleHitboxes(bool active, int damage)
    {
        if (hitboxes == null)
        {
            return;
        }

        for (int i = 0; i < hitboxes.Length; i++)
        {
            if (hitboxes[i] == null)
            {
                continue;
            }

            if (active)
            {
                hitboxes[i].StartAttackWindow(damage);
            }
            else
            {
                hitboxes[i].EndAttackWindow();
            }
        }
    }
}
