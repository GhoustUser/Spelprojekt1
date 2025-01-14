using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum Grade
{
    S, A, B, C, D, F
}

public class ScoreManager : MonoBehaviour
{
    public static int enemiesKilled;
    public static int bossEnemiesKilled;
    public static int timeRemaining;
    public static int roomsExplored;
    public static int enemiesEaten;

    public static int logsFound;
    public static float timeSpentReading;
    public static int scientistsKilled;
    public static int powerupsObtained;
    public static int powerupKills;
    public static float coffeeConsumed;
    public static float averageTimeSpentInRoom;

    public static HashSet<int> roomIds = new HashSet<int>();

    /* -------- Settings --------*/
    [SerializeField] private float textAnimationDuration = 2.0f;
    [SerializeField] private float initialDelay = 2.0f;
    [SerializeField] private float delayBetweenScores = 1.0f;
    [SerializeField] private Image fade;
    [SerializeField] private Image gradeObject;
    [SerializeField] private Animator gradeAnimator;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject dustParticles;

    [SerializeField] private List<Sprite> gradeSprites;

    /* -------- Object references --------*/
    private TextMeshProUGUI textObject;

    /* -------- Variables --------*/

    private string allScoreText = "";
    private int totalScore;
    private Grade grade;
    private static readonly Dictionary<int, Grade> scoreLevels = new Dictionary<int, Grade>()
    {
        {22250, Grade.S },
        {15000, Grade.A },
        {10000, Grade.B },
        {5000, Grade.C },
        {0, Grade.D },
        //{-1, Grade.F }
    };

    /* -------- Start --------*/
    void Start()
    {
        //find text component
        textObject = GetComponent<TextMeshProUGUI>();
        roomsExplored = roomIds.Count;
        totalScore = (int)Mathf.Round(10 * (timeRemaining / 30 * (1 + (0.25f * (enemiesKilled + (bossEnemiesKilled * 2)))) * (1 + 0.1f * roomsExplored) * (1 + 0.1f * enemiesEaten)));

        foreach (KeyValuePair<int, Grade> kvp in scoreLevels)
        {
            if (kvp.Key <= totalScore)
            {
                grade = kvp.Value;
                break;
            }
        }

        gradeObject.sprite = grade switch
        {
            Grade.S => gradeSprites[0],
            Grade.A => gradeSprites[1],
            Grade.B => gradeSprites[2],
            Grade.C => gradeSprites[3],
            Grade.D => gradeSprites[4],
            Grade.F => gradeSprites[5],
            _ => throw new System.NotImplementedException(),
        };

        //display scores
        StartCoroutine(DisplayScores());
    }

    /* -------- Functions --------*/
    private IEnumerator DisplayScores()
    {
        yield return new WaitForSeconds(initialDelay);
        float fadeCounter = 0;
        float fadeTime = 1;
        while (fadeCounter < fadeTime)
        {
            fade.color = (fadeCounter / fadeTime) * (Color.black * 0.75f);
            yield return null;
            fadeCounter += Time.deltaTime;
        }
        StartCoroutine(DisplayScore("Time Remaining: ", timeRemaining));
        yield return new WaitForSeconds(delayBetweenScores);
        StartCoroutine(DisplayScore("Enemies Killed: ", enemiesKilled));
        yield return new WaitForSeconds(delayBetweenScores);
        StartCoroutine(DisplayScore("- Boss Enemies Killed: ", bossEnemiesKilled));
        yield return new WaitForSeconds(delayBetweenScores);
        StartCoroutine(DisplayScore("Enemies Eaten: ", enemiesEaten));
        yield return new WaitForSeconds(delayBetweenScores);
        StartCoroutine(DisplayScore("Coffee consumed: ", coffeeConsumed, endText: "L"));
        yield return new WaitForSeconds(delayBetweenScores + 1);
        StartCoroutine(DisplayScore("\nTotal Score: ", totalScore));
        yield return new WaitForSeconds(delayBetweenScores);
        gradeObject.gameObject.SetActive(true);
        yield return new WaitForSeconds(0.5f);
        GameObject particles = Instantiate(dustParticles, gradeObject.transform.position + Vector3.down * 0.75f, Quaternion.identity);
        foreach (ParticleSystem ps in particles.GetComponentsInChildren<ParticleSystem>())
        {
            ps.Play();
        }
        CameraShake.ShakeCamera(0.5f, 1, 1);
        yield return new WaitForSeconds(1.5f);
        if (grade == Grade.S) gradeAnimator.Play("Score_S");
        yield return new WaitForSeconds(1f);
        pauseMenu.SetActive(true);
    }

    private IEnumerator DisplayScore(string text, float score, bool storeText = true, string endText = "")
    {
        float t = 0f;
        textObject.text = $"{allScoreText}{text}0{endText}";
        CameraShake.ShakeCamera(0.25f, 0.25f, 1);
        yield return new WaitForSeconds(.25f);

        //score increasing animation
        while (t < 1f)
        {
            //increase timer
            t += Time.deltaTime / textAnimationDuration;
            //set displayed text
            textObject.text = $"{allScoreText}{text}{Mathf.RoundToInt(Mathf.Lerp(0, score, t))}{endText}";
            yield return null;
        }
        textObject.text = $"{allScoreText}{text}{score}{endText}";

        //add this text to the saved text
        if (storeText) allScoreText = textObject.text + '\n';
    }

    public static void ResetStats()
    {
    enemiesKilled = 0;
    bossEnemiesKilled = 0;
    timeRemaining = 0;
    roomsExplored = 0;
    enemiesEaten = 0;

    logsFound = 0;
    timeSpentReading = 0;
    scientistsKilled = 0;
    powerupsObtained = 0;
    powerupKills = 0;
    coffeeConsumed = 0;
    averageTimeSpentInRoom = 0;

    roomIds = new HashSet<int>();
}
}
