using System;
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

    [Header("Components")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TrailRenderer tr;
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
        attackHitbox.transform.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);

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
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
        attackHitbox.transform.position += attackDirection.normalized * attackRange;
        attackHitbox.gameObject.SetActive(true);
        canAttack = false;

        yield return new WaitForSeconds(attackDuration);

        attackHitbox.transform.localPosition = Vector3.zero;
        attackHitbox.gameObject.SetActive(false);

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