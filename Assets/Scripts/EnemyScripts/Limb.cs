using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Limb : Enemy
{
    [HideInInspector] public bool toBeRemoved;
    [HideInInspector] public List<Vector2> tPos;
    [HideInInspector] public float speed;
    [HideInInspector] public Boss boss;


    private void Start()
    {
        tPos = new List<Vector2>();
        sr = GetComponent<SpriteRenderer>();
        health = maxHealth;
    }

    protected override void Movement()
    {
        if (tPos.Count < 1) return;
        transform.position = Vector2.MoveTowards(transform.position, tPos[0], speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, tPos[0]) < 0.1f) tPos.RemoveAt(0);
    }

    public override void TakeDamage(int amount)
    {
        base.TakeDamage(amount);
        sr.color = Color.red;
        Invoke(nameof(ChangeColor), 0.2f);
    }

    private void ChangeColor()
    {
        sr.color = Color.white;
    }

    protected override void Death()
    {
        // Play limb gore here.
        boss.DestroyLimb(this);
    }

    public override IEnumerator ApplyKnockback(Vector3 direction)
    {
        yield break;
    }
}