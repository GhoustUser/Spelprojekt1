using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Audio;

public class PlayerAttack : MonoBehaviour
{
    /* -------- Settings --------*/
    [Header("Attack")]
    [Tooltip("Time before you can attack again. (In seconds)")]
    [SerializeField] private float attackCooldown = 1.0f;
    [SerializeField] private int attackDamage = 1;
    [Tooltip("The size of the attack hurtbox.")]
    [SerializeField] private float attackRange = 1f;

    [Header("Knockback")]
    [SerializeField] private float knockbackStrength;
    [SerializeField] private float stunTime;

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
    [SerializeField] private LayerMask attackLayer;
    [SerializeField] private LayerMask enemyLayer;

    [Header("Components")]
    [SerializeField] private GameObject weapon;
    [SerializeField] private Animator clawAnimator;

    [Header("Audio Settings")]
    [SerializeField] private AudioClip hitSound; 
    [SerializeField] private AudioMixerGroup audioMixerGroup; 
    [SerializeField] private AudioClip eatSound;
    [SerializeField] private AudioClip swooshSound1;
    [SerializeField] private AudioMixerGroup swooshMixer1;
    [SerializeField] private AudioClip swooshSound2;
    [SerializeField] private AudioMixerGroup swooshMixer2;

    /* -------- Object references --------*/
    private Camera cam;
    private Animator animator;
    private AudioSource audioSource;
    private SpriteRenderer sr;

    /* -------- Variables --------*/
    private float initStunTime;
    private const float attackDuration = 0.2f; // WIP, there currently is no lingering hurtbox for the attack.
    private Vector3 atkPoint; // The center point of the attack hitbox.

    private bool canAttack;
    private bool canSpAttack;
    private bool canEat;
    private int attackCounter;

    [HideInInspector] public bool isEating;

    /* -------- Properties --------*/
    public static bool controlEnabled { get; set; } = true; // You can edit this variable from Unity Events
    
    
    /* -------- Start --------*/
    private void Start()
    {
        cam = GetComponentInChildren<Camera>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        sr = GetComponent<SpriteRenderer>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (audioMixerGroup != null)
        {
            audioSource.outputAudioMixerGroup = audioMixerGroup;
        }
        
        canAttack = true;
        canSpAttack = true;
        canEat = true;

        initStunTime = stunTime;
    }

    public void OnAttack(InputAction.CallbackContext context)
    {
        if (!canAttack || !controlEnabled) return;

        // Starts attack coroutine.
        StartCoroutine(Attack());
    }

    /* -------- Attack --------*/
    private IEnumerator Attack()
    {
        // Initializes the attack.
        attackCounter++;
        clawAnimator.SetBool(attackCounter % 2 == 0 ? "isAttacking" : "attackBack", true);
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

        PlaySwooshSound();
        // Finds all overlapping colliders and adds them to an array.
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(atkPoint, attackRange, attackLayer);

        foreach (Collider2D enemy in hitEnemies)
        {
            // If the found collider belongs to an enemy, damage the enemy and apply knockback.
            if (!enemy.TryGetComponent<Entity>(out Entity e)) continue;
            
            e.TakeDamage(attackDamage);
            stunTime = initStunTime;
            if (Player.paralysingTouch)
            {
                if (Random.Range(0f, 1f) < Player.stunOdds)
                {
                    stunTime = stunTime * Player.stunMultiplier;
                    StartCoroutine(ParalysingTouch(e, stunTime));
                }
            }

            StartCoroutine(e.ApplyKnockback(attackDirection.normalized, knockbackStrength, stunTime));
            
            if (audioSource != null && hitSound != null)
            {
                audioSource.PlayOneShot(hitSound);
            }
        }

        // Waits for the attack to finish.
        yield return new WaitForSeconds(attackDuration);

        // Stops attacking.
        clawAnimator.SetBool(attackCounter % 2 == 0 ? "isAttacking" : "attackBack", false);
        weapon.transform.localPosition = Vector3.zero;

        // Waits for the attack cooldown.
        yield return new WaitForSeconds(attackCooldown - attackDuration);
        canAttack = true;
    }

    /* -------- Paralysing Touch --------*/
    private IEnumerator ParalysingTouch(Entity entity, float stunTime)
    {
        if (!entity.TryGetComponent<Enemy>(out Enemy e)) yield break;

        Player p = GetComponent<Player>();
        SpriteRenderer sr = e.GetComponent<SpriteRenderer>();
        e.paralysed = true;
        Color initColor = sr.color;
        sr.color = Color.green;

        yield return new WaitForSeconds(stunTime);
        sr.color = initColor;
        e.paralysed = false;
    }

    /* -------- On Sp Attack --------*/
    public void OnSpAttack(InputAction.CallbackContext context)
    {
        if (!canSpAttack || !controlEnabled) return;

        // Starts special attack coroutine.
        StartCoroutine(SpAttack());
    }

    /* -------- Sp Attack --------*/
    private IEnumerator SpAttack()
    {
        // Initiates special attack.
        canSpAttack = false;

        // Sets the attack direction to the direction of the mouse.
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector3 attackDirection = new Vector3(mousePos.x - transform.position.x, mousePos.y - transform.position.y, 0).normalized;

        // Finds the non-empty spaces in the powerups array and activates the effects of the found powerups.
        foreach (Ability a in powerups.Where(a => a is Ability).ToArray()) StartCoroutine(a.Activate(attackDirection));

        // Waits for the special attack cooldown.
        yield return new WaitForSeconds(spAttackCooldown);
        canSpAttack = true;
    }

    /* -------- On Eat --------*/
    public void OnEat(InputAction.CallbackContext context)
    {
        if (!canEat || !controlEnabled) return;
        
        StartCoroutine(Eat());
    }

    /* -------- Eat --------*/
    private IEnumerator Eat()
    {
        canEat = false;
        isEating = true;
        
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, eatRange, enemyLayer);

        Enemy savedEnemy = null;
        foreach (Collider2D coll in hitEnemies)
        {
            if (!coll.TryGetComponent<Enemy>(out Enemy e)) continue;
            if (e.healthState != HealthState.HeavilyInjured && !e.paralysed) continue;
            savedEnemy = e;
            savedEnemy.eaten = true;
            break;
        }
        
        if (savedEnemy == null)
        {
            canEat = true;
            yield break;
        }
        
        PlayEatSound();
        
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

        if (savedEnemy.boss) Hunger.hungerLevel += hungerIncrement * 1.5f;
        else if (savedEnemy is Scientist) Hunger.hungerLevel += hungerIncrement * 0.5f;
        else Hunger.hungerLevel += hungerIncrement;

        if (savedEnemy is not Scientist) ScoreManager.enemiesEaten++;

        yield return new WaitForSeconds(eatCooldown);
        canEat = true;
    }

    /* -------- Play Eat Sound --------*/
    private void PlayEatSound()
    {
        if (audioSource != null && eatSound != null)
        {
            audioSource.PlayOneShot(eatSound);  
        }
    }

    /* -------- Play Swoosh Sound --------*/
    private void PlaySwooshSound()
    {
        if (audioSource == null) return;

    
        if (Random.value > 0.5f && swooshSound1 != null && swooshMixer1 != null)
        {
        
            audioSource.outputAudioMixerGroup = swooshMixer1;
            audioSource.PlayOneShot(swooshSound1);
        }
        else if (swooshSound2 != null && swooshMixer2 != null)
        {
        
            audioSource.outputAudioMixerGroup = swooshMixer2;
            audioSource.PlayOneShot(swooshSound2);
        }
    }
}
