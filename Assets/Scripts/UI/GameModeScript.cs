using UnityEngine;
using UnityEngine.SceneManagement;

//Use this script to control menus, restart and quit game. -JG 
public class GameModeScript : MonoBehaviour
{
    public GameObject PauseMenuPanel;
    public GameObject HowToPanel;
    public static bool gameIsPaused;
    

    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!gameIsPaused) PauseGame();
            else BackToGame();
        }

        /*if (Input.GetKeyDown(KeyCode.Y))
        {
            HowToPanel.SetActive(false);
        }*/

        /*if (Input.GetKeyDown(KeyCode.T))
        {
            Debug.Log("T was pressed");
            PlayerMovement.controlEnabled = false;
            PlayerAttack.controlEnabled = false;
        }
        */
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Intro_Scene");
    }

    public void SkipTutorial()
    {
        SceneManager.LoadScene("MainScene");
    }
    
    public void FullscreenToggle()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }
    //Pause and unpause game.
    //SOURCE: https: //gamedevbeginner.com/the-right-way-to-pause-the-game-in-unity/
    public void PauseGame()
    {
        PauseMenuPanel.SetActive(true);
        gameIsPaused = true;
        Time.timeScale = 0f;
        PlayerMovement.controlEnabled = false;
        PlayerAttack.controlEnabled = false;
    }

    //Deactivated menu panel and resets time to 1. 
    public void BackToGame()
    {
        PauseMenuPanel.SetActive(false);
        gameIsPaused = false;
        Time.timeScale = 1;
        PlayerMovement.controlEnabled = true;
        PlayerAttack.controlEnabled = true;
    }

    //Restarts game and resets time to 1. 
    /*public void RestartGame()
    {
        gameIsPaused = !gameIsPaused;
        PauseGame();
        SceneManager.LoadScene("Intro_Scene");
    }*/
    
    //Exit game
    public void QuitGame()
    {
        Application.Quit();
    }

}
