using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events; 
public class SceneChanger : MonoBehaviour
{
    
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
        
    }

    public void DoTransition()
    {
        doTransition = true;
        PlayerMovement.controlEnabled = false; 
        PlayerAttack.controlEnabled = false;
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

    private void OnCollisionEnter(Collision other)
    {
        
        Invoke("DoTransition", transitionDuration);
    }
}
