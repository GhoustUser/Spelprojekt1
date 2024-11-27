using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private Vector2 targetPosition;

    private Player player;
    private Rigidbody2D rb;

    [SerializeField] public float speed = 3;

    private const int rayCount = 10;
    private int counter;

    private const float collisionRadius = 0.4f;

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
            // Finds the shortest path
            List<Vector2> path = pathfinding.FindPath(new Vector2(
                (int)Math.Round(player.transform.position.x),
                (int)Math.Round(player.transform.position.y)));

            // Checks every tile in the path, starting with the furthest one.
            // If a linecast can be drawn towards a tile without colliding, the player can move in a straight line towards that tile.
            foreach (Vector2 v in path.ToArray()[^Math.Min(rayCount, path.Count)..^0])
            {
                Vector2 v2 = new Vector2(v.x + 0.5f, v.y + 0.5f);
                Vector2 offsetVector = new Vector2(v2.x - transform.position.x, v2.y - transform.position.y);

                if (offsetVector.x >= 0 && offsetVector.y >= 0) offsetVector = new Vector2(-collisionRadius, collisionRadius);
                else if (offsetVector.x <= 0 && offsetVector.y <= 0) offsetVector = new Vector2(-collisionRadius, collisionRadius);
                else offsetVector = new Vector2(collisionRadius, collisionRadius);

                LayerMask enemyLayer = LayerMask.GetMask("Walls");

                RaycastHit2D hit1 = Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y) + offsetVector, v2, enemyLayer);
                RaycastHit2D hit2 = Physics2D.Linecast(new Vector2(transform.position.x, transform.position.y) - offsetVector, v2, enemyLayer);
                Debug.DrawLine(new Vector2(transform.position.x, transform.position.y) + offsetVector, v2, Color.red, 0.1f);
                Debug.DrawLine(new Vector2(transform.position.x, transform.position.y) - offsetVector, v2, Color.green, 0.1f);
                if (hit1 || hit2) continue;

                targetPosition = v2;
                break;
            }

            /*if (Vector2.Distance(transform.position, path[path.Count-1]) < 0.05f)
            {
                // Attack here
                targetPosition = player.transform.position;
            }*/
        }
        counter++;

        rb.MovePosition(Vector2.MoveTowards(transform.position, targetPosition, speed));
    }
}
