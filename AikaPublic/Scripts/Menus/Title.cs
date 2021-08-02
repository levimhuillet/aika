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

    void Start()
    {
        // Switch to 1080 x 720 windowed
        Screen.SetResolution(1280, 720, false);
    }

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
