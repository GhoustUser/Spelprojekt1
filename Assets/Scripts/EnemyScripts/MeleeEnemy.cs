using System;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] private Pathfinding pathfinding;
    public Player player;
    public Vector2 targetPosition;
    private const int speed = 3;
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
            List<Vector2> path = pathfinding.FindPath(new Vector2(
                (int)Math.Round(player.transform.position.x),
                (int)Math.Round(player.transform.position.y)));

            foreach (Vector2 v in path)
            {
                RaycastHit2D hit = Physics2D.Linecast(transform.position, v, 6);
                Debug.DrawLine(transform.position, v, Color.red, 0.1f);
                if (hit) continue;

                targetPosition = v;
                break;
            }
        }
        counter++;


        if (Vector2.Distance(transform.position, player.transform.position) < 0.05f)
        {
            // Attack here
            targetPosition = player.transform.position;
        }

        transform.position = Vector2.MoveTowards(transform.position, targetPosition, speed * Time.deltaTime);
    }
}
