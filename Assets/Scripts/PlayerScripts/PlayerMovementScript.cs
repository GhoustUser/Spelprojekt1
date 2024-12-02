using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

/////////////// INFORMATION ///////////////
// This script automatically adds a Rigidbody2D and a CapsuleCollider2D componentin the inspector.
// The following components are needed: Player Input
public class TopDownMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 7;

    [Header("Powerups")]
    [SerializeField] public Powerup[] powerups;

    [Header("Dash")]
    [Tooltip("How fast the dash is, affects distance traveled.")]
    [SerializeField] private float dashPower = 3.0f;
    [Tooltip("How long the dash and invulnerability frames are. (In seconds)")] 
    [SerializeField] private float dashDuration = 0.2f;
    [Tooltip("Time before you can dash again. (In seconds)")]
    [SerializeField] private float dashCooldown = 1.0f;

    [Header("Attack")]
    [Tooltip("Time before you can attack again. (In seconds)")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 1;
    [Tooltip("The size of the attack hurtbox.")]
    [SerializeField] private float attackRange = 1f;

    [Header("Special Attack")]
    [SerializeField] private float spAttackCooldown = 1.0f;

    [Header("LayerMasks")]
    [Tooltip("The layers that will be registered for attack detection.")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Components")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Camera cam;

    private const float attackDuration = 0.2f; // WIP, there currently is no lingering hurtbox for the attack.

    private bool canAttack;
    private bool canDash;
    private bool canSpAttack;
    [HideInInspector] public bool isDashing;

    private Vector2 dashDirection;
    private Vector2 moveInput;

    private Vector3 atkPoint;

    public bool controlEnabled { get; set; } = true; // You can edit this variable from Unity Events

    private void Start()
    {
        canAttack = true;
        canDash = true;
        canSpAttack = true;
    }

    private void FixedUpdate()
    {
        // Set velocity based on direction of input and maxSpeed
        if (controlEnabled)
        {
            if (isDashing) rb.velocity = dashDirection * movementSpeed * dashPower;
            else rb.velocity = moveInput.normalized * movementSpeed;

            if (rb.velocity.x == 0) return;
            sr.flipX = rb.velocity.x < 0;
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

        atkPoint = transform.position + attackPoint;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(atkPoint, attackRange, enemyLayer);

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

    public void OnSpAttack(InputAction.CallbackContext context)
    {
        if (!canSpAttack) return;
        
        StartCoroutine(SpAttack());
    }

    private IEnumerator SpAttack()
    {
        canSpAttack = false;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = new Vector3(mousePos.x - transform.position.x, mousePos.y - transform.position.y, 0).normalized;
        foreach (Powerup p in powerups.Where(p => p != null).ToArray()) p.Activate(attackDirection);
        
        yield return new WaitForSeconds(spAttackCooldown);

        canSpAttack = true;
    }

    private void OnDrawGizmos()
    {
        if (canAttack) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(atkPoint, attackRange);
    }
}