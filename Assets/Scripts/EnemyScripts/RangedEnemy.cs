using LevelGen;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Default.Default;

public class RangedEnemy : Enemy
{
    [SerializeField] private LayerMask playerLayer;

    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float knockbackSpeed;

    [Header("Attack")]
    [Tooltip("The distance from the player the enemy needs to be in order to attack.")]
    [SerializeField] private float attackRange;
    [Tooltip("The maximum range of the weapon.")]
    [SerializeField] private float maxAttackRange;
    [Tooltip("The time the enemy needs to wait before attacking again.")]
    [SerializeField] private float attackCooldown;
    [Tooltip("The amount of time the enemy will spend aiming at the player.")]
    [SerializeField] private float attackChargeUp;
    [Tooltip("The amount of time the laser will be displayed on screen after shooting.")]
    [SerializeField] private float attackDuration;
    [Tooltip("The speed at which the enemy's laser will track the player.")]
    [SerializeField] private float aimSpeed;
    [Tooltip("The width of the laser while the enemy is aiming.")]
    [SerializeField] private float attackAimWidth;
    [Tooltip("The width of the laser when the enemy is shooting.")]
    [SerializeField] private float attackShootWidth;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength;
    [SerializeField] private float stunTime;

    [Header("Colors")]
    [SerializeField] private Color aimingColor;
    [SerializeField] private Color shootColor;

    [Header("Components")]
    [SerializeField] private ParticleSystem deathParticlePrefab;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private AudioClip playerHitSound;
    [SerializeField] private AudioClip moveSound;

    [HideInInspector] public event Action coroutineAction;

    private LineRenderer lr;
    private Pathfinding pathfinding;
    private Vector2 targetPosition;
    private Vector2 startingPosition;
    private Coroutine attackRoutine;
    private Coroutine walkRoutine;
    private Player player;
    private LevelMap levelMap;
    private bool isAttacking;
    private bool canAttack;
    private int counter;
    private const float collisionRadius = 0.4f; // The enemy's imaginary radius when pathfinding.

    private void Start()
    {
        lr = GetComponent<LineRenderer>();
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();

        pathfinding = new Pathfinding();
        levelMap = FindObjectOfType<LevelMap>();
        player = FindObjectOfType<Player>();
        startingPosition = transform.position;
        canAttack = true;
        health = maxHealth;

        coroutineAction += () => { walkRoutine = null; };

        switch (health)
        {
            case 2:
                healthState = HealthState.Healthy;
                break;
            case 1:
                healthState = HealthState.HeavilyInjured;
                sr.color = new Color(1, .25f, .25f, 1);
                break;
        }
    }

    protected override void Movement()
    {
        if (stunned || room != player.room || eaten)
        {
            // If the enemy is stunned, move it in the knockback direction and cancel ongoing attack coroutine.
            if (stunned) rb.MovePosition(Vector2.MoveTowards(transform.position, knockbackPosition, knockbackSpeed));
            if (attackRoutine == null) return;

            // Resets attack related attributes.
            StopCoroutine(attackRoutine);
            lr.positionCount = 0;
            attackRoutine = null;
            isAttacking = false;
            canAttack = true;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            animator.SetBool("isAttacking", false);
            animator.SetBool("isAiming", false);
            return;
        }

        if (isAttacking) return;

        float playerDistance = Vector3.Distance(transform.position, player.transform.position);
        if (playerDistance >= attackRange && playerDistance <= maxAttackRange)
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

        rb.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, speed));

        // Sets the vertical direction the enemy is heading in.
        animator.SetBool("south", targetPosition.y - transform.position.y <= 0);
        counter++;

        if (counter % 10 != 1) return;
        if (room != player.room)
        {
            targetPosition = startingPosition;
            return;
        }

        Tuple<float, Vector2Int> highestValue = new Tuple<float, Vector2Int>(-1, Vector2Int.zero);

        Vector2Int currentTile = new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y));

        Vector2Int playerTile = new Vector2Int(
            Mathf.FloorToInt(player.transform.position.x),
            Mathf.FloorToInt(player.transform.position.y));

        foreach (Vector2Int tile in levelMap.rooms[room].Floor)
        {
            // Checks if the tile is within shooting range.
            float distance = Vector2Int.Distance(tile, playerTile);
            if (distance < attackRange || distance > maxAttackRange) continue;

            // Finds the tile in the room that is the furthest from the player and the closest to the enemy.
            float tileValue = Vector2Int.Distance(tile, playerTile) - Vector2Int.Distance(tile, currentTile);
            if (tileValue <= highestValue.Item1 && highestValue.Item1 != -1) continue;

            // Checks if the enemy can shoot the player from the tile.
            RaycastHit2D hit = Physics2D.Linecast(tile, player.transform.position, wallLayer);
            if (hit) continue;

            highestValue = new Tuple<float, Vector2Int>(tileValue, tile);
        }

        // If no tile was found.
        if (highestValue.Item1 == -1) return;

        List<Vector2> path = pathfinding.FindPath(currentTile, highestValue.Item2);

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

        RaycastHit2D checkPlayer = Physics2D.Linecast(transform.position, player.transform.position, playerLayer);

        rb.constraints = RigidbodyConstraints2D.FreezeAll;
        canAttack = false;
        isAttacking = true;
        lr.positionCount = 2;

        lr.startColor = aimingColor;
        lr.endColor = aimingColor;
        lr.startWidth = attackAimWidth;
        lr.endWidth = attackAimWidth;

        animator.SetBool("isAiming", true);
        RaycastHit2D hit;
        Vector3 playerTracking = player.transform.position;
        Vector3 attackDirection = Vector3.zero;
        Vector3 endPoint = Vector3.zero;
        float attackTimer = 0;
       
        while (attackTimer < attackChargeUp)
        {
            playerTracking = Vector3.MoveTowards(playerTracking, player.transform.position, aimSpeed * Time.deltaTime);
            attackDirection = (playerTracking - transform.position).normalized;
            hit = Physics2D.Raycast(transform.position, attackDirection, maxAttackRange, wallLayer);
            animator.SetBool("south", attackDirection.y < 0);

            endPoint = hit.point == Vector2.zero ? transform.position + attackDirection * maxAttackRange : hit.point;

            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, endPoint);

            yield return null;
            attackTimer += Time.deltaTime;
        }

        animator.SetBool("isAiming", false);
        lr.startColor = shootColor;
        lr.endColor = shootColor;
        lr.startWidth = attackShootWidth;
        lr.endWidth = attackShootWidth;
        animator.SetBool("isAttacking", true);
        audioSource.PlayOneShot(shootSound);

        RaycastHit2D collisionHit = Physics2D.Linecast(transform.position, endPoint, playerLayer);
        if (collisionHit.collider != null)
        {
            if (collisionHit.collider.TryGetComponent<Player>(out Player p))
            {
                audioSource.PlayOneShot(playerHitSound);

                StartCoroutine(p.ApplyKnockback(attackDirection, knockbackStrength, stunTime));
                p.TakeDamage(1);
            }
        }

        attackTimer = attackDuration;
        while (attackTimer > 0)
        {
            lr.startWidth = attackShootWidth * attackTimer / attackDuration;
            lr.endWidth = lr.startWidth;

            yield return null;
            attackTimer = Mathf.Max(0, attackTimer - Time.deltaTime);
        }

        lr.positionCount = 0;
        animator.SetBool("isAttacking", false);
        isAttacking = false;
        rb.constraints = RigidbodyConstraints2D.FreezeRotation;

        yield return new WaitForSeconds(attackCooldown);
        canAttack = true;
    }

    protected override void Death()
    {
        gameObject.SetActive(false);
        ParticleSystem ps = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        ps.Play();

        if (audioSource != null && deathSound != null)
        {
            ps.GetComponent<AudioSource>().PlayOneShot(deathSound);
        }

        // Counts the enemy and adds time to timer.
        EnemyGetCount.enemyCount--;
        lr.positionCount = 0;
    }
}
