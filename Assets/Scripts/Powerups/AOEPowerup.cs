using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AOEPowerup : Powerup
{
    [SerializeField] private float maxRange;
    [SerializeField] private float travelSpeed;
    [SerializeField] private float blastRadius;
    [SerializeField] private float blastDuration;
    [SerializeField] private int attackDamage;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private GameObject aoeProjectile;
    [SerializeField] private GameObject aoeHitbox;
    [SerializeField] private Color blastColor;

    private Camera cam;

    private void Start()
    {
        cam = FindAnyObjectByType<Camera>();
    }

    public override IEnumerator Activate(Vector3 direction)
    {
        // Travel to the mouse, but not past a certain distance (max range)
        // Explodes when finished traveling.

        GameObject projectile = Instantiate(aoeProjectile, player.transform.position, Quaternion.identity);

        Vector3 targetPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        if (Vector3.Distance(targetPosition, player.transform.position) > maxRange) targetPosition = direction * maxRange;
        RaycastHit2D hit = Physics2D.Linecast(projectile.transform.position, targetPosition, wallLayer);
        if (hit) targetPosition = hit.distance * direction;

        while (transform.position != targetPosition)
        {
            projectile.transform.position = Vector3.MoveTowards(projectile.transform.position, targetPosition, travelSpeed * Time.deltaTime);
        }

        GameObject hitbox = Instantiate(aoeHitbox, projectile.transform.position, Quaternion.identity);
        hitbox.transform.localScale = Vector3.one * blastRadius;
        SpriteRenderer hitboxRenderer = hitbox.GetComponent<SpriteRenderer>();
        hitboxRenderer.color = blastColor;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(projectile.transform.position, blastRadius, enemyLayer);

        foreach (Collider2D coll in hitEnemies)
        {
            if (!coll.TryGetComponent<Enemy>(out Enemy e)) break;

            StartCoroutine(e.ApplyKnockback((transform.position - e.transform.position).normalized));
            e.TakeDamage(attackDamage);
        }

        float attackCounter = blastDuration;
        while (attackCounter > 0)
        {
            hitboxRenderer.color = new Color(hitboxRenderer.color.r, hitboxRenderer.color.g, hitboxRenderer.color.b, hitboxRenderer.color.a - hitboxRenderer.color.a * attackCounter / blastDuration);
            yield return null;
            attackCounter = Mathf.Max(0, attackCounter - Time.deltaTime);
        }

        yield return new WaitForSeconds(blastDuration);
        Destroy(projectile);
    }
}
