using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using AikaGame.Aikas;
using AikaGame.Entries;
using UnityEngine.Events;

using AikaGame.Tutorials;

namespace AikaGame.Managers
{
    /*
     * Tracks each object in the scene and triggers scene reloading/scene changes
     */
    public class LevelManager : MonoBehaviour
    {
        static List<GameObject> replicates;

        // Objects
        public GameObject playerPrefab; // the prefab used to instantiate the player
        public Player player; // the player
        float _playerStartBuffer = 0.3f; // the vertical distance between the exit center and the player start position
        public List<GameObject> buttons; // the buttons on the scene
        public List<GameObject> elevators; // the elevators in the scene
        public List<GameObject> aikaBlocks; // the aika blocks in the scene
        public Exit exit; // the exit
        public GameObject replicatePrefab; // the prefab used to instantiate replicates
                                           // List<GameObject> replicates; // a list of all replicates in the scene

        // Scene Triggers
        int _numRemainingReplicates; // the number of replicates remaining in the scene (quicker access than replicates.Count)
        bool _awaitingReplicates; // true if player has exited successfully, but the other replicates have yet to reach the exit

        // Scene Reloading and Changing
        public string currentScene; // the name of the current scene
        public string nextScene; // the name of the next scene
        public int levelNum; // the number of this level

        // Meta World
        public bool isMeta; // Only checked for the MetaWorld Level Manager

        // UI (may shift to a more specialized script later)
        public Text awaitText; // text displayed when replicates are making their way to the exit (_awaitingReplicates is true)

        // Menus
        public MenuManager menuManager;

        public static UnityEvent ExitSequence = new UnityEvent();
        public static UnityEvent ReplicateExiting = new UnityEvent();
        public static UnityEvent Restart = new UnityEvent();
        public static UnityEvent TogglePause = new UnityEvent();
        public static UnityEvent Pause = new UnityEvent();
        public static UnityEvent Unpause = new UnityEvent();

        public static LevelManager instance;

        // Debugging
        bool DEBUG_KEYS_ENABLED = true; // Allows a debugger to quickly skip through levels

        // ---------- Unity Callbacks ----------

        protected void Awake()
        {
            instance = this;

            // Case when the level has an exit (MetaWorld would be a case with no exit)
            if (exit != null)
            {
                Vector2 playerStart = new Vector2(exit.transform.position.x, exit.transform.position.y - _playerStartBuffer);
                // Instantiate the player at the location of the exit. The player starts at the exit in all levels.
                GameObject newPlayer = Instantiate(playerPrefab, playerStart, Quaternion.identity);
                player = newPlayer.GetComponent<Player>();
            }
        }

        /*
         * Unity Callback
         */
        protected void Start()
        {
            // Set Framerate to a consistent level
            QualitySettings.vSyncCount = 0;
            //Application.targetFrameRate = 50; //45;

            // Initialize member variables
            if (ReplicateList.instance.replicates == null)
            {
                ReplicateList.instance.replicates = new List<GameObject>();
            }

            _numRemainingReplicates = ReplicateList.instance.replicates.Count;
            _awaitingReplicates = false;

            ExitSequence.AddListener(ExitLevel);
            ReplicateExiting.AddListener(RemoveReplicate);
        }

        /*
         * Unity Callback
         */
        void Update()
        {
            // Check if the debugging programmer wishes to change the game state
            if (DEBUG_KEYS_ENABLED)
            {
                if (Input.GetKeyDown("n"))
                {
                    // progress to next scene
                    NextScene();
                }

                if (Input.GetKeyDown("r"))
                {
                    // restart level
                    RestartLevel();
                }
            }

            if (Input.GetKeyDown("escape"))
            {
                // Case when in the Meta World
                if (this.isMeta)
                {
                    menuManager.ToggleMetaMenu();
                }
                // Case when in a level
                else
                {
                    menuManager.ToggleLevelMenu();
                }
            }

            // Case when all all replicates have returned to the exit
            if (_awaitingReplicates)
            {
                if (_numRemainingReplicates == 0)
                {
                    // All replicants have returned successfully; progress to next scene
                    NextScene();
                }
            }
        }

        // ---------- Member Functions ----------

        /*
         * Load the next scene if all replicants have returned, else trigger the awaiting text
         */
        void AwaitReplicates()
        {
            if (_numRemainingReplicates == 0)
            {
                // All replicants have returned; progress to next scene
                NextScene();
            }
            else
            {
                // Signal that the last thing needed to progress is for replicants to return (lm will check in Update function)
                _awaitingReplicates = true;

                // Display the number of replicants remaining
                EnableAwaitText();
            }
        }

        /*
         * Enable and update the awaiting text
         */
        void EnableAwaitText()
        {
            if (!awaitText.enabled)
            {
                awaitText.enabled = true;
            }

            // Update the text to display the correct number of remaining replicants
            UpdateAwaitText();
        }

        /*
         * Update the awaiting text to display the correct number of remaining replicants
         */
        void UpdateAwaitText()
        {
            if (_numRemainingReplicates == 1)
            {
                awaitText.text = "Awaiting " + _numRemainingReplicates + " replicate"; // without a plural 's'
            }
            else
            {
                awaitText.text = "Awaiting " + _numRemainingReplicates + " replicates"; // with a plural 's'
            }
        }

        /*
         * Takes the player's movement record and saves it to a new list, which will be used by a new replicate
         */
        List<List<ReplicateMove>> CopyMovements(List<List<ReplicateMove>> movementRecord)
        {
            List<List<ReplicateMove>> newMovementRecord = new List<List<ReplicateMove>>(); // stores all the movements across an aika's lifetime

            foreach (List<ReplicateMove> frameMovements in movementRecord) // for each frame
            {
                List<ReplicateMove> newFrameMovements = new List<ReplicateMove>(); // stores all movement input instructions within one frame

                foreach (ReplicateMove instruction in frameMovements) // for each instruction
                {
                    // add the instruction in this frame for this aika
                    newFrameMovements.Add(instruction);
                }

                newMovementRecord.Add(newFrameMovements); // add the instructions from the frame into the entire movement record
            }

            return newMovementRecord;
        }

        // ---------- External Signals ----------

        /*
         * Called when a replicate successfully leaves the scene.
         * Decrements the count and updates the awaiting text. 
         */
        public void RemoveReplicate()
        {
            _numRemainingReplicates--;
            UpdateAwaitText();
        }

        // ---------- Scene Management ----------

        /*
         * Called when player attempts to exit the level.
         * If the exit is unlocked, the player is disabled and lm will check if any replicants have not returned
         * Else the player's actions are saved before the level is reloaded with the new replicant
         * (May rename to ReplicateOrExitSequence for clarity)
         */
        public void ExitLevel()
        {
            // Check if exit is unlocked
            if (exit.isUnlocked)
            {
                // await replicates
                AwaitReplicates();

                player.LeaveLevel();
            }
            // else reload the level with a new replicate
            else
            {
                // create new replicate
                GameObject newReplicate = Instantiate(replicatePrefab, exit.transform.position, Quaternion.identity);
                newReplicate.GetComponent<Replicate>().SetMovementRecord(CopyMovements(player.GetMovementRecord()));
                // LevelManager.TogglePause.AddListener(newReplicate.GetComponent<Replicate>().Pausable.TogglePause);
                DontDestroyOnLoad(newReplicate);

                // save replicate 
                ReplicateList.instance.replicates.Add(newReplicate);

                // reload level
                ReloadLevel();
            }
        }

        /*
         * Restarts the level, maintaining constant objects such as replicates
         */
        public void ReloadLevel()
        {
            // Reload and preserve each replicate
            foreach (GameObject replicate in ReplicateList.instance.replicates)
            {
                replicate.GetComponent<Replicate>().Reload(); // reload
                DontDestroyOnLoad(replicate); // preserve
            }

            if (Tutorial.instance != null)
            {
                DontDestroyOnLoad(Tutorial.instance);
            }
            Unpause.Invoke();

            // Reload the scene
            SceneManager.LoadScene(currentScene);
        }

        /*
         * Start the Scene from scratch
         */
        public void RestartLevel()
        {
            Restart.Invoke();
            ClearScene();

            // Reload the scene
            SceneManager.LoadScene(currentScene);
        }

        /*
         * Proceeds to the next scene
         */
        void NextScene()
        {
            ClearScene();

            // Level has been completed, so mark it as completed in GameStateManager
            GameStateManager.instance.CompleteLevel(levelNum);

            // Load the next scene
            SceneManager.LoadScene(nextScene);
        }

        /*
         * Clears the replicate list and returns to the level selection (meta) world
         */
        public void ReturnToLevelSelect()
        {
            ClearScene();

            // Reload the scene
            SceneManager.LoadScene("MetaWorld");
        }

        /*
         * Clears the replicate list and returns to the main menu
         */
        public void ReturnToMainMenu()
        {
            ClearReplicates();

            Destroy(GameStateManager.instance.gameObject);

            // Reload the scene
            SceneManager.LoadScene("MainMenu");
        }

        private void ClearScene()
        {
            ClearReplicates();
            if (Tutorial.instance != null) { Destroy(Tutorial.instance.gameObject); }
            LevelManager.TogglePause.RemoveAllListeners();
        }

        private void ClearReplicates()
        {
            // Destroy all replicates, since the level is starting from scratch
            foreach (GameObject replicate in ReplicateList.instance.replicates)
            {
                Destroy(replicate); // destroy
            }

            // Reset the static ReplicateList so that a new batch of replicates may be generated (TODO: probabaly overkill at the moment)
            ReplicateList.instance.replicates.Clear(); // clear replicate list
            ReplicateList.instance.replicates = null; // nullify replicate list
            Destroy(ReplicateList.instance.gameObject); // destroy ReplicateList instance
            ReplicateList.instance = null; // nullify the instance
        }

        public Exit GetExit()
        {
            return exit;
        }
    }
}