using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Default.Default;


public class Boss2 : MonoBehaviour
{
    /* -------- Settings --------*/
    [Header("Settings")] [SerializeField] private float movementSpeed = 3.0f;
    [Header("Settings")] [SerializeField] private float distanceBoost = 0.2f;
    public LayerMask wallLayer;
    private const float collisionRadius = 0.4f; // The enemy's imaginary radius when pathfinding.
    
    /* -------- Object references --------*/
    private Player player;
    private Pathfinding pathfinding;
    private List<Vector2> path;
    private Vector2 targetPosition;
    
    /* -------- Variables --------*/
    private float counter = 0f;
    
    /* -------- Start --------*/
    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        pathfinding = new Pathfinding();
        path = new List<Vector2>();
    }

    // Update is called once per frame
    void Update()
    {
        counter += Time.deltaTime;
        //update once every
        const float updateDelay = 1.0f;
        
        //get positions
        Vector2 position = transform.position;
        Vector2Int positionInt = new Vector2Int(Mathf.FloorToInt(position.x), Mathf.FloorToInt(position.y));
        Vector2 playerPosition = player.transform.position;
        Vector2Int playerPositionInt = new Vector2Int(Mathf.FloorToInt(playerPosition.x), Mathf.FloorToInt(playerPosition.y));
        
        //update path
        if (counter >= updateDelay || Vector2.Distance(position, targetPosition) < 0.2f)
        {
            path = pathfinding.FindPath(positionInt, playerPositionInt);
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

            counter = 0f;
        }

        float distance = Vector2.Distance(position, playerPosition);
        Vector2 direction = (targetPosition - position).normalized;

        position += direction * (Time.deltaTime * (movementSpeed + distance * distanceBoost));
        transform.position = position;
    }
}
