using System.Collections;
using UnityEngine;

public abstract class Entity : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] protected int maxHealth;
    [Tooltip("The enemy's current health.")]
    [SerializeField] protected int health;

    [Header("LayerMasks")]
    [Tooltip("The layers the enemy's raycasting will collide with.")]
    [SerializeField] protected LayerMask wallLayer;

    [HideInInspector] public bool stunned;
    [HideInInspector] public Vector3 knockbackPosition;
    protected Vector3 originalPosition;
    protected Animator animator;

    protected abstract void Death();

    public virtual void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0) Death();
    }

    public virtual IEnumerator ApplyKnockback(Vector3 direction, float knockbackStrength, float stunTime)
    {
        // Sends a linecast with the length of the knockback strength in the knockback direction.
        originalPosition = transform.position;
        RaycastHit2D hit = Physics2D.Linecast(originalPosition, originalPosition + direction * knockbackStrength, wallLayer);

        // If the linecast hit a wall, reduce the knockback position so that it doesn't go past the wall.
        float kbStrength = hit.distance == 0 ? knockbackStrength : knockbackStrength / hit.distance;
        knockbackPosition = originalPosition + direction * kbStrength;

        // Stuns the entity.
        stunned = true;
        if (animator != null) animator.SetBool("stunned", true);

        // Waits for the stunTime and sets stunned to false;
        yield return new WaitForSeconds(stunTime);
        if (animator != null) animator.SetBool("stunned", false);
        stunned = false;
    }
}
