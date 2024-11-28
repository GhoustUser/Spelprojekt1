using System;
using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private int health;

    private bool invulnerable;

    private const float invincibilityTime = 1.5f;

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

        yield return new WaitForSeconds(invincibilityTime);

        invulnerable = false;
    }

    private void Respawn()
    {
        throw new NotImplementedException(); 
    }
}
