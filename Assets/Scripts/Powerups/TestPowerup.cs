using UnityEngine;

public class TestPowerup : Powerup
{
    [SerializeField] private GameObject projectile;

    public override void Activate(Vector3 d)
    {
        GameObject go = Instantiate(projectile);
        Projectile p = go.GetComponent<Projectile>();
        go.transform.position = player.transform.position;
        p.direction = d;
    }
}
