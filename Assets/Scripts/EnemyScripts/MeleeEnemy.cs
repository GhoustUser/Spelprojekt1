using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Default.Default;

public class MeleeEnemy : Enemy
{
    [SerializeField] private float speed = 3.0f;
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 1;

    [Header("LayerMasks")]
    [SerializeField] private LayerMask playerLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Components")]
    [SerializeField] private GameObject attackHitbox;

    private Player player;
    private Pathfinding pathfinding;

    private Vector2 targetPosition;

    private const int rayCount = 10;
    private const float attackDuration = .1f;
    private const float attackCooldown = 1.0f;
    private const float collisionRadius = 0.4f;

    private bool isAttacking;
    private bool canAttack;

    private int counter;
    private int pathfindFrequency;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        pathfinding = new Pathfinding();
        canAttack = true;
        pathfindFrequency = 10;
        movementEnabled = true;
    }

    protected override void Movement()
    {
        if (Vector2.Distance(transform.position, player.transform.position) < attackRange) StartCoroutine(Attack());
        if (isAttacking || !movementEnabled) return;
        
        else rb.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, speed));
        counter++;

        // Using a counter so that the script doesn't get run every frame.
        if (counter % pathfindFrequency != 1) return;
        
        Vector2 currentTile = new Vector2(
            (int)Math.Floor(transform.position.x),
            (int)Math.Floor(transform.position.y));

        Vector2 playerTile = new Vector2(
            (int)Math.Floor(player.transform.position.x),
            (int)Math.Floor(player.transform.position.y));

        pathfindFrequency = (int)Mathf.Round(Vector2.Distance(currentTile, playerTile)) + 5;

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

        canAttack = false;
        Vector3 attackDirection = player.transform.position - transform.position;
        attackHitbox.transform.position += attackDirection.normalized;
        attackHitbox.SetActive(true);
        isAttacking = true;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackHitbox.transform.position, attackRange, playerLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<Player>(out Player p)) p.TakeDamage(attackDamage);
        }

        yield return new WaitForSeconds(attackDuration);

        isAttacking = false;
        attackHitbox.transform.localPosition = Vector3.zero;
        attackHitbox.SetActive(false);

        yield return new WaitForSeconds(attackCooldown - attackDuration);

        canAttack = true;
    }

    protected override void Death()
    {
        gameObject.SetActive(false);
    }
}
