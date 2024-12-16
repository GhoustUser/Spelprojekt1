using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class Powerup : Enemy
{
    [Header("Sprites")]
    [SerializeField] private Sprite cracked;
    [SerializeField] private string powerupDescription;

    private const float detectionRange = 2.5f;
    private bool equipped;
    protected PlayerAttack player;
    protected GameObject powerupUI;

    public abstract IEnumerator Activate(Vector3 direction);

    protected override void Movement() { }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);

        switch (health)
        {
            case 1:
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
        }
    }
}
