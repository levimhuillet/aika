using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Tutorials;
using AikaGame.Managers;

public class SynchronousTutorial : Tutorial
{
    public bool pauseOnPlay;

    private void Start()
    {
        if (!finishedTutorial)
        {
            if (pauseOnPlay)
            {
                LevelManager.Pause.Invoke();
                // ++MenuManager.numMenusOpen;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!finishedTutorial)
            {
                panels[_panelIndex].SetActive(false);

                ++_panelIndex;
                //Case when there are still more panels to display
                if (_panelIndex < panels.Count)
                {
                    panels[_panelIndex].SetActive(true);
                }
                else
                {
                    finishedTutorial = true;

                    if (pauseOnPlay)
                    {
                        //--MenuManager.numMenusOpen;
                        if (MenuManager.numMenusOpen <= 0)
                        {
                            // unpause level
                            LevelManager.Unpause.Invoke();
                        }
                    }
                }
            }
        }
    }
}
