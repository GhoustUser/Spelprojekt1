using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/////////////// INFORMATION ///////////////
// This script automatically adds a Rigidbody2D and a CapsuleCollider2D componentin the inspector.
// The following components are needed: Player Input
public class TopDownMovement : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float maxSpeed = 7;
    [SerializeField] private float dashPower = 3;
    [SerializeField] private const float dashDuration = 0.2f;
    [SerializeField] private int attackDamage = 1;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Components")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Camera cam;

    private const float attackCooldown = 1.0f;
    private const float attackDuration = 0.2f;
    private const float attackRange = 1f;
    private const float dashCooldown = 1.0f;

    private bool canAttack;
    private bool canDash;
    private bool isDashing;

    private Vector2 dashDirection;
    private Vector2 moveInput;

    public bool controlEnabled { get; set; } = true; // You can edit this variable from Unity Events

    private void Start()
    {
        canAttack = true;
        canDash = true;
    }

    private void FixedUpdate()
    {
        // Set velocity based on direction of input and maxSpeed
        if (controlEnabled)
        {
            if (isDashing) rb.velocity = dashDirection * maxSpeed * dashPower;
            else rb.velocity = moveInput.normalized * maxSpeed;

            if (rb.velocity.x == 0) return;
            sr.flipX = rb.velocity.x < 0 ? true : false;
        }
        else
        {
            rb.velocity = Vector2.zero;
        }
        // Write code for walking animation here. (Suggestion: send your current velocity into the Animator for both the x - and y - axis.)
    }

    // Handle Move-input
    // This method can be triggered through the UnityEvent in PlayerInput
    public void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>().normalized;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        // attackGenerosity is a small time window where you can queue an attack despite still being on cooldown.
        if (!canAttack) return;

        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
        Vector3 attackPoint = attackDirection.normalized * attackRange;
        weapon.SetActive(true);

        float rot = -Mathf.Rad2Deg * Mathf.Asin(attackDirection.normalized.x);
        if (attackDirection.y < 0) rot = 180 - rot;
        weapon.transform.rotation = Quaternion.Euler(0, 0, rot);
        weapon.transform.position += attackPoint * 0.2f;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position + attackPoint, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<Enemy>(out Enemy e))
            {
                e.TakeDamage(attackDamage);
                StartCoroutine(e.ApplyKnockback(attackDirection.normalized));
            }
        }

        yield return new WaitForSeconds(attackDuration);

        weapon.transform.localPosition = Vector3.zero;
        weapon.SetActive(false);

        yield return new WaitForSeconds(attackCooldown - attackDuration);

        canAttack = true;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!canDash) return;

        StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        if (moveInput.magnitude == 0) yield break;

        canDash = false;
        isDashing = true;
        tr.emitting = true;
        dashDirection = moveInput.normalized;

        yield return new WaitForSeconds(dashDuration);

        tr.emitting = false;
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown - dashDuration);

        canDash = true;
    }
}