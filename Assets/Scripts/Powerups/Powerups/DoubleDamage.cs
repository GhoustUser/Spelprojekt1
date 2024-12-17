using UnityEngine;

public class DoubleDamage : Powerup, Passive
{
    private void Start()
    {
        health = maxHealth;
        player = FindObjectOfType<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void OnPickup()
    {
        player.GetComponent<Player>().doubleDamage = true;
        player.doubleDamage = true;
    }
}