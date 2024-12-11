using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UnityEventOnTrigger : MonoBehaviour
{
    [Tooltip("The colliding object that we want to trigger these events with needs to use a tag of the same name as typed in this variable")]
    public string tagToActivate = "Player";
    
    public UnityEvent onTriggerEnter, onTriggerExit;

    private void Awake()
    {
        if ((GetComponent<Collider>() == null) && (GetComponent<Collider2D>() == null))
        {
            Debug.Log($"{gameObject} is missing a collider");
        }
        
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(tagToActivate))
        {
            onTriggerEnter.Invoke();
            Debug.Log("Unity Event Trigger (enter) activated on " + gameObject);
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(tagToActivate))
        {
            onTriggerExit.Invoke();
            Debug.Log("Unity Event Trigger (exit) activated on " + gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(tagToActivate))
        {
            onTriggerEnter.Invoke();
            Debug.Log("Unity Event Trigger (enter) activated on " + gameObject);
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(tagToActivate))
        {
            onTriggerExit.Invoke();
            Debug.Log("Unity Event Trigger (exit) activated on " + gameObject);
        }
    }
}
