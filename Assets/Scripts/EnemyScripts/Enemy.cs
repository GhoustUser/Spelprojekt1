using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] protected int health;

    protected abstract void Movement();
    protected abstract void Death();

    private void Update()
    {
        Movement();
    }

    public void TakeDamage(int amount)
    {
        health -= amount;

        if (health <= 0) Death();
    }
}
