using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class armScript : MonoBehaviour
{
    /* -------- Settings --------*/
    [HideInInspector]public float TotalLength = 1.6f;
    [HideInInspector]public float MomentumFactor = 0.9f;
    [HideInInspector]public int Attempts = 5;
    [HideInInspector]public float AngleRange = 45.0f;
    
    [HideInInspector]public Material ArmMaterial;
    
    /* -------- Variables --------*/
    private Transform parentTransform;
    private Vector3 prevParentPosition;
    private Vector3 parentMovement;
    private Vector3 TargetOrigin;
    private Vector3 TargetPos;
    //public Tilemap tilemap;
    private LineRenderer lr;
    

    private const int SegmentCount = 20;

    private Vector3[] prevPositions;

    private float segmentLength = 1.0f;

    /* -------- Properties --------*/
    private Vector3 EndPos => lr.GetPosition(lr.positionCount - 1);
    
    /* -------- Start --------*/
    void Start()
    {
        //line renderer settings
        lr = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.positionCount = SegmentCount;
        lr.material = ArmMaterial;
        
        prevPositions = new Vector3[SegmentCount];
        parentTransform = transform.parent.parent;

        segmentLength = TotalLength / SegmentCount;
        for (uint i = 0; i < SegmentCount; i++) prevPositions[i] = parentTransform.position + new Vector3(0, -i * segmentLength, 0);
        lr.SetPositions(prevPositions);
    }

    /* -------- Update --------*/
    void Update()
    {
        //get parent movement
        parentMovement = parentTransform.position - prevParentPosition;
        prevParentPosition = parentTransform.position;
        
        //calculate arm target position
        TargetOrigin = parentTransform.position + parentMovement.normalized * TotalLength;
        if (Vector3.Distance(EndPos, TargetOrigin) > TotalLength * 1.8f)
        {
            Vector2 dir = parentMovement.normalized;
            dir = dir.Rotate(Random.Range(-AngleRange, AngleRange));
            TargetPos = parentTransform.position + new Vector3(dir.x, dir.y, 0) * TotalLength;
            //randomize arm
            for (int i = 1; i < lr.positionCount - 1; i++)
            {
                lr.SetPosition(i, lr.GetPosition(i) + new Vector3(Random.value - 0.5f, Random.value - 0.5f, 0));
            }
        }

        //momentum
        for (int i = 1; i < lr.positionCount; i++)
        {
            Vector3 pos = lr.GetPosition(i);
            Vector3 positionBeforeUpdate = pos;
            pos += (pos - prevPositions[i]) * MomentumFactor;
            lr.SetPosition(i, pos);
            prevPositions[i] = positionBeforeUpdate;
        }

        //lock origin to parent object position
        lr.SetPosition(0, parentTransform.position);
        
        lr.SetPosition(lr.positionCount - 1, TargetPos);
        
        
        //fix line length
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
                        lr.SetPosition(i, midPoint + normal * (0.5f * segmentLength));
                        lr.SetPosition(i - 1, midPoint - normal * (0.5f * segmentLength));
                    }
                    else
                    {
                        lr.SetPosition(i, midPoint + normal * (1.0f * segmentLength));
                    }
                }
            }
        }
    }
    /* -------- Functions --------*/
    
}
