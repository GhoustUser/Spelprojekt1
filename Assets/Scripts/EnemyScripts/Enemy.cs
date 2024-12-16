using System.Collections;
using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private GameObject bloodStain;
    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected Animator animator;
    protected AudioSource audioSource;

    [Header("Health")]
    [SerializeField] protected int maxHealth;
    [Tooltip("The enemy's current health.")]
    [SerializeField] protected int health;
    [SerializeField] private bool canBleed;

    [Header("Knockback")]
    [Tooltip("The distance the enemy will be knocked back when hurt.")]
    [SerializeField] protected float knockbackStrength;
    [Tooltip("The speed the enemy will be knocked back at")]
    [SerializeField] protected float knockbackSpeed;
    [Tooltip("The amount of time the enemy will be unable to act after getting hurt. (In seconds)")]
    [SerializeField] private float stunTime;

    [Header("LayerMasks")]
    [Tooltip("The layers that will be registered for attack detection.")]
    [SerializeField] protected LayerMask playerLayer;
    [Tooltip("The layers the enemy's raycasting will collide with.")]
    [SerializeField] protected LayerMask wallLayer;

    protected bool stunned;
    protected Vector3 knockbackPosition;
    protected Vector3 originalPosition;
    [HideInInspector] public HealthState healthState;
    [HideInInspector] public int room;
    [HideInInspector] public bool eaten;

    private float bleedTimer;

    // Enemy specific Movement function.
    protected abstract void Movement();
    // Enemy specific Death function.
    protected abstract void Death();

    private void Start()
    {
        healthState = HealthState.Healthy;
    }

    private void Update()
    {
        if (!eaten) Movement();

        if (!canBleed) return;

        bleedTimer = Mathf.Max(bleedTimer - Time.deltaTime, 0);

        if (bleedTimer > 0 || healthState == HealthState.Healthy) return;

        switch (healthState)
        {
            // Spawns blood at different rates depending on the enemy's health state.
            case HealthState.HeavilyInjured:
                bleedTimer = 0.5f;
                Instantiate(bloodStain, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                break;
            case HealthState.Injured:
                bleedTimer = 1f;
                Instantiate(bloodStain, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                break;
        }
    }

    public virtual void TakeDamage(int amount)
    {
        health -= amount;
        bleedTimer = 1f;

        if (health <= 0) Death();

        if (!canBleed) return;

        // Temporary fix for healthstates.
        switch (health)
        {
            case 3:
                healthState = HealthState.Healthy;
                break;
            case 2: 
                healthState = HealthState.Injured;
                break;
            case 1:
                healthState = HealthState.HeavilyInjured;
                sr.color = new Color(1, .25f, .25f, 1);
                break;
        }
    }

    public virtual IEnumerator ApplyKnockback(Vector3 direction)
    {
        // Sends a linecast with the length of the knockback strength in the knockback direction.
        originalPosition = transform.position;
        RaycastHit2D hit = Physics2D.Linecast(originalPosition, originalPosition + direction * knockbackStrength, wallLayer);

        // If the linecast hit a wall, reduce the knockback position so that it doesn't go past the wall.
        float kbStrength = hit.distance == 0 ? knockbackStrength : knockbackStrength / hit.distance;
        knockbackPosition = originalPosition + direction * kbStrength;

        // Stuns the enemy.
        stunned = true;
        if (animator != null) animator.SetBool("stunned", true);

        // Waits for the stunTime and sets stunned to false;
        yield return new WaitForSeconds(stunTime);
        if (animator != null) animator.SetBool("stunned", false);
        stunned = false;
    }
}

public enum HealthState
{
    Healthy, Injured, HeavilyInjured
}
