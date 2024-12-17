using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Powerup, Ability
{
    [SerializeField] private LayerMask collisionLayers;

    [Header("Hunger")]
    [SerializeField] private float hungerIncrement;

    [Header("Powerup")]
    [SerializeField] private float attackDiameter;
    [SerializeField] private int damage;
    [SerializeField] private float speed;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength;
    [SerializeField] private float stunTime;

    [Header("Components")]
    [SerializeField] private GameObject projectile;

    private bool hit;

    private void Start()
    {
        health = maxHealth;
        player = FindObjectOfType<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();
    }

    public IEnumerator Activate(Vector3 d)
    {
        Hunger.hungerLevel -= hungerIncrement;
        // Creates a projectile.
        GameObject go = Instantiate(projectile);
        GameObject child = go.transform.GetChild(0).gameObject;

        child.transform.localScale = Vector3.one * attackDiameter;
        go.transform.position = player.transform.position;
        TrailRenderer tr = go.GetComponent<TrailRenderer>();

        float rot = -Mathf.Rad2Deg * Mathf.Asin(d.normalized.x);
        if (d.y < 0) rot = 180 - rot;
        go.transform.rotation = Quaternion.Euler(0, 0, rot);

        tr.startWidth = attackDiameter;
        tr.endWidth = 0;
        hit = false;

        HashSet<Collider2D> hitColliders = new HashSet<Collider2D>();
        
        while (!hit)
        {
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(go.transform.position, attackDiameter / 2, collisionLayers);

            foreach (Collider2D coll in hitEnemies)
            {
                // If this colliders has already been hit by the projectile, continue.
                if (hitColliders.Contains(coll)) continue;
                hitColliders.Add(coll);

                if (coll.gameObject.TryGetComponent<Enemy>(out Enemy e))
                {
                    // Make hit enemy take damage.
                    StartCoroutine(e.ApplyKnockback((go.transform.position - e.transform.position).normalized, knockbackStrength, stunTime));
                    e.TakeDamage(damage);
                    continue;
                }

                // If the projectile hits the environment, destroy the projectile.
                hit = true;
            }
            go.transform.position += speed * d * Time.deltaTime;
            yield return null;
        }
        Destroy(child);

        yield return new WaitForSeconds(0.3f);
        Destroy(go);
    }
}
