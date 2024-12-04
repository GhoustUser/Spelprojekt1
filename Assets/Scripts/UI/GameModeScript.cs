using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//Use this script to control menus, restart and quit game. -JG 
public class GameModeScript : MonoBehaviour
{
    public GameObject PauseMenuPanel;
    public static bool gameIsPaused;
    


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            //Makes the pause menu panel appear
            PauseMenuPanel.SetActive(!PauseMenuPanel.activeInHierarchy);
            //Pause and unpause
            gameIsPaused = !gameIsPaused;
            PauseGame();
        }

        if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T was pressed");
            PlayerMovement.controlEnabled = false;
            PlayerAttack.controlEnabled = false;
        }

    }

    //Pause and unpause game.
    //SOURCE: https: //gamedevbeginner.com/the-right-way-to-pause-the-game-in-unity/
    public void PauseGame()
    {
        if (gameIsPaused)
        {
            Time.timeScale = 0f;
            PlayerMovement.controlEnabled = false;
            PlayerAttack.controlEnabled = false;
        }
        else
        {
            Time.timeScale = 1;
            PlayerMovement.controlEnabled = true;
            PlayerAttack.controlEnabled = true;
        }
    }

    //Deactivated menu panel and resets time to 1. 
    public void BackToGame()
    {
        PauseMenuPanel.SetActive(!PauseMenuPanel.activeInHierarchy);
        gameIsPaused = !gameIsPaused;
        PauseGame();
    }

    //Restarts game and resets time to 1. 
    public void RestartGame()
    {
        gameIsPaused = !gameIsPaused;
        PauseGame();
        SceneManager.LoadScene(0);
    }
    
    //Exit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
