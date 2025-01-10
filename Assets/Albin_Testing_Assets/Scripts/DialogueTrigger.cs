using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class DialogueTrigger : MonoBehaviour
{
    public Dialogue dialogue;
    public bool inRange;
    public bool isTutorial;
    public bool isInitialized = false;

    private DialogueManager manager; 
    public void Start()
    {
        manager = FindObjectOfType<DialogueManager>();
    }
    public void TriggerDialogue()
    {
        return;
    }

    public void PlayerInRange()
    {
        inRange = true;
    }
    
    public void LeaveRange()
    {
        inRange = false;
        manager.EndDialogue();
    }
    
    public void Update()
    {
        
        
        print(manager.dialogueIsActive);
        if (Input.GetKeyDown(KeyCode.E) && !manager.dialogueIsActive && inRange)
        {
            manager.StartDialogue(this, isTutorial);
        }
        else if (Input.GetKeyDown(KeyCode.E) && manager.dialogueIsActive)
        {
            manager.EndDialogue();
        }
    }
}