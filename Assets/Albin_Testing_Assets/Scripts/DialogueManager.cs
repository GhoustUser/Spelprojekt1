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
        new Dialogue("Title 1", new[] { "line 1", "line 2", "line 3" }),
        new Dialogue("Title 2", new[] { "line 1", "line 2", "line 3" }),
        new Dialogue("Title 3", new[] { "line 1", "line 2", "line 3" }),
    };
}
