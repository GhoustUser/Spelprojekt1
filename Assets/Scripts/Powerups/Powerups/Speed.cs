using UnityEngine;

public class Speed : Powerup, Passive
{
    [Header("Powerup")]
    [SerializeField] private float newSpeed;

    void Start()
    {
        health = maxHealth;
        player = FindObjectOfType<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPickup()
    {
        player.GetComponent<PlayerMovement>().SetSpeed(10);
    }
}
