using LevelGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Default.Default;

public class Player : MonoBehaviour
{
    [Header("Health")]
    [Tooltip("The player's maximum health")]
    [SerializeField] private int maxHealth = 5;
    [Tooltip("The player's current health")]
    [SerializeField] private int health;

    [Header("Invincibility")]
    [Tooltip("The amount of time the player will be invulnerable after getting hit.")]
    [SerializeField] private float invincibilityTime = 1.5f;
    [Tooltip("The speed at which the player will 'blink' when invulnerable.")]
    [SerializeField] private float toggleTime = 0.15f;

    [Header("Knockback")]
    [Tooltip("The distance the player will be knocked back when hurt.")]
    [SerializeField] private float knockbackStrength;
    [Tooltip("The speed the player will be knocked back at")]
    [SerializeField] public float knockbackSpeed;
    [Tooltip("The amount of time the player will be unable to act after getting hurt. (In seconds)")]
    [SerializeField] private float stunTime;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;

    [Header("Components")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private PlayerMovement tdMovement;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private GameObject arrow;
    [SerializeField] private AudioSource audioSource;


    private Color transparentColor = new Color(1, 1, 1, 0.15f);
    private bool invulnerable;
    private List<Enemy> enemyList;
    private RoomGeneratorScript roomGen;

    [HideInInspector] public int room;
    [HideInInspector] public bool stunned;
    [HideInInspector] public Vector3 knockbackPosition;
    [HideInInspector] public Vector3 originalPosition;

    private void Start()
    {
        enemyList = new List<Enemy>();
        health = maxHealth;
        // Makes it so that the player (layer 3) won't collide with the enemy. (layer 7)
        Physics2D.IgnoreLayerCollision(3, 7);
        roomGen = FindObjectOfType<RoomGeneratorScript>();

        // Action that finds the current room the player is in using an integer index.
        Action findRoom = () =>
        {
            // Finds the tile the player is standing on.
            Vector2Int tilePos = new Vector2Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y));

            // Finds the current room and assigns it to the player's room field.
            int potentialRoom = roomGen.FindRoom(tilePos);
            // The return value -1 means that no room was find with the current tile. If that is the case, the current room index will not change.
            if (potentialRoom != -1) room = potentialRoom;
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
                // Fínds the distance between the player and the enemy using Vector2.Distance.
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
        StartCoroutine(ExecuteRepeatedly(findClosestEnemy, 8));
    }

    public void TakeDamage(int damage)
    {
        // Ignores damage if the player is invulnerable or dashing.
        if (invulnerable || tdMovement.isDashing) return;

        health -= damage;
        if (uiAnimator != null) uiAnimator.SetInteger("playerHP", Mathf.Max(0, health));

        if (health <= 0) Respawn();

        // Makes the player invulnerable for a time after taking damage.
        else StartCoroutine(InvincibilityTimer());
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

    public IEnumerator ApplyKnockback(Vector3 direction)
    {
        // Ignores if the player is invulnerable. 
        if (invulnerable) yield break;

        // Sends a linecast with the length of the knockback strength in the knockback direction.
        originalPosition = transform.position;
        RaycastHit2D hit = Physics2D.Linecast(originalPosition, originalPosition + direction * knockbackStrength, wallLayer);

        // If the linecast hit a wall, reduce the knockback position so that it doesn't go past the wall.
        float kbStrength = hit.distance == 0 ? knockbackStrength : knockbackStrength / hit.distance;
        knockbackPosition = originalPosition + direction * kbStrength;

        // Stuns the player.
        stunned = true;
        animator.SetBool("stunned", true);

        // Waits for the stunTime and sets stunned to false;
        yield return new WaitForSeconds(stunTime);
        animator.SetBool("stunned", false);
        stunned = false;
    }

    private void Respawn()
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
