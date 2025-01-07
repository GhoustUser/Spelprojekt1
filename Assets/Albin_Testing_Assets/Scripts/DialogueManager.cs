using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   

public class DialogueManager : MonoBehaviour
{
    public bool tutorial;
    
    private TextMeshProUGUI nameText;
    public TextMeshProUGUI nameTextTutorial;
    public TextMeshProUGUI nameTextNormal;
    
    private TextMeshProUGUI dialogueText;
    public TextMeshProUGUI dialogueTextTutorial;
    public TextMeshProUGUI dialogueTextNormal;
    
    private Animator animator;
    public Animator animatorTutorial;
    public Animator animatorNormal;
    
    private Queue<string> sentences;
    
    void Start()
    {
        sentences = new Queue<string>();

        if (tutorial)
        {
            animator = animatorTutorial;
            nameText = nameTextTutorial;
            dialogueText = dialogueTextTutorial;
        }
        else
        {
            animator = animatorNormal;
            nameText = nameTextNormal;
            dialogueText = dialogueTextNormal;
        }
    }

    public void StartDialogue(Dialogue dialogue, bool isTutorial)
    {
        //print(isTutorial);
        
        
        if (isTutorial)
        {
            animator = animatorTutorial;
            nameText = nameTextTutorial;
            dialogueText = dialogueTextTutorial; 
        }
        else
        {
            animator = animatorNormal;
            nameText = nameTextNormal;
            dialogueText = dialogueTextNormal; 
        }
        
        animator.SetBool("IsOpen", true);
        TimerManager.pauseTimer = true; 
        Hunger.pauseDecay = true;
        nameText.text = dialogue.name;
        
        sentences.Clear();
      
        foreach (string sentence in dialogue.sentences)
        {
            sentences.Enqueue(sentence);
        }

        DisplayNextSentence();
    }
    public void DisplayNextSentence()
    {
        //print(animator);
        if (sentences.Count == 0)
        {
            EndDialogue();
            CanTriggerScript.canTrigger = false; 
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            dialogueText.text += letter;
            yield return null;
        }
    }
    
    public void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
        TimerManager.pauseTimer = false; 
        Hunger.pauseDecay = false; 
    }

    public readonly Dialogue[] Dialogues = new[]
    {
        new Dialogue("Log 1, Dr Marcus", new[] { "Project Sanguinus has been initiated. I finally get a chance to work with Professor Morgon and build a bioweapon of peerless power. "}),
        new Dialogue("Log 12, Dr Marcus", new[] { " Iteration 24, Mitosis is active and thriving, subjects weight is growing exponentially."}),
        new Dialogue("Log 14, Dr Marcus", new[] { "Log 14, The structural placement of the eyes, mouth and teeth seems to be random and does not resemble human anatomical structure. It shares more similarity to a sphere of meat."}),
        new Dialogue("Log 20, Dr Marcus", new[] { "Subjects weight has reached 784 pounds and has now stopped, spots where eyes, mouths and teeth have developed are now clearly defined."}),
        new Dialogue("Log 24, Dr Marcus", new[] { "Dr Erik suggested that we clean up the cups of coffee all over the place, I say he is crazy."}),
        new Dialogue("Log 32, Dr Marcus", new[] { "Subject is dormant, it seems as though Morgans procedures has had no effect on the state of sentience in the Musculus."}),
        new Dialogue("Log 34, Dr Marcus", new[] { "Musculus is showing signs of voluntary movement, muscle contractions are visibly taking place in the anterior and posterior orbits." }),
        new Dialogue("Log 40, Dr Marcus", new[] { "Extracted sample, weight 0.988 grams, has agency even after separation from the main body." }),
    };
}
