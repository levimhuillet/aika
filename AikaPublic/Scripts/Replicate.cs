using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * An Aika whose actions are pulled from a copy of one iteration of the player's movement.
 */
public class Replicate : Aika
{
    List<List<string>> m_MovementRecord; // A list of instructions per frame, inside a list of frame instructions per lifetime. Replicate actions are pulled from here

    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback
     */
    new void Awake()
    {
        base.Awake();
    }

    /*
     * Start is called before the first frame update
     */
    new void Start()
    {
        base.Start();
    }

    /*
     * Update is called once per frame
     */
    new void Update()
    {



        if (!m_HasFinishedMovement)
        {
            // Replicate player movement
            m_HasHorizontalMovement = false; // tracks if any relevant input was entered; if not, the Replicate needs to be slowed down

            if (m_FrameNum >= m_MovementRecord.Count)
            {
                // replicate ran through all movement instructions and failed to exit: paradox
                lm.DisplayParadoxMenu();
            }
            else
            {
                List<string> thisFrameMovement = m_MovementRecord[m_FrameNum];

                foreach (string instruction in thisFrameMovement)
                {
                    // read in left
                    if (instruction.Equals("a"))
                    {
                        //apply left force
                        MoveLeft();

                        // call left movement animation

                        m_HasHorizontalMovement = true;
                    }
                    // read in right
                    if (instruction.Equals("d"))
                    {
                        //apply right force
                        MoveRight();

                        // call right movement animation

                        m_HasHorizontalMovement = true;
                    }
                    // read in jump
                    if (instruction.Equals("w"))
                    {
                        //apply left force
                        Jump();

                        // call jump movement animation

                    }
                    // read in end jumping
                    if (instruction.Equals("ej"))
                    {
                        //apply left force
                        EndJumping();

                        // call jump movement animation

                    }
                    // read in exit
                    if (instruction.Equals("space"))
                    {
                        if (m_IsOverExit)
                        {
                            //replicate may exit
                            // if holding something, tie up the loose ends
                            if (this.GetComponent<Carrier>().m_ObjectCarrying != null)
                            {
                                if (this.GetComponent<Carrier>().m_ObjectCarrying.tag == "NewBlock")
                                {
                                    Interact(); // Drops the block

                                    //this.GetComponent<Carrier>().m_ObjectCarrying.GetComponent<NewBlock>().SetCarrier(null);
                                    //this.GetComponent<Carrier>().m_ObjectCarrying.GetComponent<NewBlock>().SeverParent();

                                }
                                //this.GetComponent<Carrier>().m_ObjectCarrying = null;
                            }
                            //shut off replicate
                            m_HasFinishedMovement = true;
                            this.gameObject.SetActive(false);
                            // decrement the number of remaining replicates
                            lm.RemoveReplicate();
                            //self.level.remainingReplicates.num = self.level.remainingReplicates.num - 1
                        }
                    }
                    // read in interact
                    if (instruction.Equals("i"))
                    {
                        Interact();
                    }
                }


                // increment m_FrameNum
                m_FrameNum++;
            }

        }

        base.Update();
        ApplyUpdate();
    }

    /*
     * FixedUpdate is called once per frame during physics calculations
     */
    new void FixedUpdate()
    {
        // slow, accelerate, and maintain steady movement on moving structures
        base.FixedUpdate();
    }

    // ---------- Scene Management ----------

    /*
     * Resets the replicate's variables to their initial state
     */
    new public void Reload()
    {
        base.Reload();
    }

    // ---------- Getters and Setters ----------

    /*
     * Sets the replicate's movement record
     */
    public void SetMovementRecord(List<List<string>> m_MovementRecord)
    {
        this.m_MovementRecord = m_MovementRecord;
    }
}
