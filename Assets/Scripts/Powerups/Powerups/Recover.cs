using UnityEngine;
using static Default.Default;

public class Recover : Powerup, Passive
{
    private void Start()
    {
        health = maxHealth;
        player = FindObjectOfType<PlayerAttack>();
        sr = GetComponent<SpriteRenderer>();
        audioSource = GetComponent<AudioSource>();
    }

    public void OnPickup()
    {
        Player p = player.GetComponent<Player>();

        for (int i = 0; i < MAX_PLAYER_HEALTH; i++)
        {
            if (!p.GainHealth(1)) break;
        }

        Hunger.hungerLevel = MAX_HUNGER;

        for (int i = 0; i < player.powerups.Length; i++) 
        {
            if (player.powerups[i] is Recover) 
            {
                player.powerups[i] = null;
                Destroy(this);
            }
        }
    }
}
