using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Managers;

namespace AikaGame.Functionalities
{
    public class Pausable : MonoBehaviour
    {
        public bool paused { get; set; }

        public void Awake()
        {
            paused = false;
            LevelManager.TogglePause.AddListener(this.TogglePause);
            LevelManager.Pause.AddListener(this.Pause);
            LevelManager.Unpause.AddListener(this.Unpause);
        }

        public void TogglePause()
        {
            paused = !paused;
        }

        public void Pause()
        {
            paused = true;
        }

        public void Unpause()
        {
            paused = false;
        }
    }
}