﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public string nextScene;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        /*
        if (Input.GetKeyDown("space"))
        {
            SceneManager.LoadScene(nextScene);
        }
        */
    }

    public void NewGame()
    {
        SceneManager.LoadScene(nextScene);
    }

    public void Quit()
    {
        Application.Quit();
    }
}
