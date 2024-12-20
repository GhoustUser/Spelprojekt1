using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    private enum BossPhase
    {
        Dead, Phase1, Phase2
    }

    [Header("Movement")]
    [SerializeField] private float movementSpeed;

    [Header("Limbs")]
    [Tooltip("The amount of limbs the boss starts with.")]
    [SerializeField] private int startingLimbs;
    [Tooltip("The time delay between the individual boss limbs.")]
    [SerializeField] private float limbDelay;

    [Header("Components")]
    [SerializeField] private GameObject limbPrefab;
    [SerializeField] private List<Texture> limbTextures;

    // private BossPhase phase;
    private Vector2 targetPosition;
    private Vector2 targetDirection;
    private List<Limb> limbs;


    private void Start()
    {
        health = maxHealth;
        // phase = BossPhase.Phase1;
        targetPosition = transform.position;
        targetDirection = new Vector2(Random.Range(0, 360), Random.Range(0, 360)).normalized;

        for (int i = 0; i < startingLimbs; i++)
        {
            Limb limb = Instantiate(limbPrefab, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360))).GetComponent<Limb>();
            limb.speed = movementSpeed;
            limb.boss = this;
            limbs.Add(limb);
        }
    }

    protected override void Movement()
    {
        transform.position = Vector2.MoveTowards(transform.position, targetPosition, movementSpeed * Time.deltaTime);

        if (Vector2.Distance(transform.position, targetPosition) < 0.1f)
        {
            RaycastHit2D hit = Physics2D.Raycast(transform.position, targetDirection, 20, wallLayer);
            targetPosition = hit.point - Vector2.one * targetDirection;
            targetDirection = Vector2.Reflect(targetDirection, hit.normal);

            StartCoroutine(LimbFollow(targetPosition));
        }
    }

    public void DestroyLimb(Limb limb)
    {
        for (int i = limbs.Count - 1; i > limbs.IndexOf(limb); i--)
        {
            limbs[i].transform.position = limbs[i - 1].transform.position;
        }
        limbs.Remove(limb);
        Destroy(limb.gameObject);
    }

    private IEnumerator LimbFollow(Vector2 tPos)
    {
        for (int i = 0; i < limbs.Count; i++)
        {
            limbs[i].tPos.Add(tPos);
            yield return new WaitForSeconds(limbDelay);
        }
        yield break;
    }

    protected override void Death()
    {
        throw new System.NotImplementedException();
    }

    public override IEnumerator ApplyKnockback(Vector3 direction, float knockbackStrength, float stunTime)
    {
        yield break;
    }
}
