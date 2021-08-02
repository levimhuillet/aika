using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using AikaGame.Managers;

namespace AikaGame.Tutorials
{
    /*
     * Attached to a Canvas, this class controls the sequence of
     * tutorial blurbs that appear on levels featuring new interactive objects
     */
    public class Tutorial : MonoBehaviour
    {
        public List<GameObject> panels; // the first panel of this tutorial dialogue sequence
        protected int _panelIndex; // tracks which panel is displayed currently in the sequence
        public bool finishedTutorial { get; set; } // whether a level's tutorial has been completed

        public static Tutorial instance;

        protected void Awake()
        {
            _panelIndex = 0;
            panels[_panelIndex].SetActive(true);
            finishedTutorial = false;

            if (Tutorial.instance == null)
            {
                Tutorial.instance = this;
            }
            else if (Tutorial.instance != this)
            {
                Destroy(this.gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (Tutorial.instance == null)
            {
                Tutorial.instance = this;
            }
            else if (Tutorial.instance != this)
            {
                Destroy(this.gameObject);
            }
        }
    }

}