using UnityEngine;

public class Particle : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;
    private float timer;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > ps.main.duration) Destroy(gameObject);  
    }
}
