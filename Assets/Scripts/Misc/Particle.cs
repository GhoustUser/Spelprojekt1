using UnityEngine;

public class Particle : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;
    private float timer;

    void Update()
    {
        // Destroys the gameobject associated with the particle system after the effect has been played.
        timer += Time.deltaTime;
        if (timer > ps.main.duration) Destroy(gameObject);  
    }
}
