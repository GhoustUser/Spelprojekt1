using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public bool inRange;
    

    public void TriggerDialogue()
    {
        inRange = true;
        CanTriggerScript.canTrigger = false;
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