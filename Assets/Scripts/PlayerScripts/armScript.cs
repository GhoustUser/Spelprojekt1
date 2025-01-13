using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

public class armScript : MonoBehaviour
{
    /* -------- Settings --------*/
    [HideInInspector]public float totalLength = 1.8f;
    [HideInInspector]public float lengthRandomness = 0f;
    [HideInInspector]public float momentumFactor = 0.9f;
    [HideInInspector]public int attempts = 5;
    [HideInInspector]public float angleRange = 90.0f;
    [HideInInspector]public float width = 0.1f;
    [HideInInspector]public float stretching = 0.5f;
    
    [HideInInspector]public Material armMaterial;
    
    /* -------- Variables --------*/
    private Transform parentTransform;
    private Vector3 prevParentPosition;
    private Vector3 parentMovement;
    private Vector3 targetOrigin;
    private Vector3 targetPos;
    private float randomValue;
    
    private LineRenderer lr;
    private ArmManager am;
    

    private const int SegmentCount = 20;

    private Vector3[] prevPositions;

    private float segmentLength = 1.0f;

    /* -------- Properties --------*/
    private Vector3 StartPos => lr.GetPosition(0);
    private Vector3 EndPos => lr.GetPosition(lr.positionCount - 1);

    private float Length => totalLength * (1 + (lengthRandomness - 0.5f) * randomValue);

    /* -------- Start --------*/
    void Start()
    {
        //get a random value
        randomValue = Random.value;
        
        //line renderer settings
        am = GetComponentInParent<ArmManager>();
        lr = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.positionCount = SegmentCount;
        lr.material = armMaterial;
        lr.sortingOrder = 1;
        
        prevPositions = new Vector3[SegmentCount];
        parentTransform = transform.parent.parent;

        segmentLength = Length / SegmentCount;
        for (uint i = 0; i < SegmentCount; i++) prevPositions[i] = parentTransform.position + new Vector3(0, -i * segmentLength, 0);
        lr.SetPositions(prevPositions);
    }

    /* -------- Update --------*/
    void Update()
    {
        //get parent movement
        Vector3 parentPosition = parentTransform.position;
        parentMovement = parentPosition - prevParentPosition;
        prevParentPosition = parentPosition;
        
        //calculate arm target position
        targetOrigin = parentPosition + parentMovement.normalized * Length;
        if (Vector3.Distance(EndPos, targetOrigin) > Length * 1.8f)
        {
            Vector2 dir = parentMovement.normalized;
            dir = dir.Rotate(Random.Range(-angleRange * 0.5f, angleRange * 0.5f));
            targetPos = parentTransform.position + new Vector3(dir.x, dir.y, 0) * Length;

            RaycastHit2D hit = Physics2D.Linecast(parentPosition, targetPos, am.wallLayer);
            if (hit.point != Vector2.zero) targetPos = hit.point;
        }

        //momentum
        for (int i = 1; i < lr.positionCount && momentumFactor > 0.1f; i++)
        {
            Vector3 pos = lr.GetPosition(i);
            Vector3 positionBeforeUpdate = pos;
            pos += (pos - prevPositions[i]) * momentumFactor;
            lr.SetPosition(i, pos);
            prevPositions[i] = positionBeforeUpdate;
        }

        //lock origin to parent object position
        lr.SetPosition(0, parentTransform.position);
        
        lr.SetPosition(lr.positionCount - 1, targetPos);
        
        float currentLength = Vector2.Distance(StartPos, EndPos);
        
        //fix line length
        for (uint a = 0; a < attempts; a++)
        {
            for (int i = 1; i < lr.positionCount; i++)
            {
                Vector3 pos = lr.GetPosition(i);
                Vector3 prevPos = lr.GetPosition(i - 1);
                float distance = Vector3.Distance(pos, prevPos);
                
                //prevent middle segment from going beyond end segment
                if (Vector2.Distance(pos, StartPos) > currentLength)
                {
                    pos = prevPos;
                    lr.SetPosition(i, pos);
                }
                
                //adjust distance between segments
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
        
        //change with of arm based on length
        float lengthRatio = 1f - ((Vector2.Distance(StartPos, EndPos) + 0.001f) / Length) * stretching;
        lr.startWidth = width;
        lr.endWidth = width * lengthRatio;
    }
    /* -------- Functions --------*/
    public void DeleteArm()
    {
        Destroy(gameObject);
    }

    public void OverrideTarget(Vector2 position)
    {
        targetPos = position;
        targetOrigin = position;
    }
}
