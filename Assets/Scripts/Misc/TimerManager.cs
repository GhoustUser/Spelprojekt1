using System;
using TMPro;
using UnityEngine;

public class TimerManager : MonoBehaviour
{
    [SerializeField] private bool startTimer;
    [SerializeField] private bool pauseTimer;
    [SerializeField] private bool showTimer;

    [SerializeField] private TextMeshProUGUI timerText;

    public static float timer;

    private void Update()
    {
        if (startTimer) timer = 60 * 8; startTimer = false;

        if (!pauseTimer) timer = Mathf.Max(0, timer - Time.deltaTime);
        timerText.text = TimeSpan.FromSeconds(timer).ToString(@"mm\:ss");

        timerText.gameObject.SetActive(showTimer);
    }
}
