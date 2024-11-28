using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private const int maxHealth = 5;

    private const float invincibilityTime = 1.5f;
    private const float toggleTime = 0.15f;

    private Color transparentColor = new Color(1, 1, 1, 0.15f);

    private bool invulnerable;
    public int health;
    private SpriteRenderer sr;

    private void Start()
    {
        health = maxHealth;
        sr = GetComponent<SpriteRenderer>();
    }

    public void TakeDamage(int damage)
    {
        if (invulnerable) return;

        health -= damage;

        if (health <= 0) Respawn();
        else StartCoroutine(InvincibilityTimer());
    }

    private IEnumerator InvincibilityTimer()
    {
        invulnerable = true;
        float elapsedTime = 0;

        while (elapsedTime < invincibilityTime)
        {
            sr.color = elapsedTime % toggleTime > toggleTime / 2 ? transparentColor : Color.white;
            yield return null;
            elapsedTime += Time.deltaTime;
        }

        sr.color = Color.white;
        invulnerable = false;
    }

    private void Respawn()
    {
        transform.position = Vector3.zero;
        health = maxHealth;
    }
}
