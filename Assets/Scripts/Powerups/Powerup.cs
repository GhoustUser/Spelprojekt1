using UnityEngine;

public abstract class Powerup : MonoBehaviour
{
    private bool equipped;
    protected PlayerAttack player;

    public abstract void Activate(Vector3 direction);

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.gameObject.TryGetComponent<PlayerAttack>(out PlayerAttack player)) return;
        this.player = player;

        for (int i = 0; i < player.powerups.Length - 1; i++)
        {
            if (player.powerups[i] != null) continue;
            player.powerups[i] = this;
            equipped = true;
            break;
        }

        if (equipped) gameObject.SetActive(false);
    }
}
