using LevelGen;
using System.Collections.Generic;
using System;
using UnityEngine;
using static Default.Default;

public class Scientist : Enemy
{
    [Header("Movement")]
    [SerializeField] private float speed;
    [SerializeField] private float knockbackSpeed;

    [Header("Components")]
    [SerializeField] private AudioClip moveSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] private ParticleSystem deathParticlePrefab;

    [HideInInspector] public event Action coroutineAction;

    private Player player;
    private Coroutine walkRoutine;
    private Pathfinding pathfinding;
    private LevelMap levelMap;
    private Vector2 targetPosition;
    private Vector2 startingPosition;
    private int counter;

    private const float collisionRadius = 0.4f; // The enemy's imaginary radius when pathfinding.

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();

        pathfinding = new Pathfinding();
        levelMap = FindObjectOfType<LevelMap>();
        player = FindObjectOfType<Player>();
        startingPosition = transform.position;
        health = maxHealth;

        coroutineAction += () => { walkRoutine = null; };
    }
    protected override void Movement()
    {
        if (stunned || room != player.room || eaten)
        {
            // If the enemy is stunned, move it in the knockback direction and cancel ongoing attack coroutine.
            if (stunned) rb.MovePosition(Vector2.MoveTowards(transform.position, knockbackPosition, knockbackSpeed));
        }

        float playerDistance = Vector3.Distance(transform.position, player.transform.position);

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
        sr.flipX = targetPosition.x - transform.position.x > 0;
        if (animator != null)
        {
            if (Vector2.Distance(targetPosition, transform.position) > 0.5f) animator.SetBool("running", true);
            else animator.SetBool("running", false);
        }
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
            if (distance > 5) continue;
            // Finds the tile in the room that is the furthest from the player and the closest to the enemy.
            float tileValue = Vector2Int.Distance(tile, playerTile);
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

    protected override void Death()
    {
        gameObject.SetActive(false);
        Instantiate(bloodStain, transform.position, Quaternion.Euler(0, 0, UnityEngine.Random.Range(0, 360)));
        ParticleSystem ps = Instantiate(deathParticlePrefab, transform.position, Quaternion.identity).GetComponent<ParticleSystem>();
        ps.Play();

        //ScoreManager.enemiesKilled++;
        if (boss) ScoreManager.bossEnemiesKilled++;

        if (audioSource != null && deathSound != null)
        {
            ps.GetComponent<AudioSource>().PlayOneShot(deathSound);
        }
    }
}
