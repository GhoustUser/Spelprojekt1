using UnityEngine;

public class Elevator : MonoBehaviour
{
    private Animator transitionAnimator;

    [SerializeField] private bool tutorialScene;

    void Start()
    {
        transitionAnimator = GameObject.FindGameObjectWithTag("ElevatorTransition").GetComponent<Animator>();

        if (!tutorialScene) transitionAnimator.Play("ElevatorOpens");
    }

    public void StartTransition()
    {
        transitionAnimator.Play("ElevatorTransition");
        ScoreManager.timeRemaining = Mathf.RoundToInt(TimerManager.timer);
    }
}
