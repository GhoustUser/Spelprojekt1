using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

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

    [Header("LayerMasks")]
    [SerializeField] private LayerMask enemyLayer;
    
    [Header("Audio Settings")]
    [SerializeField] private AudioClip[] moveSounds;
    [SerializeField] private AudioMixerGroup moveAudioGroup;
    

    // [Header("Components")]
    private Rigidbody2D rb;
    private TrailRenderer tr;
    private SpriteRenderer sr;
    private Player player;
    private AudioSource audioSource;
    private bool canDash;
    [HideInInspector] public bool isDashing;
    [HideInInspector] public bool damageDash;

    private HashSet<Collider2D> colliders;
    private Vector2 dashDirection;
    private Vector2 moveInput;

    public static bool controlEnabled { get; set; } = true; // You can edit this variable from Unity Events.

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        tr = GetComponent<TrailRenderer>();
        sr = GetComponent<SpriteRenderer>();
        player = GetComponent<Player>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        
        if (moveAudioGroup != null)
        {
            audioSource.outputAudioMixerGroup = moveAudioGroup;  
        }
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
            if (isDashing)
            {
                rb.velocity = dashDirection * movementSpeed * dashPower;
                if (damageDash)
                {
                    Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, 0.5f, enemyLayer);

                    foreach (Collider2D hit in hitEnemies)
                    {
                        if (colliders.Contains(hit)) continue;
                        colliders.Add(hit);
                        if (!hit.TryGetComponent<Enemy>(out Enemy e)) continue;
                        StartCoroutine(e.ApplyKnockback((e.transform.position - transform.position).normalized, 1.5f, 0.25f));
                        e.TakeDamage(1);
                    }
                }
            }
            // Move player according to the current input.
            else rb.velocity = moveInput.normalized * movementSpeed;
            PlayMoveSound();
           
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
        if (damageDash) colliders = new HashSet<Collider2D>();
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
    
    private bool isMoving;
    
    private void PlayMoveSound()
    {
        if (rb.velocity.magnitude > 0.1f)
        {
            if (!isMoving && moveSounds.Length > 0)
            {
                isMoving = true;
                AudioClip randomMoveSound = moveSounds[Random.Range(0, moveSounds.Length)];
                audioSource.loop = true;
                audioSource.clip = randomMoveSound;
                audioSource.Play();
            }
        }
        else
        {
            if (isMoving)
            {
                isMoving = false;
                audioSource.Stop();
            }
        }
    }
}
    