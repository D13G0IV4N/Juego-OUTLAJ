using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float walkSpeed = 3f;
    public float runSpeed = 6f;
    public float jumpForce = 8f;

    public Transform groundCheck;
    public float groundCheckRadius = 0.2f;
    public LayerMask groundLayer;

    private Rigidbody2D rb;
    private Animator animator;
    private bool isGrounded;
    private float moveInput;

    private LifeManager lifeManager;
    private bool canTakeDamage = true;
    public float damageCooldown = 1f;

    void Start()
    {

        
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        lifeManager = FindObjectOfType<LifeManager>();
    }

    void Update()
    {
        // Movimiento solo con WASD
        moveInput = 0f;

        if (Input.GetKey(KeyCode.A))
            moveInput = -1f;
        if (Input.GetKey(KeyCode.D))
            moveInput = 1f;

        bool isRunning = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) && Mathf.Abs(moveInput) > 0.1f;
        float currentSpeed = isRunning ? runSpeed : walkSpeed;

        // Movimiento horizontal
        rb.velocity = new Vector2(moveInput * currentSpeed, rb.velocity.y);

        // Detectar suelo
        isGrounded = Physics2D.OverlapCircle(groundCheck.position, groundCheckRadius, groundLayer);

        // Salto con espacio
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.velocity = new Vector2(rb.velocity.x, jumpForce);
        }

        // Voltear personaje
        if (moveInput > 0)
            transform.localScale = new Vector3(6, 6, 1);
        else if (moveInput < 0)
            transform.localScale = new Vector3(-6, 6, 1);

        // Parámetros del Animator
        animator.SetFloat("Speed", Mathf.Abs(moveInput));
        animator.SetBool("IsRunning", isRunning);
        animator.SetBool("IsGrounded", isGrounded);
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck == null) return;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(groundCheck.position, groundCheckRadius);
    }

    void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag("Obstacle") && canTakeDamage)
    {
        if (lifeManager != null)
        {
            lifeManager.LoseLife();
        }

        StartCoroutine(DamageCooldown());
    }
}

System.Collections.IEnumerator DamageCooldown()
{
    canTakeDamage = false;
    yield return new WaitForSeconds(damageCooldown);
    canTakeDamage = true;
    }
}