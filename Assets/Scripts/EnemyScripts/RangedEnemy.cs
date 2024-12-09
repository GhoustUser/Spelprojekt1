using LevelGen;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Default.Default;

public class RangedEnemy : Enemy
{
    [Header("Movement")]
    [SerializeField] private float speed;

    [Header("Attack")]
    [Tooltip("The distance from the player the enemy needs to be in order to attack.")]
    [SerializeField] private float attackRange;
    [Tooltip("The time the enemy needs to wait before attacking again.")]
    [SerializeField] private float attackCooldown;
    [SerializeField] private float attackChargeUp;

    [Header("Components")]
    [SerializeField] private LineRenderer lr;

    private Pathfinding pathfinding;
    private Vector2 targetPosition;
    private Vector2 startingPosition;
    private Coroutine attackRoutine;
    private Player player;
    private LevelMap levelMap;
    private bool isAttacking;
    private bool canAttack;
    private int counter;
    private const float collisionRadius = 0.4f; // The enemy's imaginary radius when pathfinding.

    private void Start()
    {
        pathfinding = new Pathfinding();
        levelMap = FindObjectOfType<LevelMap>();
        player = FindObjectOfType<Player>();
        startingPosition = transform.position;
        canAttack = true;
    }

    protected override void Movement()
    {
        if (isAttacking) return;

        Vector2Int currentTile = new Vector2Int(
            Mathf.FloorToInt(transform.position.x),
            Mathf.FloorToInt(transform.position.y));

        Vector2Int playerTile = new Vector2Int(
            Mathf.FloorToInt(player.transform.position.x),
            Mathf.FloorToInt(player.transform.position.y));

        if (Vector2Int.Distance(currentTile, playerTile) > attackRange)
        {
            attackRoutine = StartCoroutine(Attack());
            return;
        }

        rb.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, speed));
        counter++;

        if (counter % 10 != 1) return;
        if (room != player.room)
        {
            targetPosition = startingPosition;
            return;
        }

        Tuple<float, Vector2Int> highestValue = new Tuple<float, Vector2Int>(-1, Vector2Int.zero);

        foreach (Vector2Int tile in levelMap.rooms[room].Floor)
        {
            // Finds the tile in the room that is the furthest from the player and the closest to the enemy. (Might need tweaking)
            if (Vector2Int.Distance(tile, playerTile) < attackRange) continue;
            float tileValue = Vector2Int.Distance(tile, playerTile) - Vector2Int.Distance(tile, currentTile);
            if (tileValue <= highestValue.Item1 && highestValue.Item1 != -1) continue;

            highestValue = new Tuple<float, Vector2Int>(tileValue, tile);
        }

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

        canAttack = false;
        isAttacking = true;

        Vector3 attackDirection = (player.transform.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, attackDirection, wallLayer);

        lr.startColor = Color.yellow;
        lr.endColor = Color.yellow;

        float attackTimer = 0;
        while (attackTimer < attackChargeUp)
        {
            lr.SetPosition(0, transform.position);
            lr.SetPosition(1, hit.point);
            yield return null;
            attackTimer += Time.deltaTime;
        }

        isAttacking = false;
        canAttack = true;
    }

    protected override void Death()
    {
        throw new System.NotImplementedException();
    }
}
