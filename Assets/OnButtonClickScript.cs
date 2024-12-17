using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OnButtonClickScript : MonoBehaviour
{
    public GameObject redButton;
    private Animator animator; 
    [SerializeField]
    private AudioSource audioSource;
    
    // Start is called before the first frame update
    void Start()
    {
        animator = redButton.GetComponent<Animator>();
        
    }

    public void OnClick()
    {
        animator.Play("Pressed");
    }

    public void PlayAudio()
    {
        audioSource.Play();
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
