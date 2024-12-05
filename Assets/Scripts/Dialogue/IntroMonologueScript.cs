using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

// Used brackeys tutorial, Unity manual and Canvas resource on dialogue to write this script. 
public class IntroDialogueScript : MonoBehaviour
{
    [SerializeField] 
    private TextMeshProUGUI text; //Text to write monologue on
    
    private Queue <string> sentences; //Queue of line to use in the monologue.

    [TextArea(3, 1)] //From Brackeys tutorial https://www.youtube.com/watch?v=_nRzoTzeyxU&t=260s
    public string[] dialogueLines;
    
    [SerializeField] 
    private Animator animator;
    
    [SerializeField]
    private float sceneChangeSpeed = 3.0f;

    public static bool changeScene = false;
    
    public void ProgressDialogue() // Function that progresses monologue. 
    {
        if (sentences.Count == 0)
        {
            FadeScreen.startFade = true;
            animator.Play("Eye_Open");
            text.enabled = false;
            changeScene = true;
            //Put scene change here
            return;
            
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    public void FixedUpdate()
    {
        if (changeScene)
            sceneChangeSpeed += Time.deltaTime;
            if (sceneChangeSpeed > 6)
            {
                SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
                changeScene = false;
            }
    }
    
    IEnumerator TypeSentence(string sentence)
    {
        text.text = "";
        foreach (char letter in sentence.ToCharArray())
        {
            text.text += letter;
            yield return null;
        }
    }
    private void Start()
    {
        sentences = new Queue<string>();
        foreach (string sentence in dialogueLines)
        {
            sentences.Enqueue(sentence);
        }
    }
}