using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDialogueActivator : MonoBehaviour
{
    private DialogueTrigger trigger;

    private DialogueManager manager; 
    // Start is called before the first frame update
    void Start()
    {
        trigger = FindObjectOfType<DialogueTrigger>();
    }

    
    // Update is called once per frame
    void Update()
    {
        
    }
}
