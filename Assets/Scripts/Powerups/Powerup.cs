using UnityEngine;

public abstract class Powerup : Entity
{
    [Header("Sprites")]
    [SerializeField] private Sprite cracked;
    [SerializeField] private string powerupDescription;
    [SerializeField] private AudioClip destructionSFX;
    [SerializeField] private AudioClip impactSFX;

    protected AudioSource audioSource;
    public delegate void PowerupDestroyed(); // Change parameter and return type to whatever you want.
    public event PowerupDestroyed OnPowerupDestroyed;

    private bool equipped;
    protected PlayerAttack player;
    protected GameObject powerupUI;
    protected SpriteRenderer sr;

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(1);
        switch (health)
        {
            case 1:
                audioSource.PlayOneShot(impactSFX);
                if (cracked == null) break;
                sr.sprite = cracked;
                break;
        }
    }

    protected override void Death()
    {
        // Attaches the powerup to the player in the first available slot.

        for (int i = 0; i < player.powerups.Length; i++)
        {
            if (player.powerups[i] != null) continue;
            player.powerups[i] = this;
            transform.parent = player.transform;
            transform.localPosition = Vector3.zero;
            equipped = true;
            break;
        }

        // If the powerup has been equipped, hide it and make it untouchable.
        if (equipped)
        {
            sr.enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
            gameObject.layer = 10;
            if (TryGetComponent<Passive>(out Passive p)) p.OnPickup();
            audioSource.PlayOneShot(destructionSFX);

            OnPowerupDestroyed?.Invoke();
        }
    }

    public void OtherPowerupDestroyed()
    {
        // Maybe add a destruction animation trigger here.
        Destroy(gameObject);
    }
}
