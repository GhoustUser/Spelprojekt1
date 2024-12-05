using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;   

public class DialogueManager : MonoBehaviour
{
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI dialogueText;
    
    public Animator animator;
    private Queue<string> sentences;
    
    // Start is called before the first frame update
    void Start()
    {
        sentences = new Queue<string>();
    }

    public void StartDialogue(Dialogue dialogue)
    {
        animator.SetBool("IsOpen", true);
        
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
    
    void EndDialogue()
    {
        animator.SetBool("IsOpen", false);
    }

    public readonly Dialogue[] Dialogues = new[]
    {
        new Dialogue("Log 237 by Dr Marcus", new[] { "Subject is dormant, it seems as though Morgans procedures has had no effect on the state of sentience in the Musculus."}),
        new Dialogue("Log 238 by Dr Marcus", new[] { "Musculus is showing signs of voluntary movement, muscle contractions are visibly taking place in the anterior and posterior orbits." }),
        new Dialogue("Log 239 by Dr Marcus", new[] { "Extracted sample, weight 0.988 grams, has agency even after separation from the main body." }),
    };
}
