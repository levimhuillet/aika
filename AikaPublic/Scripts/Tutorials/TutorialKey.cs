using AikaGame.Managers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialKey : MonoBehaviour
{
    private void Update()
    {
        this.GetComponent<Key>().exit = LevelManager.instance.GetExit();
    }
}
