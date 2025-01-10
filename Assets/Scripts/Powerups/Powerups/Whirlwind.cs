using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Whirlwind : Powerup, Ability
{
    [Header("Hunger")]
    [SerializeField] private float hungerIncrement;

    [Header("Powerup")]
    [SerializeField] private float chargeUpDuration;
    [SerializeField] private float activeDuration;
    [SerializeField] private float attackRadius;
    [SerializeField] private float knockbackStrength;
    [SerializeField] private float stunTime;
    [SerializeField] private int attackDamage;
    [SerializeField] private float rotationSpeed;

    [Header("LayerMasks")]
    [SerializeField] private LayerMask enemyLayer;

    [Header("Components")]
    [SerializeField] private GameObject whirlwindHitbox;

    private void Start()
    {
        player = FindObjectOfType<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
        health = maxHealth;
    }

    public IEnumerator Activate(Vector3 direction)
    {
        if (hungerIncrement > Hunger.hungerLevel) yield break;
        Hunger.hungerLevel -= hungerIncrement;

        GameObject whirlwind = Instantiate(whirlwindHitbox, player.transform.position, Quaternion.identity);
        whirlwind.transform.GetComponent<SpriteRenderer>().sortingOrder = 2;
        PlayerMovement pMov = player.GetComponent<PlayerMovement>();
        pMov.canDash = false;

        float chargeUpCounter = 0;
        while (chargeUpCounter < chargeUpDuration)
        {
            whirlwind.transform.position = player.transform.position;
            yield return null;
            chargeUpCounter += Time.deltaTime;
        }
        print("Chargeup done!");
        float activeCounter = 0;
        HashSet<Collider2D> hitColliders = new HashSet<Collider2D>();
        while (activeCounter < activeDuration)
        {
            whirlwind.transform.position = player.transform.position;
            whirlwind.transform.Rotate(Vector3.back * (rotationSpeed * Time.deltaTime));
            Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(player.transform.position, attackRadius, enemyLayer);

            foreach (Collider2D coll in hitEnemies)
            {               
                if (!coll.TryGetComponent<Enemy>(out Enemy e) || hitColliders.Contains(coll)) continue;

                hitColliders.Add(coll);
                StartCoroutine(RemoveCollider(hitColliders, coll));

                StartCoroutine(e.ApplyKnockback((e.transform.position - player.transform.position).normalized, knockbackStrength, stunTime));
                e.TakeDamage(attackDamage);
            }
            yield return null;
            activeCounter += Time.deltaTime;
        }
        print("ActiveDuration done!");


        pMov.canDash = true;
        Destroy(whirlwind);
    }

    private IEnumerator RemoveCollider(HashSet<Collider2D> set, Collider2D coll)
    {
        yield return new WaitForSeconds(0.5f);
        set.Remove(coll);
    }
}
