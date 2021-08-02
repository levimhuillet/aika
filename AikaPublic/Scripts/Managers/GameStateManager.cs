using AikaGame.GameData;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AikaGame.Managers
{
    /*
     * Tracks which levels have been completed
     */
    public class GameStateManager : MonoBehaviour
    {
        public static GameStateManager instance;

        private int _highestLevelCompleted; // only one int is needed since players cannot skip levels. If the player could skip levels, a separate bool would be needed for each level
        public bool reloadedLevel;

        /*
         * Unity Callback
         */
        void Awake()
        {
            if (instance == null)
            {
                instance = this;
                DontDestroyOnLoad(this.gameObject);
                _highestLevelCompleted = 0;
                LevelManager.ExitSequence.AddListener(ReloadedLevel);
                LevelManager.Restart.AddListener(RefreshLevel);
            }
            else if (this != instance)
            {
                Destroy(this.gameObject);
            }
        }

        /*
         * 
         */
        public void CompleteLevel(int levelNum)
        {
            // Case when the completed level is higher than the highest level completed
            if (levelNum > _highestLevelCompleted)
            {
                // Highest level must be updated to track the new highest level
                _highestLevelCompleted = levelNum;
            }
            RefreshLevel();
        }

        /*
         * 
         */
        public int GetHighestLevel()
        {
            return _highestLevelCompleted;
        }

        public void RefreshLevel()
        {
            reloadedLevel = false;
        }

        public void ReloadedLevel()
        {
            reloadedLevel = true;
        }
    }

}