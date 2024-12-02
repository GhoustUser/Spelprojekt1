using UnityEngine;

public class Projectile : MonoBehaviour
{
    [SerializeField] private float speed;
    [SerializeField] private int damage;
    [SerializeField] private Rigidbody2D rb;

    [HideInInspector] public Vector3 direction;

    private void Update()
    {
        transform.position += speed * direction * Time.deltaTime;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.TryGetComponent<Player>(out Player p)) return;
        if (collision.gameObject.TryGetComponent<Enemy>(out Enemy e)) e.TakeDamage(damage);
        Destroy(gameObject);
    }
}
