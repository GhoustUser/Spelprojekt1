using System;
using System.Collections.Generic;
using UnityEngine;
using static Default.Default;

public class MeleeEnemy : Enemy
{
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private Vector2 targetPosition;
    [SerializeField] private GameObject attackHitbox;

    private Player player;
    private Rigidbody2D rb;

    [SerializeField] public float speed = 3.0f;
    [SerializeField] public float attackRange = 1.5f;

    private const int rayCount = 10;
    private const float attackDuration = .1f;
    protected const float attackMaxCooldown = 1.0f;
    private const float collisionRadius = 0.4f;

    private int counter;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        rb = GetComponent<Rigidbody2D>();
    }

    protected override void Movement()
    {
        // Using a counter so that the script doesn't get run every frame.
        if (counter % 10 == 1)
        {
            Vector2 playerTile = new Vector2(
                (int)Math.Floor(player.transform.position.x),
                (int)Math.Floor(player.transform.position.y));

            // Finds the shortest path
            List<Vector2> path = pathfinding.FindPath(playerTile);

            // Checks every tile in the path, starting with the furthest one.
            // If a linecast can be drawn towards a tile without colliding, the player can move in a straight line towards that tile.
            foreach (Vector2 v in path.ToArray()[^Math.Min(rayCount, path.Count)..^0])
            {
                Vector2 v2 = new Vector2(v.x + TILE_SIZE / 2, v.y + TILE_SIZE / 2);
                Vector2 offsetVector = new Vector2(v2.x - transform.position.x, v2.y - transform.position.y);

                if (offsetVector.x >= 0 && offsetVector.y >= 0) offsetVector = new Vector2(-collisionRadius, collisionRadius);
                else if (offsetVector.x <= 0 && offsetVector.y <= 0) offsetVector = new Vector2(-collisionRadius, collisionRadius);
                else offsetVector = new Vector2(collisionRadius, collisionRadius);

                LayerMask wallLayer = LayerMask.GetMask("Walls");

                RaycastHit2D hit1 = Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y) + offsetVector, v2, wallLayer);
                RaycastHit2D hit2 = Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y) - offsetVector, v2, wallLayer);

                Debug.DrawLine(new Vector2(transform.position.x, transform.position.y) + offsetVector, v2, Color.red, 0.1f);
                Debug.DrawLine(new Vector2(transform.position.x, transform.position.y) - offsetVector, v2, Color.green, 0.1f);
                if (hit1 || hit2) continue;

                targetPosition = v2;
                break;
            }

            print(Vector2.Distance(transform.position, player.transform.position));
            if (Vector2.Distance(transform.position, player.transform.position) < attackRange) Attack();
            else rb.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, speed));
        }
        counter++;
    }

    private void Attack()
    {
        if (attackCooldown > 0) return;

        Vector3 attackDirection = player.transform.position - transform.position;
        attackHitbox.transform.position += attackDirection.normalized;
        attackHitbox.gameObject.SetActive(true);
        attackCooldown = attackMaxCooldown;

        Invoke(nameof(EndAttack), attackDuration);
    }

    private void EndAttack()
    {
        attackHitbox.transform.localPosition = Vector3.zero;
        attackHitbox.gameObject.SetActive(false);

    }
}
