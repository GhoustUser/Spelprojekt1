using System.Collections;
using UnityEngine;

public class TestPowerup : Powerup
{
    [SerializeField] private GameObject projectile;

    public override IEnumerator Activate(Vector3 d)
    {
        // Creates a projectile.
        GameObject go = Instantiate(projectile);
        go.transform.position = player.transform.position;
        Projectile p = go.GetComponent<Projectile>();

        // Sends the projectile in the direction of the cursor.
        p.direction = d;
        yield break;
    }
}
