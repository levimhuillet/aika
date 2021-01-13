using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A Raybody often works in tandem with a Raycaster. It is responsible for the movement side of things.
 * In comparison, the Raycaster detects collisions.
 * For example, an object that needs to handle collisions but never moves (such as a Key) only needs a Raycaster.
 * Together these are my alternative to using Unity's rigidbodies, which were causing me issues.
 */
public class Raybody : MonoBehaviour, IRaybody
{
    public float m_Gravity { get; set; } // acceleration of gravity at any point in time
    public float m_DefaultGravity { get; set; } // acceleration of gravity when standing
    public float m_FallingGravity { get; set; } // (TODO: do I use this?) acceleration of gravity when falling (falling is quicker)
    public Vector2 m_Velocity { get; set; } // this object's velocity at any point in time
    public Vector2 m_PreviousVelocity { get; set; } // this object's velocity during the previous frame
    public float m_CollisionBuffer { get; set; } // the desired amount of space between this object and what it collides with
    public bool m_IsSomethingBelowThisTurn { get; set; } // TODO: remove this duplicate variable (m_IsGrounded should cover it)
    public bool m_IsGrounded { get; set; } // whether the aika is on a solid surface from which they may jump

    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback 
     */
    public void Awake()
    {
        m_DefaultGravity = -9.81f * 2.5f;
        m_Gravity = m_DefaultGravity;
        m_FallingGravity = 3f * m_DefaultGravity;

        m_Velocity = new Vector2(0f, 0f);
        m_PreviousVelocity = new Vector2(0f, 0f);
        m_CollisionBuffer = 0.1f;
        m_IsSomethingBelowThisTurn = false;
        m_IsGrounded = false; // objects hover a little above floor at the start
    }

    // ---------- Member Functions ----------

    /*
     * Move's the object to its new position after all physics calculations have been performed
     */
    public void Move(Vector2 dir)
    {
        this.transform.position = (Vector2)this.transform.position + dir * Time.deltaTime;
    }

    // ---------- Raycasts ---------- 

    /*
     * One succinct function which calls each relevant raycasting functions below.
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastStructures() { return; }

    /*
     * Raycasts for objects with the tag "Structures" above this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastStructuresAbove() { return; }

    /*
     * Raycasts for objects with the tag "Structures" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastStructuresSide() { return; }

    /*
     * Raycasts for objects with the tag "Structures" below this Raybody 
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastStructuresBelow() { return; }

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "MovingStructures".
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastMovingStructures() { return; }

    /*
     * Raycasts for objects with the tag "MovingStructures" above this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastMovingStructuresAbove() { return; }

    /*
     * Raycasts for objects with the tag "MovingStructures" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastMovingStructuresSide() { return; }

    /*
     * Raycasts for objects with the tag "MovingStructures" below this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastMovingStructuresBelow() { return; }

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "Button".
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastButtons() { return; }

    /*
     * Raycasts for objects with the tag "Button" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastButtonsSide() { return; }

    /*
     * Raycasts for objects with the tag "Button" below this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastButtonsBelow() { return; }

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "NewBlock".
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastNewBlocks() { return; }

    /*
     * Raycasts for objects with the tag "NewBlock" above this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastNewBlocksAbove() { return; }

    /*
     * Raycasts for objects with the tag "NewBlock" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastNewBlocksSide() { return; }

    /*
     * Raycasts for objects with the tag "NewBlock" below this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastNewBlocksBelow() { return; }

    /*
     * Returns this Raybody's velocity
     */
    public Vector2 GetVelocity() { return m_Velocity; }

    /*
     * Returns this Raybody's velocity in the previous frame
     */
    public Vector2 GetPreviousVelocity() { return m_PreviousVelocity; }

    /*
     * Adds a velocity to this Raybody's velocity
     */
    public void AddVelocity(Vector2 velocity)
    {
        m_Velocity += velocity;
    }
}
