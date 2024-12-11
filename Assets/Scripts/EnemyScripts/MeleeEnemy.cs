using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Default.Default;

public class MeleeEnemy : Enemy
{
    [Header("Movement")]
    [SerializeField] private float speed = 3.0f;

    [Header("Attack")]
    [Tooltip("Distance the enemy is from the player when it decides to start attacking.")]
    [SerializeField] private float attackDetectionRange = 1.5f;
    [Tooltip("Size of the attack hurtbox.")]
    [SerializeField] private float attackRange = 1f;
    [SerializeField] private int attackDamage = 1;
    [Tooltip("Time before the enemy will decide to attack again. (In seconds)")]
    [SerializeField] private float attackCooldown = 1.0f;
    [Tooltip("The amount of time between the attack initiation and the hurtbox spawning.")]
    [SerializeField] private float attackChargeUp = 0.2f;

    [Header("Particle Effects")]
    [SerializeField] private GameObject deathParticlePrefab;
    [SerializeField] private GameObject attackParticlePrefab;

    [Header("Colors")]
    [SerializeField] private Color attackAreaColor;
    [SerializeField] private Color hitColor;

    [Header("Sound Effects")]
    [SerializeField] AudioClip meleeAttack;

    [Header("Components")]
    [SerializeField] private GameObject attackHitbox;

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

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        player = FindObjectOfType<Player>();
        pathfinding = new Pathfinding();
        canAttack = true;
        health = maxHealth;
        startingPosition = transform.position;
        targetPosition = startingPosition;
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
            animator.SetBool("isAttacking", false);
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

        // Moves the enemy towards the target tile.
        rb.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, speed));

        // Sets the vertical direction the enemy is heading in.
        animator.SetBool("South", targetPosition.y - transform.position.y <= 0);
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
        animator.SetBool("isAttacking", true);
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

        foreach (Collider2D enemy in hitEnemies)
        {
            // If the found collider belongs to the player, damage the player and apply knockback.
            if (!enemy.TryGetComponent<Player>(out Player p)) continue;

            StartCoroutine(p.ApplyKnockback(new Vector3(p.transform.position.x - transform.position.x, p.transform.position.y - transform.position.y, 0).normalized));
            p.TakeDamage(attackDamage);
        }

        // Waits for the attack to finish.
        yield return new WaitForSeconds(attackDuration);

        // Stops attacking.
        animator.SetBool("isAttacking", false);
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

        // Counts the enemy and adds time to timer.
        EnemyGetCount.enemyCount--;
    }
}
