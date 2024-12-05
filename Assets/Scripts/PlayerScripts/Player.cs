using LevelGen;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
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

    [Header("Stun")]
    [Tooltip("The amount of time the player will be unable to act after getting hurt. (In seconds)")]
    [SerializeField] private float stunTime;

    [Header("Layer Masks")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Components")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private PlayerMovement tdMovement;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator uiAnimator;
    [SerializeField] private GameObject arrow;

    private Color transparentColor = new Color(1, 1, 1, 0.15f);
    private bool invulnerable;
    [HideInInspector] public int room;
    private RoomGeneratorScript roomGen;

    [HideInInspector] public bool stunned;
    [HideInInspector] public Vector3 knockbackDirection;
    [HideInInspector] public Vector3 originalPosition;

    private void Start()
    {
        health = maxHealth;
        // Makes it so that the player (layer 3) won't collide with the enemy. (layer 7)
        Physics2D.IgnoreLayerCollision(3, 7);
        roomGen = FindObjectOfType<RoomGeneratorScript>();

        Action findRoom = () =>
        {
            Vector2Int tilePos = new Vector2Int(
                Mathf.FloorToInt(transform.position.x),
                Mathf.FloorToInt(transform.position.y));
            int potentialRoom = roomGen.FindRoom(tilePos);
            if (potentialRoom != -1) room = potentialRoom;
        };

        Action findClosestEnemy = () =>
        {
            List<Enemy> enemyList = FindObjectsOfType<Enemy>().ToList();

            float lowestMagnitude = -1;

            arrow.SetActive(enemyList.Count != 0);

            foreach (Enemy e in enemyList)
            {
                RaycastHit2D hit = Physics2D.Linecast(transform.position, e.transform.position, enemyLayer);
                if (lowestMagnitude > hit.distance || lowestMagnitude == -1)
                {
                    lowestMagnitude = hit.distance;
                    Vector3 arrowDirection = (e.transform.position - transform.position).normalized;
                    arrow.transform.position = transform.position + arrowDirection;
                    float rot = -Mathf.Rad2Deg * Mathf.Asin(arrowDirection.x);
                    if (arrowDirection.y < 0) rot = 180 - rot;
                    arrow.transform.rotation = Quaternion.Euler(0, 0, rot);
                }
            }
        };

        StartCoroutine(ExecuteRepeatedly(findRoom, 4));
        StartCoroutine(ExecuteRepeatedly(findClosestEnemy, 4));
    }

    public void TakeDamage(int damage)
    {
        if (invulnerable || tdMovement.isDashing) return;

        health -= damage;
        uiAnimator.SetInteger("playerHP", health);

        if (health <= 0) Respawn();
        else StartCoroutine(InvincibilityTimer());
    }

    private IEnumerator InvincibilityTimer()
    {
        invulnerable = true;
        float elapsedTime = 0;

        while (elapsedTime < invincibilityTime)
        {
            sr.color = elapsedTime % toggleTime > toggleTime / 2 ? transparentColor : Color.white;
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        sr.color = Color.white;
        invulnerable = false;
    }

    public IEnumerator ApplyKnockback(Vector3 direction)
    {
        if (invulnerable) yield break;

        knockbackDirection = direction;
        originalPosition = transform.position;
        stunned = true;
        animator.SetBool("stunned", true);

        yield return new WaitForSeconds(stunTime);
        animator.SetBool("stunned", false);
        stunned = false;
    }

    private void Respawn()
    {
        transform.position = Vector3.zero;
        health = maxHealth;
        uiAnimator.SetInteger("playerHP", health);
    }
}
