using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    [SerializeField] private Animator animator;
    [SerializeField] private Animator transitionAnimator;
    [SerializeField] private Animator fadeScreenAnimator;
    [SerializeField] private GameObject fadeScreen; 
    [SerializeField] private float sceneChangeSpeed = 3.0f;
    
    public static bool changeScene = false;
    
    public void ProgressDialogue() // Function that progresses monologue. 
    {
        if (sentences.Count == 0)
        {
            Invoke(nameof(FadeScreen), 1);
            fadeScreenAnimator.SetBool("stopFade", false);
            animator.SetBool("eyeOpen", true);
            text.enabled = false;
            changeScene = true;
            return;
            
        }
        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
    }

    private void FadeScreen()
    {
        fadeScreenAnimator.SetBool("fadeScreen", true);
    }

    public void FixedUpdate()
    {
        if (changeScene) sceneChangeSpeed += Time.deltaTime;

        if (sceneChangeSpeed > 6)
        {
            SceneManager.LoadScene("TutorialScene", LoadSceneMode.Single);
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
        fadeScreenAnimator = fadeScreen.GetComponent<Animator>(); 
        transitionAnimator.SetBool("respawn", false);
        transitionAnimator.SetBool("open", true);
        sentences = new Queue<string>();
        foreach (string sentence in dialogueLines)
        {
            sentences.Enqueue(sentence);
        }
    }
}