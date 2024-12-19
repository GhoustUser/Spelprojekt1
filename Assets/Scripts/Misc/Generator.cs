using UnityEngine;


public class Generator : Entity
{
    public delegate void GeneratorDestroyed(); // Change parameter and return type to whatever you want.
    public event GeneratorDestroyed OnGeneratorDestroyed;
    public static bool isDestroyed = false;

    [SerializeField] private AudioClip destructionSFX;
    [SerializeField] private AudioClip impactSFX;

    private AudioSource audioSource;

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
        health = maxHealth;
        isDestroyed = false;
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        audioSource.PlayOneShot(impactSFX);
        switch (health)
        {
            case 2:
                // Add code that changes the appearance after you hit the generator.
                break;
            case 1:
                // Add code that changes the appearance after you hit the generator.
                break;
        }
    }

    protected override void Death()
    {
        OnGeneratorDestroyed?.Invoke();
        isDestroyed = true;
        audioSource.PlayOneShot(destructionSFX);

        // Changes to broken layer, so that it won't be registered for attacks.
        gameObject.layer = 10;
    }
}
