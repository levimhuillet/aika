using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Controls the functions available at the Main Menu
 */ 
public class MainMenu : MonoBehaviour
{
    public string nextScene; // the name of the scene after the main menu

    /*
     * Starts a new game
     */ 
    public void NewGame()
    {
        SceneManager.LoadScene(nextScene);
    }

    /*
     * Terminates the program
     */ 
    public void Quit()
    {
        Application.Quit();
    }
}
