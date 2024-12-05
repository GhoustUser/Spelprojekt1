using System;
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

    [Header("Pathfinding")]
    [Tooltip("The amount of rays the enemy will cast to pathfind.\nMore rays means more lag.")]
    [SerializeField] private const int rayCount = 10;

    [Header("LayerMasks")]
    [Tooltip("The layers that will be registered for attack detection.")]
    [SerializeField] private LayerMask playerLayer;
    [Tooltip("The layers the enemy's raycasting will collide with.")]
    [SerializeField] private LayerMask wallLayer;

    [Header("Particle Effects")]
    [SerializeField] private GameObject deathParticlePrefab;
    [SerializeField] private GameObject attackParticlePrefab;

    [Header("Sound Effects")]
    [SerializeField] AudioClip meleeAttack;

    [Header("Components")]
    [SerializeField] private GameObject attackHitbox;

    private const float attackDuration = .2f; // WIP, there currently is no lingering hurtbox for the attack.
    private const float collisionRadius = 0.4f; // The enemy's imaginary radius when pathfinding.

    private Player player;
    private Pathfinding pathfinding;
    private Vector2 targetPosition;
    private Vector2 startingPosition;

    private bool isAttacking;
    private bool canAttack;

    private int counter;
    private int pathfindFrequency; // Maybe change this in the future (band-aid thing)
    private Coroutine attackRoutine;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        pathfinding = new Pathfinding();
        canAttack = true;
        pathfindFrequency = 10;
        health = maxHealth;
        startingPosition = transform.position;
        targetPosition = startingPosition;
    }

    protected override void Movement()
    {
        if (stunned)
        {
            rb.MovePosition(Vector2.MoveTowards(transform.position, originalPosition + knockbackDirection * knockbackStrength, knockbackSpeed));
            if (attackRoutine == null) return;
            
            StopCoroutine(attackRoutine);
            attackRoutine = null;
            isAttacking = false;
            canAttack = true;
            attackHitbox.SetActive(false);
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            return;
        }

        if (isAttacking) return;
        else if (Vector2.Distance(transform.position, player.transform.position) < attackDetectionRange) attackRoutine = StartCoroutine(Attack());
        else
        {
            rb.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, speed));
            animator.SetBool("South", targetPosition.y - transform.position.y <= 0);
        }

        counter++;

        // Using a counter so that the script doesn't get run every frame.
        if (counter % pathfindFrequency != 1) return;
        if (room != player.room)
        {
            targetPosition = startingPosition; 
            return;
        }
        
        Vector2 currentTile = new Vector2(
            (int)Math.Floor(transform.position.x),
            (int)Math.Floor(transform.position.y));

        Vector2 playerTile = new Vector2(
            (int)Math.Floor(player.transform.position.x),
            (int)Math.Floor(player.transform.position.y));

        pathfindFrequency = (int)Mathf.Round(Vector2.Distance(currentTile, playerTile)) + 20;

        // Finds the shortest path
        List<Vector2> path = pathfinding.FindPath(currentTile, playerTile);

        // Checks every tile in the path, starting with the furthest one.
        // If a linecast can be drawn towards a tile without colliding, the player can move in a straight line towards that tile.
        foreach (Vector2 v in path.ToArray()[^Math.Min(rayCount, path.Count)..^0])
        {
            Vector2 v2 = new Vector2(v.x + TILE_SIZE / 2, v.y + TILE_SIZE / 2);
            Vector2 offsetVector = new Vector2(v2.x - transform.position.x, v2.y - transform.position.y);

            if (offsetVector.x >= 0 && offsetVector.y >= 0) offsetVector = new Vector2(-collisionRadius, collisionRadius);
            else if (offsetVector.x <= 0 && offsetVector.y <= 0) offsetVector = new Vector2(-collisionRadius, collisionRadius);
            else offsetVector = new Vector2(collisionRadius, collisionRadius);

            RaycastHit2D hit1 = Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y) + offsetVector, v2, wallLayer);
            RaycastHit2D hit2 = Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y) - offsetVector, v2, wallLayer);

            Debug.DrawLine(new Vector2(transform.position.x, transform.position.y) + offsetVector, v2, Color.red, 0.1f);
            Debug.DrawLine(new Vector2(transform.position.x, transform.position.y) - offsetVector, v2, Color.green, 0.1f);
            if (hit1 || hit2) continue;

            targetPosition = v2;
            break;
        }
    }

    private IEnumerator Attack()
    {
        if (!canAttack) yield break;

        animator.SetBool("isAttacking", true);
        canAttack = false;
        Vector3 attackDirection = (player.transform.position - transform.position).normalized;
        isAttacking = true;
        rb.constraints = RigidbodyConstraints2D.FreezeAll;

        attackHitbox.transform.localScale = Vector3.one * attackRange;
        attackHitbox.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
        attackHitbox.SetActive(true);

        yield return new WaitForSeconds(attackChargeUp);

        ParticleSystem[] ps = Instantiate(attackParticlePrefab, transform.position, Quaternion.identity).GetComponentsInChildren<ParticleSystem>();
        foreach (ParticleSystem p in ps) p.Play();

        attackHitbox.GetComponent<SpriteRenderer>().color = new Color(0.5f, 0.5f, 0.8f, 0.8f);

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, playerLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<Player>(out Player p))
            {
                StartCoroutine(p.ApplyKnockback(new Vector3(p.transform.position.x - transform.position.x, p.transform.position.y - transform.position.y, 0).normalized));
                p.TakeDamage(attackDamage);
            }
        }

        yield return new WaitForSeconds(attackDuration);

        animator.SetBool("isAttacking", false);
        isAttacking = false;
        attackHitbox.SetActive(false);
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSeconds(attackCooldown - attackDuration);

        canAttack = true;
    }

    protected override void Death()
    {
        gameObject.SetActive(false);
        ParticleSystem ps = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        ps.Play();
        EnemyGetCount.enemyCount--; 
    }

    private void OnDrawGizmos()
    {
        if (isAttacking) 
        {
            Gizmos.DrawSphere(transform.position, attackRange);
        }
    }
}
