using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * An object that signals to another object (currently only elevator) when to change m_States
 */
public class Button : HeightModulator
{
    public LevelManager lm; // used to query whether other objects are above or below this block
    public Elevator elevator; // the elevator this button controls

    int m_NumObjectsAtop; // the number of objects on top of this button
    int m_OneFrame; // the current frame (stored in prep for ensuring the same object can only leave once in its frame (strange debugging issue))
    float m_LowerDistance; // how far the button lowers
    List<GameObject> objectsAtop; // a list of objects on top of this button (used to ensure an object leaving only decrements number of objects atop if it was indeed on  top before it exited)

    // ---------- Unity Callbacks ----------

    /*
     * Start is called before the first frame update
     */
    new void Start()
    {
        base.Start(); // HeightModulator.Start()

        m_State = "max"; // a button starts at its max position, then is pushed down
        m_MaxPos = m_BC.bounds.center;
        m_LowerDistance = 0.2f; // 0.4f
        m_MinPos.y = m_MaxPos.y - m_LowerDistance;
        m_Speed = 3; // 4

        objectsAtop = new List<GameObject>();
        m_NumObjectsAtop = 0;
        m_OneFrame = 0;
    }

    /*
     * FixedUpdate is called once per frame during physics calculations
     */
    new void FixedUpdate()
    {
        // -- Update button height according to its state
        base.FixedUpdate(); // HeightModulator.FixedUpdate()
    }

    // ---------- Member Functions ----------

    /*
     * Handles when an aika lands on this button
     */
    public void AikaArrived(GameObject aikaObj)
    {
        // Add the aika to the list of objects pressing down this button
        AddObjectAtop(aikaObj);

        // Keep player at level with button
        aikaObj.GetComponent<Aika>().SetParent(this.transform);
        aikaObj.gameObject.GetComponent<Aika>().Land();
    }

    /*
     * Handles when the aika leaves this button, whether from the top or by walking off
     */
    public void AikaLeft(GameObject aikaObj)
    {
        // Case when aika is recorded in the list of objects on top of this button
        if (objectsAtop.Contains(aikaObj))
        {
            // Remove this aika from the list of objects on top of this button
            RemoveObjectAtop(aikaObj);

            // Sever the aika from this button because it is no longer on top
            aikaObj.GetComponent<Aika>().SeverParent();
        }

        // Let the aika know it has left
        aikaObj.GetComponent<Aika>().Leave();
    }

    /*
     * Handles when a block lands on this button
     */
    public void BlockArrived(GameObject blockObj)
    {
        // Add the block to the list of objects pressing down this button
        AddObjectAtop(blockObj);

        // Keep block at level with button
        blockObj.GetComponent<NewBlock>().SetParent(this.transform);
    }

    /*
     * Handles when a block leaves this button
     */
    public void BlockLeft(GameObject blockObj)
    {
        // Case when block is recorded in the list of objects on top of this button
        if (objectsAtop.Contains(blockObj))
        {
            // Remove this block from the list of objects on top of this button
            RemoveObjectAtop(blockObj);

            // Sever the block from this button because it is no longer on top
            blockObj.GetComponent<NewBlock>().SeverParent();
        }
    }

    /*
     * When a block leaves this button from above,
     * passes control to BlockLeft() and ensures it is not called multiple times.
     */ 
    public void BlockLeaveAbove(GameObject blockObj)
    {
        // Prevents the same block from triggering this exit function twice in the same frame
        if (Time.frameCount != m_OneFrame)
        {
            // Block has left from the top
            BlockLeft(blockObj);
            m_OneFrame = Time.frameCount; // store the frame in which this exit was triggered
        }
    }

    /*
     * Add the given object the list of objects pressing down this button
     */
    public void AddObjectAtop(GameObject obj)
    {
        objectsAtop.Add(obj);
        m_NumObjectsAtop++;

        // Trigger elevator to rise or fall and button to push down
        RevalStates();
    }

    /*
     * Remove the given object the list of objects pressing down this button
     */
    public void RemoveObjectAtop(GameObject obj)
    {
        objectsAtop.Remove(obj);
        m_NumObjectsAtop--;

        // Trigger elevator to fall and button to rise
        RevalStates();
    }

    /*
     * The button will reevaluate its own state and the state of its elevator
     * according to the number of objects on top of this button.
     * Any number > 0 will trigger it, and = 0 will reset it to its default.
     */
    public void RevalStates()
    {
        // Case when there is at least one object on the button
        if (m_NumObjectsAtop > 0)
        {
            m_State = "lowering"; // button pressed down
            elevator.SetState("rising"); // elevator rises
        }
        // Case when there are <= 0 objects on the button
        else
        {
            // Case when there is a negative number of objects on the button
            if (m_NumObjectsAtop < 0)
            {
                // Some incorrect counting occured, usually due to edge collision cases
                m_NumObjectsAtop = 0;
            }

            // No objects are on the button
            m_State = "rising"; // button rises
            elevator.SetState("lowering"); // elevator lowers
        }
    }
}
