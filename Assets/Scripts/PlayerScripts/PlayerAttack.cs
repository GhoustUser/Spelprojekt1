using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttack : MonoBehaviour
{
    [Header("Attack")]
    [Tooltip("Time before you can attack again. (In seconds)")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 1;
    [Tooltip("The size of the attack hurtbox.")]
    [SerializeField] private float attackRange = 1f;

    [Header("Powerups")]
    [SerializeField] public Powerup[] powerups;

    [Header("Special Attack")]
    [SerializeField] private float spAttackCooldown = 1.0f;

    [Header("LayerMasks")]
    [Tooltip("The layers that will be registered for attack detection.")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Components")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private Camera cam;

    private const float attackDuration = 0.2f; // WIP, there currently is no lingering hurtbox for the attack.

    private bool canAttack;
    private bool canSpAttack;
    private Vector3 atkPoint;


    private void Start()
    {
        canAttack = true;
        canSpAttack = true;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack) return;

        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        canAttack = false;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
        Vector3 attackPoint = attackDirection.normalized * attackRange;
        weapon.SetActive(true);

        float rot = -Mathf.Rad2Deg * Mathf.Asin(attackDirection.normalized.x);
        if (attackDirection.y < 0) rot = 180 - rot;
        weapon.transform.rotation = Quaternion.Euler(0, 0, rot);
        weapon.transform.position += attackPoint * 0.2f;

        atkPoint = transform.position + attackPoint;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(atkPoint, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            if (enemy.TryGetComponent<Enemy>(out Enemy e))
            {
                e.TakeDamage(attackDamage);
                StartCoroutine(e.ApplyKnockback(attackDirection.normalized));
            }
        }

        yield return new WaitForSeconds(attackDuration);

        weapon.transform.localPosition = Vector3.zero;
        weapon.SetActive(false);

        yield return new WaitForSeconds(attackCooldown - attackDuration);

        canAttack = true;
    }

    public void OnSpAttack(InputAction.CallbackContext context)
    {
        if (!canSpAttack) return;

        StartCoroutine(SpAttack());
    }

    private IEnumerator SpAttack()
    {
        canSpAttack = false;
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = new Vector3(mousePos.x - transform.position.x, mousePos.y - transform.position.y, 0).normalized;
        foreach (Powerup p in powerups.Where(p => p != null).ToArray()) p.Activate(attackDirection);

        yield return new WaitForSeconds(spAttackCooldown);

        canSpAttack = true;
    }

    private void OnDrawGizmos()
    {
        if (canAttack) return;
        Gizmos.color = Color.yellow;
        Gizmos.DrawSphere(atkPoint, attackRange);
    }
}
