using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public bool startFade;
    
    [SerializeField] 
    private Animator animator;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (startFade)
        {
            animator.Play("StartFade2");
            startFade = false; 
        }
        
    }
}
