using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/////////////// INFORMATION ///////////////
// This script automatically adds a Rigidbody2D and a CapsuleCollider2D componentin the inspector.
// The following components are needed: Player Input
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed = 7;

    [Header("Knockback")]
    [Tooltip("The distance the player will be knocked back when hurt.")]
    [SerializeField] protected float knockbackStrength;
    [Tooltip("The speed the player will be knocked back at")]
    [SerializeField] protected float knockbackSpeed;
    [Tooltip("The amount of time the player will be unable to act after getting hurt. (In seconds)")]
    [SerializeField] private float stunTime;

    [Header("Dash")]
    [Tooltip("How fast the dash is, affects distance traveled.")]
    [SerializeField] private float dashPower = 3.0f;
    [Tooltip("How long the dash and invulnerability frames are. (In seconds)")] 
    [SerializeField] private float dashDuration = 0.2f;
    [Tooltip("Time before you can dash again. (In seconds)")]
    [SerializeField] private float dashCooldown = 1.0f;

    [Header("Components")]
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private TrailRenderer tr;
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private Player player;

    private bool canDash;
    [HideInInspector] public bool isDashing;

    private Vector2 dashDirection;
    private Vector2 moveInput;

    public bool controlEnabled { get; set; } = true; // You can edit this variable from Unity Events

    private void Start()
    {
        canDash = true;
    }

    private void FixedUpdate()
    {
        if (player.stunned)
        {
            rb.MovePosition(Vector2.MoveTowards(transform.position, player.originalPosition + player.knockbackDirection * knockbackStrength, knockbackSpeed));
            return;
        }
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