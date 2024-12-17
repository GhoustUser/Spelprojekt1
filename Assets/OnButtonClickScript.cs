using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OnButtonClickScript : MonoBehaviour
{
    public GameObject redButton;
    private Animator animator; 
    
    // Start is called before the first frame update
    void Start()
    {
        animator = redButton.GetComponent<Animator>();
        
    }

    public void OnClick()
    {
        animator.Play("Pressed");
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
