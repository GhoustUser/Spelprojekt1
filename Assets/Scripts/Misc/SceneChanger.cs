using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events; 
public class SceneChanger : MonoBehaviour
{
    [SerializeField] 
    private GameObject fadeScreen;

    [SerializeField] 
    private float fadeSpeed = 3.0f; 
    private Animator fadeScreenAnimator;
    [SerializeField]
    UnityEvent sceneChangeEvent;
    
    [SerializeField] 
    private string goingToScene;
    
    [SerializeField]
    private float transitionDuration;

    public bool doTransition;
    // Start is called before the first frame update
    void Start()
    {
        fadeScreenAnimator = fadeScreen.GetComponent<Animator>(); 
        fadeScreenAnimator.Play("NoFade");
    }

    public void DoTransition()
    {
        doTransition = true;
        PlayerMovement.controlEnabled = false; 
        PlayerAttack.controlEnabled = false;
        fadeScreenAnimator.Play("FadeScreen");
        fadeScreenAnimator.speed = fadeSpeed;
    }
   
    // Update is called once per frame
    void Update()
    {
        if (doTransition)
        {
            transitionDuration -= Time.deltaTime;
            if (transitionDuration < 0)
            {
                PlayerMovement.controlEnabled = true; 
                PlayerAttack.controlEnabled = true;
                SceneManager.LoadScene(goingToScene);
                doTransition = false;
            }
        }
        
    }
}
