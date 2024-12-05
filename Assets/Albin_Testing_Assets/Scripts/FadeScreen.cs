using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public static bool startFade;
    
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
            print("This code is running ");
            animator.Play("FadeScreen");
            startFade = false; 
        }
        
    }
}
