using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class OnButtonClickScript : MonoBehaviour
{
    public GameObject redButton;
    private Animator animator; 
    [SerializeField]
    private AudioSource audioSourceAttention;
    [SerializeField]
    private AudioSource audioSourceAlarm;

    private bool hasplayed; 
    
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
        audioSourceAlarm.Play();
        hasplayed = true; 
    }
    // Update is called once per frame
    void Update()
    {
        if (audioSourceAlarm.isPlaying == false && hasplayed == true)
        {
            audioSourceAttention.Play();
            hasplayed = false;
        }
    }
}
