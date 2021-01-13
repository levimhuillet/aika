using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Carries carriable objects
 */
public class Carrier : MonoBehaviour, ICarrier
{
    public GameObject m_ObjectCarrying; // the object being carried
    public float m_HeightBuffer; // the height at which the carried object should be above this object's top

    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback
     */
    void Awake()
    {
        m_ObjectCarrying = null;
        m_HeightBuffer = 0.1f;
    }

    // ---------- ICarrier Methods ----------

    /*
     * Implementation of the ICarrier PopToPosition() method
     */
    public void PopToPosition(GameObject objCarrying)
    {
        float transportX = this.gameObject.transform.position.x; // the x value of the carrier's position

        float transportY = this.gameObject.transform.position.y // carrier center y
           + this.gameObject.GetComponent<BoxCollider2D>().bounds.extents.y // distance from center of carrier sprite to carrier sprite edge
           + objCarrying.GetComponent<BoxCollider2D>().bounds.extents.y; // distance from center of block sprite to block sprite edge

        // ------------ Elevator Modifications
        // Case when object being carried is a Block
        if (objCarrying.tag == "NewBlock")
        {
            if (this.gameObject.GetComponent<Aika>() == null)
            {
                // offset is applied when the carrier is anything but an aika
                // transportX -= objCarrying.GetComponent<Block>().GetOffset(); // apply offset
            }

            // Sets the block above this carrier
            objCarrying.transform.position = new Vector2(transportX, transportY);
        }

        // ------------ Aika Modifications

        // Case when object being carried is an aika block
        if (objCarrying.tag == "NewBlock")
        {
            // Sets the block above this carrier
            objCarrying.transform.position = new Vector2(transportX, transportY - m_HeightBuffer);
        }
    }

    /*
     * Implementation of the ICarrier Carry() method
     */
    public void Carry(GameObject objCarrying)
    {
        if (objCarrying.tag == "NewBlock")
        {
            //GetObjectCarrying().GetComponent<Raybody>().Move(this.GetComponent<Raybody>().m_Velocity);

            Vector2 BlockedVelocity = GetComponent<Raybody>().m_Velocity;

            ParentModifiable pmCarrying = objCarrying.GetComponent<ParentModifiable>();
            Raybody raybodyCarrying = objCarrying.GetComponent<Raybody>();

            if (pmCarrying.GetBlockedMovement("left") && BlockedVelocity.x < 0)
            {
                BlockedVelocity.x = 0f;
            }
            if (pmCarrying.GetBlockedMovement("right") && BlockedVelocity.x > 0)
            {
                BlockedVelocity.x = 0f;
            }
            // TODO: better place for blocked above?
            // if (blocked up)

            raybodyCarrying.m_Velocity = new Vector2(BlockedVelocity.x, 0f);

            raybodyCarrying.Move(raybodyCarrying.m_Velocity);

            float newY = this.GetComponent<BoxCollider2D>().transform.position.y // the parent y
                + this.GetComponent<BoxCollider2D>().bounds.extents.y // the parent sprite y bounds (to reach top edge of parent sprite)
                + objCarrying.GetComponent<BoxCollider2D>().bounds.extents.y; // this aika's y bounds (to reach center of this aika's sprite)

            objCarrying.transform.position = new Vector2(objCarrying.transform.position.x, newY);
        }
    }

    /*
     * Sets the object the aika is carrying
     */
    public void SetObjectCarrying(GameObject carryingObj)
    {
        this.m_ObjectCarrying = carryingObj;
        if (m_ObjectCarrying != null && m_ObjectCarrying.tag == "AikaBlock")
        {
            // this.m_ObjectCarrying.GetComponent<AikaBlock>().GetPhysicalBlock().gameObject.layer = 15;
        }
    }

    /*
     * Returns the object the aika is carrying
     */
    public GameObject GetObjectCarrying()
    {
        return this.m_ObjectCarrying;
    }

}
