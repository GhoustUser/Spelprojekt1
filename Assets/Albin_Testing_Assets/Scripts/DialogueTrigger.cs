using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public bool inRange;
    public bool isTutorial;
    public bool isInitialized = false;

    public DialogueManager manager; 
    public void TriggerDialogue()
    {
        if (manager.dialogueActive)
        {
            manager.StartDialogue(this, isTutorial);
        }
        else
        {
            CanTriggerScript.canTrigger = false; 
        }
     
    }

    public void PlayerInRange()
    {
        inRange = true;
    }
    
    public void LeaveRange()
    {
        inRange = false;
    }



    public void Start()
    {
        manager = GetComponent<DialogueManager>();
    }
    public void Update()
    {
        print(inRange);
        if (inRange && CanTriggerScript.canTrigger)
        {
            FindObjectOfType<DialogueManager>().StartDialogue(this, isTutorial);
        }
    }
}