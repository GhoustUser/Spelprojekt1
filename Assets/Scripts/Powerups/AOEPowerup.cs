using System.Collections;
using UnityEngine;

public class AOEPowerup : Powerup
{
    [SerializeField] private LayerMask enemyLayer;

    [Header("Powerup")]
    [SerializeField] private float maxRange;
    [SerializeField] private float travelSpeed;
    [SerializeField] private float blastRadius;
    [SerializeField] private float blastDuration;
    [SerializeField] private int attackDamage;

    [Header("Colors")]
    [SerializeField] private Color blastColor;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength;
    [SerializeField] private float stunTime;

    [Header("Components")]
    [SerializeField] private GameObject aoeProjectile;
    [SerializeField] private GameObject aoeHitbox;

    private Camera cam;

    private void Start()
    {
        cam = FindAnyObjectByType<Camera>();
        health = maxHealth;
    }

    public override IEnumerator Activate(Vector3 direction)
    {
        // Travel to the mouse, but not past a certain distance (max range)
        // Explodes when finished traveling.

        GameObject projectile = Instantiate(aoeProjectile, player.transform.position, Quaternion.identity);

        Vector3 mousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 targetPosition = new Vector3(mousePosition.x, mousePosition.y, 0);

        if (Vector3.Distance(targetPosition, player.transform.position) > maxRange) targetPosition = player.transform.position + direction * maxRange;
        RaycastHit2D hit = Physics2D.Linecast(projectile.transform.position, targetPosition, wallLayer);
        if (hit) targetPosition = player.transform.position + hit.distance * direction;

        while (projectile.transform.position != targetPosition)
        {
            projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, targetPosition, travelSpeed * Time.deltaTime);
            yield return null;
        }

        GameObject hitbox = Instantiate(aoeHitbox, projectile.transform.position, Quaternion.identity);
        hitbox.transform.localScale = Vector3.one * blastRadius;
        SpriteRenderer hitboxRenderer = hitbox.GetComponent<SpriteRenderer>();
        hitboxRenderer.color = blastColor;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(projectile.transform.position, blastRadius, enemyLayer);

        foreach (Collider2D coll in hitEnemies)
        {
            if (!coll.TryGetComponent<Enemy>(out Enemy e)) break;

            StartCoroutine(e.ApplyKnockback((e.transform.position - projectile.transform.position).normalized, knockbackStrength, stunTime));
            e.TakeDamage(attackDamage);
        }

        Destroy(projectile);

        float attackCounter = blastDuration;
        while (attackCounter > 0)
        {
            hitboxRenderer.color = new Color(hitboxRenderer.color.r, hitboxRenderer.color.g, hitboxRenderer.color.b, attackCounter / blastDuration);
            yield return null;
            attackCounter = Mathf.Max(0, attackCounter - Time.deltaTime);
        }

        Destroy(hitbox);
    }
}
