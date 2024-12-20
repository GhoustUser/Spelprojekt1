using LevelGen;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using static Default.Default;

public class SceneChanger : MonoBehaviour
{
    private GameObject fadeScreen;

    [SerializeField] 
    private float fadeSpeed = 1.0f; 
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
        fadeScreenAnimator = GameObject.FindGameObjectWithTag("FadeScreen").GetComponent<Animator>();
    }

    public void DoTransition()
    {
        doTransition = true;
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("MusicPlayer"))
        {
            go.GetComponent<AudioFade>().StopAllCoroutines();
            StartCoroutine(FadeOut(go.GetComponent<AudioSource>(), 2));
        }
        PlayerMovement.controlEnabled = false; 
        PlayerAttack.controlEnabled = false;
        fadeScreenAnimator.SetBool("fadeScreen", true);
        fadeScreenAnimator.SetBool("stopFade", false);
        fadeScreenAnimator.speed = fadeSpeed;

        LevelMap.ClearListeners();
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
                TimerManager.timer = 240;
                Generator.isDestroyed = false;
                SceneManager.LoadScene(goingToScene);
                doTransition = false;
            }
        }
        
    }
}
