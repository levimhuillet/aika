using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The parent class for any objects which rise and fall according to their state
 */ 
public class HeightModulator : MonoBehaviour
{
    // protected Rigidbody2D m_BC; // this object's RigidBody2D
    protected Vector2 m_MinPos; // the starting position of this object (in prep for Reload() function and raising/lowering)
    protected Vector2 m_MaxPos; // the destination/max position of this object (in prep for Reload() function and raising/lowering)
    protected BoxCollider2D m_BC; // this modulator's box collider
    protected string m_State; // stores the object's state (e.g. min, rising, max, lowering)
    protected float m_Speed; // the speed at which this object rises and falls

    // ---------- Unity Callbacks ----------

    /*
     * Start is called before the first frame update
     */
    protected void Start()
    {
        m_BC = GetComponent<BoxCollider2D>();
    }

    /*
     * FixedUpdate is called once per frame during physics calculations
     */
    protected void FixedUpdate()
    {
        Modulate();
    }

    // ---------- Member Functions ----------

    /*
     * Causes object to rise or fall according to its state
     */ 
    protected void Modulate()
    {

        // Case when object is rising
        if (m_State.Equals("rising"))
        {
            // Case when object has reached its max height
            if (m_BC.bounds.center.y >= m_MaxPos.y)
            {
                // Set the height to exactly its max
                m_BC.transform.position = new Vector2(m_BC.bounds.center.x, m_MaxPos.y);

                // Change the state from "rising" to "max"
                m_State = "max";
            }
            // Case when object has not reached its max height
            else
            {
                // Continue rising
                m_BC.transform.position = new Vector2(m_BC.bounds.center.x, m_BC.bounds.center.y + m_Speed * Time.deltaTime);
            }
            
        }
        // Case when object is lowering
        else if (m_State.Equals("lowering"))
        {
            // Case when object has reached its min height
            if (m_BC.bounds.center.y <= m_MinPos.y)
            {
                // Set the height to exactly its min
                m_BC.transform.position = new Vector2(m_BC.bounds.center.x, m_MinPos.y);

                // Change the state from "lowering" to "min"
                m_State = "min";
            }
            else
            {
                // Continue lowering
                m_BC.transform.position = new Vector2(m_BC.bounds.center.x, m_BC.bounds.center.y - m_Speed * Time.deltaTime);
            }
            
        }
    }

    /*
     * Used by other objects for physics calculations
     */ 
    public float GetSpeed()
    {
        return m_Speed;
    }

    /*
     * Used by other objects for physics calculations, determining crushing, etc.
     */ 
    public string GetState()
    {
        return m_State;
    }
}
