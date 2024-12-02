using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


//Use this script to control menus, restart and quit game. -JG 
public class GameModeScript : MonoBehaviour
{
    public GameObject PauseMenuPanel;


    public void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            PauseMenuPanel.SetActive(!PauseMenuPanel.activeInHierarchy);
        }
    }
    //Restart game
    public void RestartGame()
    {
        SceneManager.LoadScene(0);
    }
    
    //Exit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
