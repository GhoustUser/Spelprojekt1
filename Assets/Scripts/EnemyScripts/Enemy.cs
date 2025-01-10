using LevelGen;
using UnityEngine;

public abstract class Enemy : Entity
{
    [SerializeField] private AudioClip hitSound;
    [SerializeField] protected GameObject bloodStain;
    [SerializeField] protected bool canBleed;

    protected Rigidbody2D rb;
    protected SpriteRenderer sr;
    protected AudioSource audioSource;

    [HideInInspector] public HealthState healthState;
    [HideInInspector] public int room;
    [HideInInspector] public Room roomRef;
    [HideInInspector] public bool eaten;
    [HideInInspector] public bool paralysed;

    private float bleedTimer;

    // Enemy specific Movement function.
    protected abstract void Movement();

    private void Start()
    {
        healthState = HealthState.Healthy;
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!eaten) Movement();

        bleedTimer = Mathf.Max(bleedTimer - Time.deltaTime, 0);

        if (bleedTimer > 0 || healthState == HealthState.Healthy || !canBleed) return;

        GameObject stain;
        switch (healthState)
        {
            // Spawns blood at different rates depending on the enemy's health state.
            case HealthState.HeavilyInjured:
                bleedTimer = 0.5f;
                stain = Instantiate(bloodStain, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                stain.transform.localScale = sr.size;
                break;
            case HealthState.Injured:
                bleedTimer = 1f;
                stain = Instantiate(bloodStain, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360)));
                stain.transform.localScale = sr.size;
                break;
        }

    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        bleedTimer = 1f;
        
        if (audioSource != null && hitSound != null && health > 0)
        {
            //audioSource.PlayOneShot(hitSound);
        }
        
        // Temporary fix for healthstates.
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
                sr.color = new Color(1, .25f, .25f, 1);
                break;
        }
    }
}

public enum HealthState
{
    Healthy, Injured, HeavilyInjured
}
