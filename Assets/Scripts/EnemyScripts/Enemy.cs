using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    protected float attackCooldown;
    protected int health;

    protected abstract void Movement();

    private void Update()
    {
        Movement();

        if (attackCooldown > 0) attackCooldown = Mathf.Max(0, attackCooldown - Time.deltaTime);
    }
}
