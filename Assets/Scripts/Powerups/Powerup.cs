using System.Collections;
using UnityEngine;

public abstract class Powerup : MonoBehaviour
{
    private bool equipped;
    protected PlayerAttack player;

    public abstract IEnumerator Activate(Vector3 direction);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // Checks if the collided hitbox is the player.
        if (!collision.TryGetComponent<PlayerAttack>(out PlayerAttack player)) return;
        this.player = player;

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

        // If the powerup has been equipped, set it to false.
        if (equipped) GetComponent<SpriteRenderer>().enabled = false;
    }
}
