using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] private float speed;
    [SerializeField] private int damage;

    [Header("Components")]
    [Tooltip("Child is the visible part of the arrow.")]
    [SerializeField] private GameObject child;

    [HideInInspector] public Vector3 direction;

    private HashSet<Enemy> hitEnemies;
    private bool hit;

    private void Start()
    {
        hitEnemies = new HashSet<Enemy>();
    }

    private void Update()
    {
        if (!hit) transform.position += speed * direction * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If the arrow collides with the player, returns.
        if (collision.gameObject.TryGetComponent<Player>(out Player p)) return;
        if (collision.gameObject.TryGetComponent<Enemy>(out Enemy e))
        {
            // If this enemy has already been hit by the projectile, return.
            if (hitEnemies.Contains(e)) return;

            // Make hit enemy take damage.
            e.TakeDamage(damage);

            // Adds enemy to the set of hit enemies.
            hitEnemies.Add(e);
            return;
        }
        // If the projectile hits the environment, destroy the projectile.
        hit = true;
        Destroy(child);

        // Destroys the projectile with delay, allowing the trail to fade out.
        Invoke(nameof(DestroyThis), 0.3f); // 0.3 seconds is the time it takes for the trail to disappear.
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
