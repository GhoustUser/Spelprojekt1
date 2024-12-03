using LevelGen;
using System;
using System.Collections;
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

    [Header("Components")]
    [SerializeField] private SpriteRenderer sr;
    [SerializeField] private PlayerMovement tdMovement;

    private Color transparentColor = new Color(1, 1, 1, 0.15f);
    private bool invulnerable;
    [HideInInspector] public int room;
    private RoomGeneratorScript roomGen;
    
    private void Start()
    {
        health = maxHealth;
        // Makes it so that the player (layer 3) won't collide with the enemy. (layer 7)
        Physics2D.IgnoreLayerCollision(3, 7);
        roomGen = FindObjectOfType<RoomGeneratorScript>();

        Action a = () =>
        {
            room = roomGen.FindRoom(new Vector2Int(
                (int)Mathf.Floor(transform.position.x),
                (int)Mathf.Floor(transform.position.y)));
        };

        StartCoroutine(ExecuteRepeatedly(a, 4));
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

    private void Respawn()
    {
        transform.position = Vector3.zero;
        health = maxHealth;
    }
}
