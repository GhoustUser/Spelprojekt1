using LevelGen;
using System;
using System.Collections;
using UnityEditor.Rendering;
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

    [Header("Knockback")]
    [Tooltip("The distance the player will be knocked back when hurt.")]
    [SerializeField] protected float knockbackStrength;
    [Tooltip("The speed the player will be knocked back at")]
    [SerializeField] protected float knockbackSpeed;
    [Tooltip("The amount of time the player will be unable to act after getting hurt. (In seconds)")]
    [SerializeField] private float stunTime;

    [Header("Components")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private PlayerMovement tdMovement;
    [SerializeField] private Animator animator;

    private Color transparentColor = new Color(1, 1, 1, 0.15f);
    private bool invulnerable;
    public int room;
    private RoomGeneratorScript roomGen;

    public bool stunned;
    public Vector3 knockbackDirection;
    public Vector3 originalPosition;

    private void Start()
    {
        health = maxHealth;
        // Makes it so that the player (layer 3) won't collide with the enemy. (layer 7)
        Physics2D.IgnoreLayerCollision(3, 7);
        roomGen = FindObjectOfType<RoomGeneratorScript>();

        Action findRoom = () =>
        {
            int potentialRoom = roomGen.FindRoom(new Vector2Int(
                (int)Mathf.Floor(transform.position.x),
                (int)Mathf.Floor(transform.position.y)));
            if (potentialRoom != -1) room = potentialRoom;
        };

        StartCoroutine(ExecuteRepeatedly(findRoom, 4));
    }

    public void TakeDamage(int damage)
    {
        if (invulnerable || tdMovement.isDashing) return;

        health -= damage;

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
    }
}
