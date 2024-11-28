using UnityEngine;

public abstract class Enemy : MonoBehaviour
{
    [Header("Parameters")]
    [SerializeField] protected int health;
    [SerializeField] private GameObject bloodStain;

    protected HealthState healthState;

    private float bleedTimer;
    private TrailRenderer tr;

    protected abstract void Movement();
    protected abstract void Death();

    private void Start()
    {
        tr = GetComponent<TrailRenderer>();
        healthState = HealthState.Healthy;
    }

    private void Update()
    {
        Movement();

        bleedTimer = Mathf.Max(bleedTimer - Time.deltaTime, 0);

        if (bleedTimer > 0 || healthState == HealthState.Healthy) return;

        switch (healthState)
        {
            case HealthState.HeavilyInjured:
                bleedTimer = 0.5f;
                Instantiate(bloodStain, transform.position, Quaternion.identity);
                break;
            case HealthState.Injured:
                bleedTimer = 1f;
                Instantiate(bloodStain, transform.position, Quaternion.identity);
                break;
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        bleedTimer = 1f;

        // Temporary fix for healthstates
        switch (health)
        {
            case 3:
                healthState = HealthState.Healthy;
                break;
            case 2: 
                healthState = HealthState.Injured;
                break;
            case 1:
                healthState = HealthState.HeavilyInjured;
                break;
        }

        if (health <= 0) Death();
    }
}

public enum HealthState
{
    Healthy, Injured, HeavilyInjured
}
