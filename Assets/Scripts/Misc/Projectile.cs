using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private GameObject child;

    [HideInInspector] public Vector3 direction;
    private bool hit;

    private void Update()
    {
        if (!hit) transform.position += speed * direction * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player p)) return;
        if (collision.gameObject.TryGetComponent<Enemy>(out Enemy e)) e.TakeDamage(damage);
        hit = true;
        Destroy(child);
        Invoke(nameof(DestroyThis), 0.3f); // 0.3 seconds is the time it takes for the trail to disappear.
    }

    private void DestroyThis()
    {
        Destroy(gameObject);
    }
}
