using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor.SceneManagement;


//Use this script to control menus, restart and quit game. -JG 
public class GameModeScript : MonoBehaviour
{
    
    //Restart game
    public void RestartGame()
    {
        Application.LoadLevel(0);
    }
    
    //Exit game
    public void QuitGame()
    {
        Application.Quit();
    }
}
