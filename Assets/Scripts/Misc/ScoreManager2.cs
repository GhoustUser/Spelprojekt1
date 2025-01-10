using System.Collections;
using TMPro;
using UnityEngine;

public class ScoreManager2 : MonoBehaviour
{
    /* -------- Settings --------*/
    [SerializeField] private float textAnimationDuration = 2.0f;
    [SerializeField] private float initialDelay = 2.0f;
    [SerializeField] private float delayBetweenScores = 1.0f;
    
    /* -------- Object references --------*/
    private TextMeshProUGUI textObject;
    
    /* -------- Variables --------*/
    public static int enemiesKilled;
    public static int bossEnemiesKilled;
    public static int timeRemaining;
    public static int roomsExplored;
    public static int enemiesEaten;

    private string allScoreText = "";
    
    /* -------- Start --------*/
    void Start()
    {
        //find text component
        textObject = GetComponent<TextMeshProUGUI>();
        
        //set scores
        enemiesKilled = 1;
        bossEnemiesKilled = 2;
        timeRemaining = 3;
        roomsExplored = 4;
        enemiesEaten = 5;
        
        //display scores
        StartCoroutine(DisplayScores());
    }
    
    /* -------- Functions --------*/
    private IEnumerator DisplayScores()
    {
        yield return new WaitForSeconds(initialDelay);
        StartCoroutine(DisplayScore("score from kills: ", enemiesKilled));
        yield return new WaitForSeconds(delayBetweenScores);
        StartCoroutine(DisplayScore("score from boss enemies: ", bossEnemiesKilled));
        yield return new WaitForSeconds(delayBetweenScores);
        StartCoroutine(DisplayScore("score from time remaining: ", timeRemaining));
        yield return new WaitForSeconds(delayBetweenScores);
        StartCoroutine(DisplayScore("score from rooms explored: ", roomsExplored));
        yield return new WaitForSeconds(delayBetweenScores);
        StartCoroutine(DisplayScore("score from enemies eaten: ", enemiesEaten));
    }

    private IEnumerator DisplayScore(string text, int score, bool storeText = true)
    {
        float t = 0f;
        //score increasing animation
        while (t < 1f)
        {
            //increase timer
            t += Time.deltaTime / textAnimationDuration;
            //set displayed text
            textObject.text = $"{allScoreText}{text}{Mathf.RoundToInt(Mathf.Lerp(0, score, t))}";
            yield return null;
        }

        //add this text to the saved text
        if(storeText) allScoreText = textObject.text + '\n';
    }
}
