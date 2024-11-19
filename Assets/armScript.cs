using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class armScript : MonoBehaviour
{
    public Transform ParentObject;
    //public Tilemap tilemap;
    private LineRenderer lr;
    public Vector2 TargetPos;
    
    public float MomentumFactor = 1.0f;
    public int Attempts = 5;
    public float Gravity = 1.0f;

    const int SegmentCount = 20;
    const float TotalLength = 3.0f;

    private Vector3[] prevPositions;

    private float segmentLength = 1.0f;


    // Start is called before the first frame update
    void Start()
    {
        lr = GetComponent<LineRenderer>();
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.positionCount = SegmentCount;
        prevPositions = new Vector3[SegmentCount];

        segmentLength = TotalLength / SegmentCount;
        for (uint i = 0; i < SegmentCount; i++) prevPositions[i] = ParentObject.position + new Vector3(0, -i * segmentLength, 0);
        lr.SetPositions(prevPositions);
    }

    // Update is called once per frame
    void Update()
    {

        //momentum
        for (int i = 1; i < lr.positionCount; i++)
        {
            Vector3 pos = lr.GetPosition(i);
            Vector3 positionBeforeUpdate = pos;
            pos += (pos - prevPositions[i]) * MomentumFactor;
            pos += Vector3.down * Gravity * Time.deltaTime * Time.deltaTime;
            lr.SetPosition(i, pos);
            prevPositions[i] = positionBeforeUpdate;
        }

        //fix line length
        lr.SetPosition(0, ParentObject.position);
        lr.SetPosition(lr.positionCount - 1, TargetPos);
        for (uint a = 0; a < Attempts; a++)
        {
            for (int i = 1; i < lr.positionCount; i++)
            {
                Vector3 pos = lr.GetPosition(i);
                Vector3 prevPos = lr.GetPosition(i - 1);
                float distance = Vector3.Distance(pos, prevPos);
                if (distance > segmentLength)
                {
                    Vector3 normal = Vector3.Normalize(pos - prevPos);
                    Vector3 midPoint = (pos + prevPos) * 0.5f;
                    if (i > 1)
                    {
                        lr.SetPosition(i, midPoint + normal * 0.5f * segmentLength);
                        lr.SetPosition(i - 1, midPoint - normal * 0.5f * segmentLength);
                    }
                    else
                    {
                        lr.SetPosition(i, midPoint + normal * 1.0f * segmentLength);
                    }
                }
            }
        }
    }
}
