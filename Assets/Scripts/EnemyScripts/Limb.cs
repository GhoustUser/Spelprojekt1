using System.Collections.Generic;
using UnityEngine;

public class Limb : MonoBehaviour
{
    public bool follow;
    public List<Vector2> tPos;

    private void Start()
    {
        tPos = new List<Vector2>();
    }

    public void Move(float speed)
    {
        if (tPos.Count < 1) return;
        transform.position = Vector2.MoveTowards(transform.position, tPos[0], speed * Time.deltaTime);

        if (Vector2.Distance(transform.position, tPos[0]) < 0.1f) tPos.RemoveAt(0);
    }
}