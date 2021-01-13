using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

/*
 * Tracks each object in the scene and triggers scene reloading/scene changes
 */
public class LevelManager : MonoBehaviour
{
    static List<GameObject> replicates;

    // Objects
    public GameObject playerPrefab; // the prefab used to instantiate the player
    public Player player; // the player
    public List<GameObject> buttons; // the buttons on the scene
    public List<GameObject> elevators; // the elevators in the scene
    public List<GameObject> aikaBlocks; // the aika blocks in the scene
    public Exit exit; // the exit
    public Key key; // the key to unlock the exit
    public GameObject replicatePrefab; // the prefab used to instantiate replicates
    // List<GameObject> replicates; // a list of all replicates in the scene

    // Scene Triggers
    int m_NumRemainingReplicates; // the number of replicates remaining in the scene (quicker access than replicates.Count)
    bool m_AwaitingReplicates; // true if player has exited successfully, but the other replicates have yet to reach the exit

    // Scene Reloading and Changing
    public string currentScene; // the name of the current scene
    public string nextScene; // the name of the next scene
    public int levelNum; // the number of this level

    // Meta World
    public bool isMeta; // Only checked for the MetaWorld Level Manager

    // UI (may shift to a more specialized script later)
    public Text awaitText; // text displayed when replicates are making their way to the exit (m_AwaitingReplicates is true)
    public Text paradoxText; // text displayed when a paradox occured (m_IsParadox is true)
    public Text crushedText; // text displayed when an aika has been crushed

    // Menus
    public EventSystem eventSystem; // for rendering Canvases
    public Canvas levelMenu; // Canvas containing the In-Level Menu
    public Canvas metaMenu; // Canvas containing the Meta World Menu
    public Canvas paradoxMenu; // Canvas containing options when a paradox occurs
    public Canvas crushedMenu; // Canvas containing options when an Aika is crushed

    // Debugging
    bool DEBUG_KEYS_ENABLED = true; // Allows a debugger to quickly skip through levels

    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback
     */
    protected void Awake()
    {

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

        foreach (GameObject replicate in ReplicateList.instance.replicates)
        {
            replicate.GetComponent<Aika>().SetLevelManager(this);
        }

        m_NumRemainingReplicates = ReplicateList.instance.replicates.Count;
        m_AwaitingReplicates = false;

        // Case when the level has an exit (MetaWorld would be a case with no exit)
        if (exit != null)
        {
            // Instantiate the player at the location of the exit. The player starts at the exit in all levels.
            GameObject newPlayer = Instantiate(playerPrefab, exit.GetStartPosition(), Quaternion.identity);
            player = newPlayer.GetComponent<Player>();

            // Create a link between this lm and the player
            player.SetLevelManager(this);
        }
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

            if (Input.GetKeyDown("escape"))
            {
                // return to main menu
                // ReturnToMainMenu();

                // Case when in the Meta World
                if (this.isMeta)
                {
                    if (metaMenu.gameObject.activeSelf == true)
                    {
                        CloseMetaMenu();
                    }
                    else
                    {
                        DisplayMetaMenu();
                    }
                }
                // Case when in a level
                else
                {
                    if (levelMenu.gameObject.activeSelf == true)
                    {
                        CloseLevelMenu();
                    }
                    else
                    {
                        DisplayLevelMenu();
                    }
                }
            }
        }

        // Case when all all replicates have returned to the exit
        if (m_AwaitingReplicates)
        {
            if (m_NumRemainingReplicates == 0)
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
        if (m_NumRemainingReplicates == 0)
        {
            // All replicants have returned; progress to next scene
            NextScene();
        }
        else
        {
            // Signal that the last thing needed to progress is for replicants to return (lm will check in Update function)
            m_AwaitingReplicates = true;

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
        if (m_NumRemainingReplicates == 1)
        {
            awaitText.text = "Awaiting " + m_NumRemainingReplicates + " replicate"; // without a plural 's'
        }
        else
        {
            awaitText.text = "Awaiting " + m_NumRemainingReplicates + " replicates"; // with a plural 's'
        }
    }

    /*
     * Takes the player's movement record and saves it to a new list, which will be used by a new replicate
     */
    List<List<string>> CopyMovements(List<List<string>> movementRecord)
    {
        List<List<string>> newMovementRecord = new List<List<string>>(); // stores all the movements across an aika's lifetime

        foreach (List<string> frameMovements in movementRecord) // for each frame
        {
            List<string> newFrameMovements = new List<string>(); // stores all movement input instructions within one frame

            foreach (string instruction in frameMovements) // for each instruction
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
        m_NumRemainingReplicates--;
        UpdateAwaitText();
    }


    // ---------- Object Positional Checks ----------

    //TODO: consolidate the IsAbove functions

    /*
     * Given two game objects, returns whether the first object is above the second object and not to either side
     * Requires Objects to have boxCollider2D
     * 
     * enteringObj - 
     * stationaryObj - 
     */
    public bool IsDirectlyAbove(GameObject enteringObj, GameObject stationaryObj, float margin = 0.3f)
    {
        // First check if the calculation is necessary by seeing if one object is to the side of the other
        if (IsToSide(enteringObj, stationaryObj))
        {
            // Object may be above, but it is not directly above.
            return false;
        }

        BoxCollider2D enteringBC = enteringObj.GetComponent<BoxCollider2D>(); // 2D Box Collider of the first object
        BoxCollider2D stationaryBC = stationaryObj.GetComponent<BoxCollider2D>(); // 2D Box Collider of the second object

        // Check if (the center position y minus the sprite boundary y) is greater than or equal to (the center position y + the sprite boundary y)
        if (enteringBC.transform.position.y - enteringBC.bounds.extents.y + margin
            >=
            stationaryBC.transform.position.y + stationaryBC.bounds.extents.y)
        {
            return true;
        }

        // Object was not above
        return false;
    }

    /*
     * Given two game objects, returns whether the center of the first object is above the center of the second
     */
    public bool CenterIsAbove(GameObject enteringObj, GameObject stationaryObj)
    {
        BoxCollider2D enteringBC = enteringObj.GetComponent<BoxCollider2D>(); // 2D Box Collider of the first object
        BoxCollider2D stationaryBC = stationaryObj.GetComponent<BoxCollider2D>(); // 2D Box Collider of the second object

        // Check if (the center position y minus the sprite boundary y) is greater than or equal to (the center position y + the sprite boundary y)
        if (enteringBC.transform.position.y
            >=
            stationaryBC.transform.position.y)
        {
            return true;
        }

        // Object was not above
        return false;
    }

    /*
     * Given two game objects, returns whether the entriety of the first object is above the entirety of the second
     */
    public bool IsAbove(GameObject enteringObj, GameObject stationaryObj, float margin = 0.3f)
    {
        BoxCollider2D enteringBC = enteringObj.GetComponent<BoxCollider2D>(); // 2D Box Collider of the first object
        BoxCollider2D stationaryBC = stationaryObj.GetComponent<BoxCollider2D>(); // 2D Box Collider of the second object

        // Check if (the center position y minus the sprite boundary y) is greater than or equal to (the center position y + the sprite boundary y)
        if (enteringBC.transform.position.y - enteringBC.bounds.extents.y + margin
            >=
            stationaryBC.transform.position.y + stationaryBC.bounds.extents.y)
        {
            return true;
        }

        // Object was not above
        return false;
    }
    

    /*
     * Given two game objects, returns whether the first object is to the right of the left of the second object
     * Requires Objects to have boxCollider2D
     */
    public bool IsToSide(GameObject enteringObj, GameObject stationaryObj, float margin = 0f)
    {
        BoxCollider2D enteringBC = enteringObj.GetComponent<BoxCollider2D>(); // 2D Box Collider of the first object
        BoxCollider2D stationaryBC = stationaryObj.GetComponent<BoxCollider2D>(); // 2D Box Collider of the second object

        if(IsToLeft(enteringBC, stationaryBC, margin))
        {
            return true;
        }
        if (IsToRight(enteringBC, stationaryBC, margin))
        {
            return true;
        }

        // Object is neither to the left nor the right
        return false;
    }

    /*
     * Given two game objects, returns whether the entirety of the first object is to the left of the entirety of the second
     */
    public bool IsToLeft(BoxCollider2D enteringBC, BoxCollider2D stationaryBC, float margin = 0.3f)
    {
        // Check if (the center position x + the sprite boundary x) is less than or equal to (the center position x - the sprite boundary x)
        if (enteringBC.bounds.center.x + enteringBC.bounds.extents.x - margin
            <
            stationaryBC.bounds.center.x - stationaryBC.bounds.extents.x)
        {
            // is to the left
            return true;
        }

        return false;
    }

    /*
     * Given two game objects, returns whether the entriety of the first object is to the right of the entirety of the second
     */
    public bool IsToRight(BoxCollider2D enteringBC, BoxCollider2D stationaryBC, float margin = 0.3f)
    {
        // Check if (the center position x - the sprite boundary x) is greater than or equal to (the center position x + the sprite boundary x)
        if (enteringBC.bounds.center.x - enteringBC.bounds.extents.x + margin
            >
            stationaryBC.bounds.center.x + stationaryBC.bounds.extents.x)
        {
            // is to the right
            return true;
        }

        return false;
    }

    // ---------- Scene Management ----------

    /*
     * Called when player attempts to exit the level.
     * If the exit is unlocked, the player is disabled and lm will check if any replicants have not returned
     * Else the player's actions are saved before the level is reloaded with the new replicant
     * (May rename to ReplicateOrExitSequence for clarity)
     */
    public void ExitSequence()
    {
        // Check if exit is unlocked
        if (exit.GetIsUnlocked())
        {
            // await replicates
            AwaitReplicates();

            player.LeaveLevel();
        }
        // else reload the level with a new replicate
        else
        {
            // create new replicate
            GameObject newReplicate = Instantiate(replicatePrefab, exit.GetStartPosition(), Quaternion.identity);
            newReplicate.GetComponent<Replicate>().SetMovementRecord(CopyMovements(player.GetMovementRecord()));
            newReplicate.GetComponent<Replicate>().SetLevelManager(this);
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

        // Reload the scene
        SceneManager.LoadScene(currentScene);
    }

    /*
     * Start the Scene from scratch
     */
    public void RestartLevel()
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

        // Reload the scene
        SceneManager.LoadScene(currentScene);
    }

    /*
     * Proceeds to the next scene
     */ 
    void NextScene()
    {
        // Destroy all replicates, since a new level is starting from scratch
        foreach (GameObject replicate in ReplicateList.instance.replicates)
        {
            Destroy(replicate); // destroy
        }

        // Reset the static ReplicateList so that a new batch of replicates may be generated (TODO: probabaly overkill at the moment)
        ReplicateList.instance.replicates.Clear(); // clear replicate list
        ReplicateList.instance.replicates = null; // nullify replicate list
        Destroy(ReplicateList.instance.gameObject); // destroy ReplicateList instance
        ReplicateList.instance = null; // nullify the instance

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

        // Reload the scene
        SceneManager.LoadScene("MetaWorld");
    }

    /*
     * Clears the replicate list and returns to the main menu
     */ 
    public void ReturnToMainMenu()
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

        // Reload the scene
        SceneManager.LoadScene("MainMenu");
    }

    // ---------- Menus ----------

    // Level Menu

    /*
     * Displays the in-level menu
     */ 
    void DisplayLevelMenu()
    {
        levelMenu.gameObject.SetActive(true);
        eventSystem.gameObject.SetActive(true);
    }

    /*
     * Closes the in-level menu
     */ 
    void CloseLevelMenu()
    {
        levelMenu.gameObject.SetActive(false);
        eventSystem.gameObject.SetActive(false);
    }

    // Meta Menu

    /*
     * Displays the menu for the Meta World
     */ 
    void DisplayMetaMenu()
    {
        metaMenu.gameObject.SetActive(true);
        eventSystem.gameObject.SetActive(true);
    }

    /*
     * Closes the menu for the Meta World 
     */ 
    void CloseMetaMenu()
    {
        metaMenu.gameObject.SetActive(false);
        eventSystem.gameObject.SetActive(false);
    }

    // Paradox Menu

    /*
     * Displays the menu when a paradox occurs
     */ 
    public void DisplayParadoxMenu()
    {
        paradoxMenu.gameObject.SetActive(true);
        eventSystem.gameObject.SetActive(true);
    }

    /*
     * Closes the paradox menu
     */ 
    public void CloseParadoxMenu()
    {
        paradoxMenu.gameObject.SetActive(false);
        eventSystem.gameObject.SetActive(false);
    }

    // Crushed Menu

    /*
     * Displays the menu when an aika is crushed
     */ 
    public void DisplayCrushedMenu()
    {
        crushedMenu.gameObject.SetActive(true);
        eventSystem.gameObject.SetActive(true);
    }

    /*
     * Closes the crushed menu
     */ 
    public void CloseCrushedMenu()
    {
        crushedMenu.gameObject.SetActive(false);
        eventSystem.gameObject.SetActive(false);
    }
}
