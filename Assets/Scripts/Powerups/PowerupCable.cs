using System.Collections;
using System.Collections.Generic;
using LevelGen;
using UnityEngine;

public class PowerupCable : MonoBehaviour
{
    /* -------- Variables --------*/
    private LineRenderer lr;
    
    
    /* -------- Initialize --------*/
    public void Initialize(Powerup powerup1, Powerup powerup2, Material cableMaterial)
    {
        //initialize line renderer
        lr = gameObject.AddComponent(typeof(LineRenderer)) as LineRenderer;
        lr.startWidth = 0.1f;
        lr.endWidth = 0.1f;
        lr.material = cableMaterial;
        
        //subscribe to event OnPowerupDestroyed
        powerup1.OnPowerupDestroyed += DestroyPowerupCable;
        powerup2.OnPowerupDestroyed += DestroyPowerupCable;
        LevelMap.OnLevelUnloaded += DestroyPowerupCable;
        
        //create line
        lr.positionCount = 2;
        lr.SetPosition(0, powerup1.transform.position + Vector3.down * 0.5f);
        lr.SetPosition(1, powerup2.transform.position + Vector3.down * 0.5f);
    }

    private void DestroyPowerupCable()
    {
        Destroy(gameObject);
    }
}
