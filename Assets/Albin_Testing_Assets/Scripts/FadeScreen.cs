using UnityEngine;

public class FadeScreen : MonoBehaviour
{
    public bool startFade;
    public bool dontFade;
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
            animator.SetBool("fadeScreen", true);
            animator.SetBool("stopFade", false);
            startFade = false; 
        }
        else if (dontFade)
        {
            animator.SetBool("stopFade", true);
            animator.SetBool("fadeScreen", false);
            dontFade = false; 
        }
        
    }
}
