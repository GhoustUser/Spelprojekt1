using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
    private enum BossPhase
    {
        Dead, Phase1, Phase2
    }

    [SerializeField] private int maxHealth;
    [SerializeField] private int startingLimbs;
    [SerializeField] private List<Limb> limbs;
    [SerializeField] private float movementSpeed;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private GameObject limbPrefab;
    [SerializeField] private float limbDelay;

    private int health;
    private BossPhase phase;
    private Vector2 targetPosition;
    private Vector2 targetDirection;

    private void Start()
    {
        health = maxHealth;
        phase = BossPhase.Phase1;
        targetPosition = transform.position;
        targetDirection = new Vector2(Random.Range(0, 360), Random.Range(0, 360)).normalized;
        //targetDirection = Vector2.down;

        for (int i = 0; i < startingLimbs; i++) limbs.Add(Instantiate(limbPrefab, transform.position, Quaternion.Euler(0, 0, Random.Range(0, 360))).GetComponent<Limb>());
        limbs[0].transform.localScale = new Vector3(2, 2, 2);
    }

    private void Update()
    {
        switch(phase)
        {
            case BossPhase.Phase1:
                Movement();
                foreach (Limb limb in limbs)
                {
                    if (limb.follow) limb.Move(movementSpeed);
                }
                break;
        }
    }

    private void Movement()
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

    private IEnumerator LimbFollow(Vector2 tPos)
    {
        foreach (Limb limb in limbs)
        {
            limb.follow = true;
            limb.tPos.Add(tPos);
            yield return new WaitForSeconds(limbDelay);
        }
        yield break;
    }
}

