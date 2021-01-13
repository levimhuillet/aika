using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The player is the aika controlled by the user in a scene.
 * Player actions are stored over the course of a scene, and when the level is reloaded a Replicate repeats their actions without user input.
 */
public class Player : Aika
{
    List<List<string>> m_MovementRecord; // A list of instructions per frame, inside a list of frame instructions per lifetime. Future replicate movements are pulled from here

    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback
     */
    new void Awake()
    {
        // Resolve aika Awake() function
        base.Awake();

        // Only the player begins with an empty movement record
        m_MovementRecord = new List<List<string>>();
    }

    /*
     * Start is called before the first frame update
     */
    new void Start()
    {
        // Resolve aika Start() function
        base.Start();
    }

    /*
     * Update is called once per frame
     */
    new void Update()
    {

        /*
        // Case when the player has finished their movement (by interacting with exit)
        if (m_HasFinishedMovement)
        {
            // Player disappears from scene on exiting
            this.gameObject.SetActive(false);
        }
        */

        // -- Record inputs and move accordingly

        List<string> thisFrameMovement = new List<string>(); // tracks all movement made during the present frame
        m_HasHorizontalMovement = false; // tracks if any relevant input was entered. If not, the player will need to be slowed
        bool isExiting = false; // tracks whether the player is exiting this frame

        // Case when user enters input to move left
        if (Input.GetKey("a"))
        {
            MoveLeft();
            // TODO: call left movement animation

            // Record this movement
            thisFrameMovement.Add("a");

            // Track this horizontal movement
            m_HasHorizontalMovement = true;
        }
        // Case when user enters input to move right
        else if (Input.GetKey("d")) // Note: left and right movement are mutually exclusive
        {
            MoveRight();
            // TODO: call right movement animation

            // Record this movement
            thisFrameMovement.Add("d");

            // Track this horizontal movement
            m_HasHorizontalMovement = true;
        }

        // Case when user enters input to move up
        if (Input.GetKey("w"))
        {
            Jump();
            // TODO: call jump movement animation

            //record this movement
            thisFrameMovement.Add("w");
        }
        else if (Input.GetKeyUp("w"))
        {
            EndJumping();

            thisFrameMovement.Add("ej");
        }
        // Case when user enters input to move down
        else if (Input.GetKey("s")) // Note: up and down movement are mutually exclusive
        {
            // Nothing yet
            // TODO: eventually fall faster
        }

        // Case when user enters input to jump
        if (Input.GetKeyDown("space"))
        {
            thisFrameMovement.Add("space"); // m_FrameNum - 1 because frameNum starts at 1, while m_MovementRecord index starts at 0
            
            // Check if player overlaps exit
            if (m_IsOverExit && this.m_FrameNum > 40) // prevents immediate reloading, which creates a useless replicate
            {
                // Player will be exiting this frame
                isExiting = true; // see (#) below. lm.ExitSequence() is not called immeditaely because this frame's movement is not saved until all inputs have been recorded
            }
            // Case when this player is overlapping a portal (currently only players may enter portals, not replicates)
            else if (m_PortalOverlapping != null)
            {
                // Enter the portal to the corresponding leve
                m_PortalOverlapping.EnterPortal();
            }

        }

        // Case when user enters input to interact
        if (Input.GetKeyDown("i"))
        {
            // Save interact instruction to replicate
            thisFrameMovement.Add("i"); // m_FrameNum - 1 because frameNum starts at 1, while m_MovementRecord index starts at 0

            // Interact with object
            Interact();
        }

        // Store all movements into m_MovementRecord
        m_MovementRecord.Add(thisFrameMovement);
        m_FrameNum++;

        // (#) Check whether player is exiting this frame
        if (isExiting)
        {
            // The palyer will leave the level
            // this.LeaveLevel();

            // Enter exit sequence
            lm.ExitSequence();
        }

        // Resolve aika Update() function
        base.Update();
        ApplyUpdate();
    }

    /*
     * FixedUpdate is called once per frame during physics calculations
     */
    new void FixedUpdate()
    {
        // Resolve aika's FixedUpdate() method, which will slow, accelerate, and maintain steady movement on moving structures
        base.FixedUpdate();
    }

    // ---------- Getters and Setters ----------

    /*
     * Returns the player's movement record
     */
    public List<List<string>> GetMovementRecord()
    {
        return m_MovementRecord;
    }

    // ---------- Scene Management ----------

    /*
     * Removes the player from the level
     */
    public void LeaveLevel()
    {
        // TODO: Call player exit animation
        gameObject.SetActive(false);
        m_HasFinishedMovement = true;
    }
}
