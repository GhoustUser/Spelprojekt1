using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class TutorialStartScript : MonoBehaviour
{
    public GameObject player;
    private SpriteRenderer spriteRenderer;
    [SerializeField]
    private int glassHitsLimit = 0; 
    private int hitsOnGlass;
    [SerializeField]
    private Animator animator; 

    public GameObject drMarcus;

    private bool hasPressed = false; 
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = player.GetComponent<SpriteRenderer>();
        PlayerMovement.controlEnabled = false;
        PlayerAttack.controlEnabled = false;
        spriteRenderer.enabled = false; 
        TimerManager.pauseTimer = true; 
        animator = drMarcus.GetComponent<Animator>();
        Debug.Log("Start code Tutorialscript has run");
    }

    // Update is called once per frame
    void Update()
    {
        if (hitsOnGlass == 2)
        {
            animator.Play("Idle");
        }
        else if (hitsOnGlass == 3)
        {
            animator.Play("Scared");
        }
        else if (hitsOnGlass == 4 && hasPressed == false)
        {
            animator.Play("ButtonClick");
            TimerManager.pauseTimer = false; 
            hasPressed = true;
        }
        if (Input.GetMouseButtonDown(0))
        {
            GlassHit();
        }
    }

    public void GlassHit()
    {
        hitsOnGlass++; 
        if(hitsOnGlass >= glassHitsLimit)
        {
            player.SetActive(true);
            PlayerMovement.controlEnabled = true;
            PlayerAttack.controlEnabled = true;
            spriteRenderer.enabled = true; 
        }
    }
}
