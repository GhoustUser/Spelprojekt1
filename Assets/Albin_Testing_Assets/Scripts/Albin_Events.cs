using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.Events;

public class Albin_Events : MonoBehaviour
{
    // Start is called before the first frame update
    
    public string tagCondition = "Player";
    
    public UnityEvent onAreaCollision, onAreaNoCollision;
    
    void Start()
    {
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagCondition))
        {
            onAreaCollision.Invoke();
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(tagCondition))
        {
            onAreaNoCollision.Invoke();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
