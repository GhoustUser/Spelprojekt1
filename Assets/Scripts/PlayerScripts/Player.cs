using LevelGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Default.Default;

public class Player : Entity
{
    [SerializeField] private LayerMask enemyLayer;

    [Header("Invincibility")]
    [Tooltip("The amount of time the player will be invulnerable after getting hit.")]
    [SerializeField] private float invincibilityTime = 1.5f;
    [Tooltip("The speed at which the player will 'blink' when invulnerable.")]
    [SerializeField] private float toggleTime = 0.15f;

    [Header("Components")]
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private Animator pulseAnimator;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private GameObject arrow;

    [Header("Is Tutorial")] [SerializeField]
    private bool isTutorial = false;

    private SpriteRenderer sr;
    private PlayerMovement playerMovement;
    private PlayerAttack playerAttack;
    private AudioSource audioSource;

    private Color transparentColor = new Color(1, 1, 1, 0.15f);
    private bool invulnerable;
    private List<Enemy> enemyList;
    private LevelMap levelMap;

    [HideInInspector] public int room;
    [HideInInspector] public bool doubleDamage;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        playerMovement = GetComponent<PlayerMovement>();
        playerAttack = GetComponent<PlayerAttack>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (uiAnimator != null) uiAnimator.SetInteger("playerHP", Mathf.Max(0, health));

        enemyList = new List<Enemy>();
        //health = maxHealth;
        // Makes it so that the player (layer 3) won't collide with the enemy. (layer 7)
        Physics2D.IgnoreLayerCollision(3, 7);
        levelMap = FindObjectOfType<LevelMap>();

        // Action that finds the current room the player is in using an integer index.
        Action findRoom = () =>
        {
            if (!LevelMap.IsLoaded) return;
            // Finds the tile the player is standing on.
            Vector2Int tilePos = new Vector2Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y));

            // Finds the current room and assigns it to the player's room field.
            int potentialRoom = levelMap.FindRoom(tilePos);
            // The return value -1 means that no room was find with the current tile. If that is the case, the current room index will not change.
            if (potentialRoom != -1)
            {
                room = potentialRoom;
            }
        };
        Action findGoal = () =>
        {
            if (!LevelMap.IsLoaded) return;
            arrow.SetActive(true);

            Vector3 targetPosition = Vector3.zero;
            if (Generator.isDestroyed) targetPosition = FindObjectOfType<Elevator2>().transform.position;
            else if (FindObjectOfType<Generator>() != null) targetPosition = FindObjectOfType<Generator>().transform.position;
            
            Vector3 arrowDirection = (targetPosition - transform.position).normalized;
            arrow.transform.position = transform.position + arrowDirection;
            float rot = -Mathf.Rad2Deg * Mathf.Asin(arrowDirection.x);
            if (arrowDirection.y < 0) rot = 180 - rot;
            arrow.transform.rotation = Quaternion.Euler(0, 0, rot);
        };
        
        Action findClosestEnemy = () =>
        {
            // Finds all enemies currently on the scene.
            enemyList = FindObjectsOfType<Enemy>().ToList();

            // Sets the lowest magnitude to a temporary number.
            float lowestDistance = -1;

            // Hides the arrow if there are no enemies.
            arrow.SetActive(enemyList.Count != 0);

            foreach (Enemy e in enemyList)
            {
                // F�nds the distance between the player and the enemy using Vector2.Distance.
                float distance = Vector2.Distance(transform.position, e.transform.position);
                // If the distance is higher than the lowest current distance, keep iterating.
                if (lowestDistance <= distance && lowestDistance != -1) continue;

                // Sets new lowest distance.
                lowestDistance = distance;

                // Makes the arrow point in the direction of the closest enemy.
                Vector3 arrowDirection = (e.transform.position - transform.position).normalized;
                arrow.transform.position = transform.position + arrowDirection;
                float rot = -Mathf.Rad2Deg * Mathf.Asin(arrowDirection.x);
                if (arrowDirection.y < 0) rot = 180 - rot;
                arrow.transform.rotation = Quaternion.Euler(0, 0, rot);
            }
        };

        // Starts coroutines that executes the action x times per second.
        StartCoroutine(ExecuteRepeatedly(findRoom, 8));
        if(isTutorial) StartCoroutine(ExecuteRepeatedly(findClosestEnemy, 8));
        else StartCoroutine(ExecuteRepeatedly(findGoal, 8));
    }

    public override void TakeDamage(int damage)
    {
        // Ignores damage if the player is invulnerable or dashing.
        if (invulnerable || playerMovement.isDashing || playerAttack.isEating) return;

        health -= damage * (doubleDamage ? 2 : 1);
        if (uiAnimator != null) uiAnimator.SetInteger("playerHP", Mathf.Max(0, health));
        if (pulseAnimator != null) pulseAnimator.SetBool("pulse", true);

        if (health <= 0) Death();

        // Makes the player invulnerable for a time after taking damage.
        else StartCoroutine(InvincibilityTimer());
    }

    public override IEnumerator ApplyKnockback(Vector3 direction, float knockbackStrength, float stunTime)
    {
        if (invulnerable || playerAttack.isEating || playerMovement.isDashing) yield break;
        base.ApplyKnockback(direction, knockbackStrength, stunTime);
    }

    public bool GainHealth(int amount)
    {
        if (health == maxHealth) return false;
        health += amount;
        if (uiAnimator != null) uiAnimator.SetInteger("playerHP", Mathf.Max(0, health));
        return true;
    }

    private IEnumerator InvincibilityTimer()
    {
        // Initiates invulnerability.
        invulnerable = true;
        float elapsedTime = 0;

        // Iterates while invulnerable.
        while (elapsedTime < invincibilityTime)
        {
            // Makes the player blink while invulnerable.
            sr.color = elapsedTime % toggleTime > toggleTime / 2 ? transparentColor : Color.white;
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        // Makes the player fully visible and sets invulnerability to false.
        sr.color = Color.white;
        invulnerable = false;
    }

    protected override void Death()
    {
        // Resets the player's attributes.
        health = maxHealth;
        stunned = false;
        if (uiAnimator != null) uiAnimator.SetInteger("playerHP", health);
        if (transitionAnimator != null) transitionAnimator.SetBool("respawn", true);

        // Brings the player back to the first scene after a delay, allowing the transition animation to play.
        Invoke(nameof(ResetGame), 1);
    }

    private void ResetGame()
    {
        SceneManager.LoadScene(0);
    }
}
