using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanTriggerScript : MonoBehaviour
{
    public static bool canTrigger;
    
    public static void EnableTrigger()
    {
        canTrigger = true;
        /*
        if (DialogueTrigger.inRange)
        {
            canTrigger = true;
        }
        */
    }

    
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        //print(canTrigger);
    }
}
