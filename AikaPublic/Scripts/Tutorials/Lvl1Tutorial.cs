using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Tutorials;
using AikaGame.Managers;
using AikaGame.Entries;

public class Lvl1Tutorial : Tutorial
{
    bool _reloaded;
    public GameObject keyPrefab;
    GameObject tutorialKey;

    new private void Awake()
    {
        if (GameStateManager.instance.reloadedLevel)
        {
            tutorialKey = Instantiate(keyPrefab);
            tutorialKey.transform.position = new Vector2(10f, -8f);
            tutorialKey.GetComponent<Key>().exit = LevelManager.instance.GetExit();
        }
        base.Awake();
    }

    private void Start()
    {
        LevelManager.ExitSequence.AddListener(ListenReload);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            if (!finishedTutorial)
            {
                panels[_panelIndex].SetActive(false);
            }
        }
    }

    public void ListenReload()
    {
        if (!_reloaded)
        {
            panels[_panelIndex].SetActive(false);
            ++_panelIndex;
            panels[_panelIndex].SetActive(true);
            _reloaded = true;
        }
    }
}
