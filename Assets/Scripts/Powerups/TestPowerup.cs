using UnityEngine;

public class TestPowerup : Powerup
{
    [SerializeField] private GameObject projectile;

    public override void Activate(Vector3 d)
    {
        Projectile p = Instantiate(projectile).GetComponent<Projectile>();
        p.transform.position = player.transform.position;
        p.direction = d;
    }
}
