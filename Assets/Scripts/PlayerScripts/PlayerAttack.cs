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

    [Header("Eat")]
    [SerializeField] private float eatCooldown;
    [SerializeField] private float eatTime;
    [SerializeField] private float eatRange;
    [SerializeField] private float hungerIncrement;

    [Header("LayerMasks")]
    [Tooltip("The layers that will be registered for attack detection.")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Components")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private Camera cam;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator clawAnimator;
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private SpriteRenderer sr;

    private const float attackDuration = 0.2f; // WIP, there currently is no lingering hurtbox for the attack.

    private bool canAttack;
    private bool canSpAttack;
    private bool canEat;
    [HideInInspector] public bool isEating;
    private Vector3 atkPoint; // The center point of the attack hitbox.

    public static bool controlEnabled { get; set; } = true; // You can edit this variable from Unity Events


    private void Start()
    {
        canAttack = true;
        canSpAttack = true;
        canEat = true;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack || !controlEnabled) return;

        // Starts attack coroutine.
        StartCoroutine(Attack());
    }

    private IEnumerator Attack()
    {
        // Initializes the attack.
        clawAnimator.SetBool("isAttacking", true);
        canAttack = false;

        // Sets the attack direction to the direction the mouse is pointing in.
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = new Vector3(mousePos.x, mousePos.y, 0) - transform.position;
        Vector3 attackPoint = attackDirection.normalized * attackRange;

        // Rotates the weapon according to the direction of the attack.
        float rot = -Mathf.Rad2Deg * Mathf.Asin(attackDirection.normalized.x);
        if (attackDirection.y < 0) rot = 180 - rot;
        weapon.transform.rotation = Quaternion.Euler(0, 0, rot);
        weapon.transform.position += attackPoint * 0.2f;

        // Sets the attack point relative to the player's position.
        atkPoint = transform.position + attackPoint;

        // Finds all overlapping colliders and adds them to an array.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(atkPoint, attackRange, enemyLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // If the found collider belongs to an enemy, damage the enemy and apply knockback.
            if (!enemy.TryGetComponent<Enemy>(out Enemy e)) continue;
            
            e.TakeDamage(attackDamage);
            StartCoroutine(e.ApplyKnockback(attackDirection.normalized));
        }

        // Waits for the attack to finish.
        yield return new WaitForSeconds(attackDuration);

        // Stops attacking.
        clawAnimator.SetBool("isAttacking", false);
        weapon.transform.localPosition = Vector3.zero;

        // Waits for the attack cooldown.
        yield return new WaitForSeconds(attackCooldown - attackDuration);
        canAttack = true;
    }

    public void OnSpAttack(InputAction.CallbackContext context)
    {
        if (!canSpAttack || !controlEnabled) return;

        // Starts special attack coroutine.
        StartCoroutine(SpAttack());
    }

    private IEnumerator SpAttack()
    {
        // Initiates special attack.
        canSpAttack = false;

        // Sets the attack direction to the direction of the mouse.
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = new Vector3(mousePos.x - transform.position.x, mousePos.y - transform.position.y, 0).normalized;

        // Finds the non-empty spaces in the powerups array and activates the effects of the found powerups.
        foreach (Powerup p in powerups.Where(p => p != null).ToArray()) p.Activate(attackDirection);

        // Waits for the special attack cooldown.
        yield return new WaitForSeconds(spAttackCooldown);
        canSpAttack = true;
    }

    public void OnEat(InputAction.CallbackContext context)
    {
        if (!canEat || !controlEnabled) return;

        StartCoroutine(Eat());
    }

    private IEnumerator Eat()
    {
        canEat = false;
        isEating = true;

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, eatRange, enemyLayer);

        Enemy savedEnemy = null;
        foreach (Collider2D coll in hitEnemies)
        {
            if (!coll.TryGetComponent<Enemy>(out Enemy e)) continue;
            if (e.healthState != HealthState.HeavilyInjured) continue;
            savedEnemy = e;
            savedEnemy.eaten = true;
            break;
        }

        if (savedEnemy == null)
        {
            canEat = true;
            yield break;
        }

        float eatDistance = .5f;
        float distance = Vector3.Distance(savedEnemy.transform.position, transform.position);
        Vector3 direction = (savedEnemy.transform.position - transform.position).normalized;
        if (distance > eatDistance) transform.position = transform.position + (distance - eatDistance) * direction;

        sr.flipX = direction.x < 0;
        animator.SetBool("isBiting", true);
        PlayerMovement.controlEnabled = false;
        controlEnabled = false;

        yield return new WaitForSeconds(eatTime);
        animator.SetBool("isBiting", false);
        PlayerMovement.controlEnabled = true;
        controlEnabled = true;
        isEating = false;
        savedEnemy.TakeDamage(10);
        Hunger.hungerLevel += hungerIncrement;

        yield return new WaitForSeconds(eatCooldown);
        canEat = true;
    }
}
