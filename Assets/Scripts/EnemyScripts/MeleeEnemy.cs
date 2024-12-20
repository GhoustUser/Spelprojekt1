using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Default.Default;

public class MeleeEnemy : Enemy
{
    [SerializeField] private LayerMask playerLayer;

    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float knockbackSpeed;

    [Header("Attack")]
    [Tooltip("Distance the enemy is from the player when it decides to start attacking.")]
    [SerializeField] private float attackDetectionRange;
    [Tooltip("Size of the attack hurtbox.")]
    [SerializeField] private float attackRange;
    [SerializeField] private int attackDamage;
    [Tooltip("Time before the enemy will decide to attack again. (In seconds)")]
    [SerializeField] private float attackCooldown;
    [Tooltip("The amount of time between the attack initiation and the hurtbox spawning.")]
    [SerializeField] private float attackChargeUp;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength;
    [SerializeField] private float stunTime;

    [Header("Colors")]
    [SerializeField] private Color attackAreaColor;
    [SerializeField] private Color hitColor;
    
    [Header("Components")]
    [SerializeField] private GameObject attackHitbox;
    [SerializeField] private GameObject deathParticlePrefab;
    [SerializeField] private GameObject attackParticlePrefab;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip meleeHitSound;
    [SerializeField] private AudioClip playerHitSound;
    [SerializeField] private AudioClip moveSound;
    
    private const float attackDuration = .2f; // WIP, there currently is no lingering hurtbox for the attack.
    private const float collisionRadius = 0.4f; // The enemy's imaginary radius when pathfinding.

    private Pathfinding pathfinding;
    private Player player;
    private Vector2 targetPosition;
    private Vector2 startingPosition;

    private bool isAttacking;
    private bool canAttack;

    private int counter; // Counter that decides when the pathfinding code is going to be executed.
    private Coroutine attackRoutine; // Saves the attack coroutine so that it can be canceled on hit.
    private Coroutine walkRoutine;

    [HideInInspector] public event Action coroutineAction;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();
        player = FindObjectOfType<Player>();

        pathfinding = new Pathfinding();
        canAttack = true;
        
        //health = maxHealth;
        startingPosition = transform.position;
        targetPosition = startingPosition;

        coroutineAction += () => { walkRoutine = null; };

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
   
    protected override void Movement()
    {
        if (stunned || eaten)
        {
            // If the enemy is stunned, move it in the knockback direction and cancel ongoing attack coroutine.
            if (stunned) rb.MovePosition(Vector2.MoveTowards(transform.position, knockbackPosition, knockbackSpeed));
            if (attackRoutine == null) return;
            
            // Resets attack related attributes.
            StopCoroutine(attackRoutine);
            attackRoutine = null;
            isAttacking = false;
            canAttack = true;
            attackHitbox.SetActive(false);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            if (animator != null) animator.SetBool("isAttacking", false);
            return;
        }

        // Ends the movement script if the enemy is attacking.
        if (isAttacking) return;

        // Attacks if in range of the player.
        if (Vector2.Distance(transform.position, player.transform.position) < attackDetectionRange)
        {
            attackRoutine = StartCoroutine(Attack());
            return;
        }

        if (walkRoutine == null && Vector2.Distance(transform.position, targetPosition) > 0.5f)
        {
            audioSource.clip = moveSound;
            float startTime = UnityEngine.Random.Range(0, moveSound.length - 0.5f);
            audioSource.time = startTime;
            audioSource.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
            audioSource.Play();
            walkRoutine = StartCoroutine(StopAfterDuration(audioSource, 0.5f, coroutineAction));
        }

        // Moves the enemy towards the target tile.
        rb.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, speed));

        // Sets the vertical direction the enemy is heading in.
        if (animator != null) animator.SetBool("South", targetPosition.y - transform.position.y <= 0);
        counter++;

        // Using a counter so that the script doesn't get run every frame.
        if (counter % 10 != 1) return;
        if (room != player.room)
        {
            targetPosition = startingPosition; 
            return;
        }
        
        // Finds the tile the enemy and the player are standing on top of.
        Vector2 currentTile = new Vector2(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y));

        Vector2 playerTile = new Vector2(
            Mathf.FloorToInt(player.transform.position.x),
            Mathf.FloorToInt(player.transform.position.y));

        // Finds the shortest path.
        List<Vector2> path = pathfinding.FindPath(currentTile, playerTile);

        // Checks every tile in the path, starting with the furthest one.
        // If a linecast can be drawn towards a tile without colliding, the player can move in a straight line towards that tile.
        foreach (Vector2 v in path.ToArray()[^Mathf.Min(RAY_COUNT, path.Count)..^0])
        {
            Vector2 v2 = new Vector2(v.x + TILE_SIZE / 2, v.y + TILE_SIZE / 2);
            Vector2 offsetVector = new Vector2(v2.x - transform.position.x, v2.y - transform.position.y);

            if (offsetVector.x >= 0 && offsetVector.y >= 0) offsetVector = new Vector2(-collisionRadius, collisionRadius);
            else if (offsetVector.x <= 0 && offsetVector.y <= 0) offsetVector = new Vector2(-collisionRadius, collisionRadius);
            else offsetVector = new Vector2(collisionRadius, collisionRadius);

            RaycastHit2D hit1 = Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y) + offsetVector, v2, wallLayer);
            RaycastHit2D hit2 = Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y) - offsetVector, v2, wallLayer);

            if (hit1 || hit2) continue;

            // Sets the targetPosition to the furthest possible tile.
            targetPosition = v2;
            break;
        }
    }

    private IEnumerator Attack()
    {
        if (!canAttack) yield break;

        // Initializes the attack.
        if (animator != null) animator.SetBool("isAttacking", true);
        canAttack = false;
        isAttacking = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        // Fixes the attackHitbox GameObject.
        attackHitbox.transform.localScale = Vector3.one * attackRange;
        attackHitbox.GetComponent<SpriteRenderer>().color = attackAreaColor;
        attackHitbox.SetActive(true);

        // Waits for charge up time.
        yield return new WaitForSeconds(attackChargeUp);
         
        // Finds the attack particle system and plays it.
        ParticleSystem[] ps = Instantiate(attackParticlePrefab, transform.position, Quaternion.identity).GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in ps) p.Play();

        attackHitbox.GetComponent<SpriteRenderer>().color = hitColor;

        // Finds all overlapping colliders and adds them to an array.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);
        GetComponent<AudioSource>().PlayOneShot(meleeHitSound);

        foreach (Collider2D enemy in hitEnemies)
        {
            // If the found collider belongs to the player, damage the player and apply knockback.
            if (!enemy.TryGetComponent<Player>(out Player p)) continue;

            audioSource.PlayOneShot(playerHitSound);

            StartCoroutine(p.ApplyKnockback(new Vector3(p.transform.position.x - transform.position.x, p.transform.position.y - transform.position.y, 0).normalized, knockbackStrength, stunTime));
            p.TakeDamage(attackDamage);
        }

        // Waits for the attack to finish.
        yield return new WaitForSeconds(attackDuration);
        
        // Stops attacking.
        if (animator != null) animator.SetBool("isAttacking", false);
        isAttacking = false;
        attackHitbox.SetActive(false);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Waits for the attack cooldown.
        yield return new WaitForSeconds(attackCooldown - attackDuration);
        canAttack = true;
    }

    protected override void Death()
    {
        gameObject.SetActive(false);

        // Plays the on death particles.
        ParticleSystem ps = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        ps.Play();

        if (audioSource != null && deathSound != null)
        {
            ps.GetComponent<AudioSource>().PlayOneShot(deathSound);
        }

        // Counts the enemy and adds time to timer.
        EnemyGetCount.enemyCount--;
    }
}
