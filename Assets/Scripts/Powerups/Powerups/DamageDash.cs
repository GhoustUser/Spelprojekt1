using UnityEngine;

public class DamageDash : Powerup, Passive
{
    private void Start()
    {
        health = maxHealth;
        player = FindObjectOfType<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();
    }

    public void OnPickup()
    {
        player.GetComponent<PlayerMovement>().damageDash = true;
    }
}
