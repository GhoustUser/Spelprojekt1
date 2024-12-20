using TMPro;
using UnityEngine;

public abstract class Powerup : Entity
{
    [Header("Sprites")]
    [SerializeField] private string powerupDescription;
    [SerializeField] private GameObject powerupText;
    [SerializeField] private GameObject glassShatter;
    [SerializeField] private AudioClip destructionSFX;
    [SerializeField] private AudioClip impactSFX;
    [SerializeField] private Sprite emptyTube;
    [SerializeField] private Sprite brokenTube;

    protected AudioSource audioSource;
    public delegate void PowerupDestroyed(); // Change parameter and return type to whatever you want.
    public event PowerupDestroyed OnPowerupDestroyed;

    [SerializeField] private bool equipped;
    private GameObject textInstance;
    protected PlayerAttack player;
    protected GameObject powerupUI;
    protected SpriteRenderer sr;

    public override void TakeDamage(int amount)
    {
        GameObject go = Instantiate(glassShatter);
        go.transform.position = transform.position;
        go.GetComponent<ParticleSystem>().Play();

        base.TakeDamage(1);
        switch (health)
        {
            case 1:
                audioSource.PlayOneShot(impactSFX);
                break;
        }
    }

    private void Update()
    {
        if (!equipped)
        {
            if (Vector2.Distance(player.transform.position, transform.position) < 1.5f && textInstance == null)
            {
                textInstance = Instantiate(powerupText, new Vector2(transform.position.x, transform.position.y), Quaternion.identity);
                textInstance.GetComponent<Canvas>().worldCamera = player.GetComponentInChildren<Camera>();
                textInstance.GetComponentInChildren<TextMeshProUGUI>().text = powerupDescription;
            }
            else if (Vector2.Distance(player.transform.position, transform.position) > 1.5f && textInstance != null)
            {
                Destroy(textInstance);
                textInstance = null;
            }
        }

    }

    protected override void Death()
    {
        // Attaches the powerup to the player in the first available slot.

        for (int i = 0; i < player.powerups.Length; i++)
        {
            if (player.powerups[i] != null) continue;
            player.powerups[i] = this;
            equipped = true;
            break;
        }

        // If the powerup has been equipped, hide it and make it untouchable.
        if (equipped)
        {
            sr.sprite = brokenTube;
            GetComponent<Animator>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
            gameObject.layer = 10;
            if (TryGetComponent<Passive>(out Passive p)) p.OnPickup();
            audioSource.PlayOneShot(destructionSFX);
            if (textInstance != null) Destroy(textInstance);
            textInstance = null;

            OnPowerupDestroyed?.Invoke();
        }
    }

    public void OtherPowerupDestroyed()
    {
        // Maybe add a destruction animation trigger here.
        GetComponent<BoxCollider2D>().enabled = false;
        GetComponent<Animator>().enabled = false;
        gameObject.layer = 10;
        sr.sprite = emptyTube;
        if (textInstance != null) Destroy(textInstance);
        textInstance = null;
    }
}
