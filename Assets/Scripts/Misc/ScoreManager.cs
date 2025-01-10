using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour
{
    public static int enemiesKilled;
    public static int bossEnemiesKilled;
    public static int timeRemaining;
    public static int roomsExplored;
    public static int enemiesEaten;

    public static int logsFound;
    public static float timeSpentReading;
    //public static int scientistsKilled;
    public static int powerupsObtained;
    public static int powerupKills;
    //public static int coffeeDrunk;
    public static float averageTimeSpentInRoom;

    public static HashSet<int> roomIds = new HashSet<int>();
    public int displayScore;
    public float displayMultiplier;

    private void Start()
    {
        EndOfGame();
    }

    public static void ResetStats()
    {
        enemiesKilled = 0;
        timeRemaining = 0;
        roomsExplored = 0;
        enemiesEaten = 0;

        logsFound = 0;
        timeSpentReading = 0;
        //scientistsKilled = 0;
        powerupsObtained = 0;
        powerupKills = 0;
        //coffeeDrunk = 0;
        averageTimeSpentInRoom = 0;

        roomIds = new HashSet<int>();
    }

    public void EndOfGame()
    {
        roomsExplored = roomIds.Count;
        if (roomsExplored != 0) averageTimeSpentInRoom = (600 - timeRemaining) / roomsExplored;

        /*print(enemiesKilled);
        print(bossEnemiesKilled);
        print(timeRemaining);
        print(roomsExplored);
        print(enemiesEaten);

        print(logsFound);
        print(timeSpentReading);
        //print(scientistsKilled);
        print(powerupsObtained);
        print(powerupKills);
        //print(coffeeDrunk);
        print(averageTimeSpentInRoom);*/

        enemiesKilled = 15;
        bossEnemiesKilled = 3;
        roomsExplored = 8;
        timeRemaining = 500;
        enemiesEaten = 10;

        StartCoroutine(CalculateScore());
    }

    private IEnumerator aaaa(float countTime, float startPos, float endPos)
    {
        float countScore = 0;
        while (countScore < countTime)
        {
            float d = (countScore / countTime);
            displayScore = Mathf.RoundToInt(startPos * (1 - d) + endPos * d);
            yield return null;
            countScore += Time.deltaTime;
        }
    }

    private IEnumerator bbbb(float countTime, float startPos, float endPos)
    {
        float countScore = 0;
        while (countScore < countTime)
        {
            float d = (countScore / countTime);
            displayMultiplier = Mathf.RoundToInt(startPos * (1 - d) + endPos * d);
            yield return null;
            countScore += Time.deltaTime;
        }
    }

    private IEnumerator CalculateScore()
    {
        int score = 0;
        displayScore = 0;

        float countTime = timeRemaining / 100;

        StartCoroutine(aaaa(countTime, 0, timeRemaining));
        displayScore = timeRemaining;
        score = displayScore;
        yield return new WaitForSeconds(1);

        countTime = enemiesKilled / 5;
        displayMultiplier = 0;

        StartCoroutine(bbbb(countTime, 0, (float)Math.Round(1 + (0.25f * enemiesKilled))));
        displayMultiplier = (float) Math.Round(1 + (0.25f * enemiesKilled));
        yield return new WaitForSeconds(0.25f);

        countTime = bossEnemiesKilled / 2;

        StartCoroutine(bbbb(countTime, displayMultiplier, displayMultiplier + (bossEnemiesKilled * 0.5f)));
        displayMultiplier = displayMultiplier + (bossEnemiesKilled * 0.5f);

        countTime = displayMultiplier / 2;

        StartCoroutine(aaaa(countTime, score, Mathf.RoundToInt(score * displayMultiplier)));
        displayScore = Mathf.RoundToInt(score * displayMultiplier);
        score = displayScore;

        yield return new WaitForSeconds(1f);

        countTime = roomsExplored / 3;
        displayMultiplier = 0;

        StartCoroutine(bbbb(countTime, 0, (float)Math.Round((1 + (0.1f * roomsExplored)), 2)));
        displayMultiplier = (float)Math.Round((1 + (0.1f * roomsExplored)), 2);
        yield return new WaitForSeconds(0.25f);

        countTime = displayMultiplier / 2;

        StartCoroutine(aaaa(countTime, 0, Mathf.RoundToInt(score * displayMultiplier)));
        displayScore = Mathf.RoundToInt(score * displayMultiplier);
        score = displayScore;

        yield return new WaitForSeconds(1f);

        countTime = enemiesEaten / 5;
        displayMultiplier = 0;

        StartCoroutine(bbbb(countTime, 0, (float)Math.Round((1 + (0.2f * enemiesEaten)))));
        displayMultiplier = (float)Math.Round((1 + (0.2f * enemiesEaten)));
        yield return new WaitForSeconds(0.25f);

        countTime = displayMultiplier / 2;

        StartCoroutine(aaaa(countTime, 0, Mathf.RoundToInt(score * displayMultiplier)));
        displayScore = Mathf.RoundToInt(score * displayMultiplier);
        score = displayScore;

        yield return new WaitForSeconds(1f);

        print(score);

        score = (int)Mathf.Round(timeRemaining * (1 + (0.25f * (enemiesKilled + (bossEnemiesKilled * 2)))) * (1 + 0.1f * roomsExplored) * (1 + 0.2f * enemiesEaten));
        print(score);
    }
}
