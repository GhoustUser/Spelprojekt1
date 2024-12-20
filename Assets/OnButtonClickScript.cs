using UnityEngine;

public class OnButtonClickScript : MonoBehaviour
{
    [SerializeField] private GameObject redButton;
    [SerializeField] private AudioSource audioSourceAttention;
    [SerializeField] private AudioSource audioSourceAlarm;

    private Animator animator;
    private bool hasplayed; 
    
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

    void Update()
    {
        if (audioSourceAlarm.isPlaying == false && hasplayed == true)
        {
            audioSourceAttention.Play();
            hasplayed = false;
        }
    }
}
