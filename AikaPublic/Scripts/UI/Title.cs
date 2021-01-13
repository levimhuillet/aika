using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * Controls the functions available at the Title Screen
 */
public class Title : MonoBehaviour
{
    public string nextScene; // the name of the scene after the title screen

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown("space"))
        {
            SceneManager.LoadScene(nextScene);
        }
        if(Input.GetKeyDown("escape"))
        {
            Application.Quit();
        }
    }
}
