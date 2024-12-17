public class Generator : Entity
{
    public delegate void GeneratorDestroyed(); // Change parameter and return type to whatever you want.
    public static event GeneratorDestroyed OnGeneratorDestroyed;

    private void Start()
    {
        health = maxHealth;
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

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

        // Changes to broken layer, so that it won't be registered for attacks.
        gameObject.layer = 10;
    }
}
