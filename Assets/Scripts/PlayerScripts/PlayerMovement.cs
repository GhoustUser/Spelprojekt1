using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

/////////////// INFORMATION ///////////////
// This script automatically adds a Rigidbody2D and a CapsuleCollider2D componentin the inspector.
// The following components are needed: Player Input
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float movementSpeed;
    [SerializeField] private float knockbackSpeed;

    [Header("Dash")]
    [Tooltip("How fast the dash is, affects distance traveled.")]
    [SerializeField] private float dashPower;
    [Tooltip("How long the dash and invulnerability frames are. (In seconds)")] 
    [SerializeField] private float dashDuration;
    [Tooltip("Time before you can dash again. (In seconds)")]
    [SerializeField] private float dashCooldown;

    // [Header("Components")]
    private Rigidbody2D rb;
    private TrailRenderer tr;
    private SpriteRenderer sr;
    private Player player;

    private bool canDash;
    [HideInInspector] public bool isDashing;

    private Vector2 dashDirection;
    private Vector2 moveInput;

    public static bool controlEnabled { get; set; } = true; // You can edit this variable from Unity Events.

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
        sr = GetComponent<SpriteRenderer>();
        player = GetComponent<Player>();

        canDash = true;
    }

    private void FixedUpdate()
    {
        if (player.stunned)
        {
            // Moves the player in the knockback direction while stunned.
            rb.MovePosition(Vector2.MoveTowards(transform.position, player.knockbackPosition, knockbackSpeed));
        }
        else if (controlEnabled)
        {
            // If the player is dashing, dash in the currently facing direction.
            if (isDashing) rb.velocity = dashDirection * movementSpeed * dashPower;
            // Move player according to the current input.
            else rb.velocity = moveInput.normalized * movementSpeed;

            // Flips the image depending on the direction the player is facing.
            if (rb.velocity.x == 0) return;
            sr.flipX = rb.velocity.x < 0;
        }
        else
        {
            // Sets the velocity of player to zero if the player isn't supposed to move.
            rb.velocity = Vector2.zero;
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        // Reads the player's movement input and converts it to a Vector2.
        moveInput = context.ReadValue<Vector2>().normalized;
    }

    public void OnDash(InputAction.CallbackContext context)
    {
        if (!canDash || !controlEnabled) return;

        // Starts Dash coroutine.
        StartCoroutine(Dash());
    }

    private IEnumerator Dash()
    {
        // If the player is standing still, cancel the dash coroutine.
        if (moveInput.magnitude == 0) yield break;

        // Prepares for Dash.
        canDash = false;
        isDashing = true;
        tr.emitting = true;
        dashDirection = moveInput.normalized;

        // Waits for the dash duration.
        yield return new WaitForSeconds(dashDuration);

        // Stops the trail renderer from emitting and disables isDashing, meaning that the player is no longer invulnerable.
        tr.emitting = false;
        isDashing = false;

        // Waits for the dash cooldown.
        yield return new WaitForSeconds(dashCooldown - dashDuration);
        canDash = true;
    }
}