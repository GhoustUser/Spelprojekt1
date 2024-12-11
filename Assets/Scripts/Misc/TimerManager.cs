using System;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [SerializeField] public static bool startTimer = true;
    [SerializeField] public static bool pauseTimer;

    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private float startTime = 120;

    public static float timer;

    private void Update()
    {
        // Starts the timer.
        if (startTimer) timer = startTime; 
        startTimer = false;

        // Returns so the player won't die before the timer is started.
        if (timer <= 0) return;

        // If timer isn't paused, reduce it using delta time.
        if (!pauseTimer) timer = Mathf.Max(0, timer - Time.deltaTime);
        // Displays the timer on the screen in 'mm:ss' format.
        timerText.text = TimeSpan.FromSeconds(timer).ToString(@"mm\:ss");

        // Kills the player if the timer reaches 0;
        if (timer == 0) FindFirstObjectByType<Player>().TakeDamage(5);
    }
}
