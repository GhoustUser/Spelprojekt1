using UnityEngine;

public class ParalysingTouch : Powerup, Passive
{
    [Header("Powerup")]
    [SerializeField] [Range(0, 1)] private float stunOdds;
    [SerializeField][Range(0, 10)] private float stunMultiplier;
    private void Start()
    {
        health = maxHealth;
        player = FindObjectOfType<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPickup()
    {
        Player.paralysingTouch = true;
        Player.stunOdds = stunOdds;
        Player.stunMultiplier = stunMultiplier;
    }
}
