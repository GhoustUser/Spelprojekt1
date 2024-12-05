using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public static bool inRange;
    

    public void TriggerDialogue()
    {
        inRange = true;
    }

    public void LeaveRange()
    {
        inRange = false;
    }

    public void Update()
    {
        if (inRange && CanTriggerScript.canTrigger)
        {
            FindObjectOfType<DialogueManager>().StartDialogue(dialogue);
            CanTriggerScript.canTrigger = false;
        }
    }
}