using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] private Pathfinding pathfinding;
    [SerializeField] private Vector2 targetPosition;

    public Player player;

    private const int speed = 3;
    private const int enemyLayer = 6;
    private int counter;

    private void Start()
    {
        player = FindObjectOfType<Player>();
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
            foreach (Vector2 v in path)
            {
                RaycastHit2D hit = Physics2D.Linecast(transform.position, v);
                Debug.DrawLine(transform.position, v, Color.red, 0.1f);
                print(Vector2.Distance(transform.position, hit.point));
                if (hit && Vector2.Distance(transform.position, hit.point) >= 0.51f) continue;

                targetPosition = v;
                break;
            }

            /*if (Vector2.Distance(transform.position, path[path.Count-1]) < 0.05f)
            {
                // Attack here
                targetPosition = player.transform.position;
            }*/
        }
        counter++;

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
}
