using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Contains methods for modifying the transform parent of the class
 */
public class ParentModifiable : MonoBehaviour, IParentModifiable
{
    float m_OffsetUnderneath; // the x offset of the parent object and this aika

    bool m_BlockedMovementLeft; // whether the object is physically blocked on its left
    bool m_BlockedMovementRight; // whether the object is physically blocked on its right
    bool m_BlockedMovementUp; // whether the object is physically blocked above

    /*
    bool m_SupporterMovingLeft;
    bool m_SupporterMovingRight;
    bool m_SupporterMovingUp;
    */

    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback
     */
    public void Awake()
    {
        m_OffsetUnderneath = 0f;
    }

    // ---------- IParentModifiable Methods ----------

    /*
     * Sets this object's parent to null, and sets this object's relative offset to 0, because there is no parent to have an offset with
     */
    public void SeverParent()
    {
        transform.parent = null;
        m_OffsetUnderneath = 0f;
    }

    /*
     * Keeps object on top of its parent
     * Current implementation relies on transform.parent for moving.
     * This commented code is saved in case it becomes useful again
     * TODO: candidate for removal
     */
    public void BeSupported()
    {
        if (transform.parent.tag == "NewBlock" && transform.parent.gameObject.GetComponent<NewBlock>().GetLowestAika() != null)
        {
            /*
            float newY = transform.parent.gameObject.GetComponent<BoxCollider2D>().transform.position.y // the parent y
                + transform.parent.gameObject.GetComponent<BoxCollider2D>().bounds.extents.y // the parent sprite y bounds (to reach top edge of parent sprite)
                + GetComponent<BoxCollider2D>().bounds.extents.y; // this aika's y bounds (to reach center of this aika's sprite)

            this.transform.position = new Vector2(this.transform.position.x, newY);
            */

            /*
            Vector2 BlockedVelocity = transform.parent.gameObject.GetComponent<NewBlock>().GetLowestAika().GetComponent<Raybody>().m_Velocity;

            if (this.GetBlockedMovement("left") && BlockedVelocity.x < 0)
            {
                BlockedVelocity.x = 0f;
            }
            if (this.GetBlockedMovement("right") && BlockedVelocity.x > 0)
            {
                BlockedVelocity.x = 0f;
            }
            // TODO: better place for blocked above?
            // if (blocked up)

            this.gameObject.GetComponent<Raybody>().m_Velocity = new Vector2(BlockedVelocity.x, 0f);

            this.gameObject.GetComponent<Raybody>().Move(this.gameObject.GetComponent<Raybody>().m_Velocity);

            float newY = transform.parent.gameObject.GetComponent<BoxCollider2D>().transform.position.y // the parent y
                + transform.parent.gameObject.GetComponent<BoxCollider2D>().bounds.extents.y // the parent sprite y bounds (to reach top edge of parent sprite)
                + this.GetComponent<BoxCollider2D>().bounds.extents.y; // this aika's y bounds (to reach center of this aika's sprite)

            this.transform.position = new Vector2(this.transform.position.x, newY);
            */
        }


        /*
        // Maintain steady movement on the aika's parent

        float newX;  // the new X value for the aika, updated to match the parent's movement

        // if the solid surface is horizontal-moveable, the aika's transform.position.x should match the parent (unless input is given)
        // Currently, AikaBlock is the only surface which moves horizontally
        // TODO: allow movement in direction of block movement

        Aika pmAika = this.gameObject.GetComponent<Aika>();
        if (pmAika != null)
        {
            if (transform.parent.gameObject.tag == "NewBlock" && !pmAika.GetHasHorizontalMovement())
            {
                // Update the aika's position to be in the same relative location on the moving surface
                newX = transform.parent.position.x - this.GetOffset();
            }
            else
            {
                // The aika's position should not be affected by the parent's x position, because the parent's x position does not change
                newX = transform.position.x;
                this.SetOffset(transform.parent.position.x - transform.position.x);
            }
        }
        else if (this.gameObject.GetComponent<NewBlock>() != null)
        {
            // TODO: Make sure you understand why this is "Block" while aika uses "AikaBlock"
            if (transform.parent.gameObject.tag == "NewBlock")
            {
                // Update the block's position to be in the same relative location on the moving surface
                // TODO: allow relative movement on block; use SetParent() methods for all which include offsetUnderneath, then see Aika implementation
                newX = transform.parent.position.x - this.GetOffset();
            }
            else
            {
                // The block's position should not be affected by the parent's x position, because the parent's x position does not change
                newX = transform.position.x;
                this.SetOffset(transform.parent.position.x - transform.position.x);
            }
        }
        else
        {
            // Doesn't occur, just avoids an error for unassigned value for newX
            newX = transform.position.x;
        }

        float newY = transform.parent.gameObject.GetComponent<BoxCollider2D>().transform.position.y // the parent y
            + transform.parent.gameObject.GetComponent<BoxCollider2D>().bounds.extents.y // the parent sprite y bounds (to reach top edge of parent sprite)
            + GetComponent<BoxCollider2D>().bounds.extents.y; // this aika's y bounds (to reach center of this aika's sprite)

        if (this.gameObject.GetComponent<NewBlock>() != null)
        {
            // Case when parent is a Button or Platform (or other object which requires extra calibration)
            if (transform.parent.gameObject.tag == "Button" || transform.parent.gameObject.tag == "Elevator")
            {
                newY = newY - 0.045f; // reduces jittering
            }
        }

        if (newX <= transform.position.x)
        {
            m_SupporterMovingLeft = true;
        }
        else
        {
            m_SupporterMovingLeft = false;
        }
        if (newX >= transform.position.x)
        {
            m_SupporterMovingRight = true;
        }
        else
        {
            m_SupporterMovingRight = false;
        }

        if ((m_BlockedMovementLeft && m_SupporterMovingLeft) || (m_BlockedMovementRight && m_SupporterMovingRight))
        {
            newX = this.gameObject.GetComponent<BoxCollider2D>().bounds.center.x;
        }

        // Move the aika to the correct x, then keep it level
        transform.position = new Vector2(newX, newY);
        // this.gameObject.GetComponent<Rigidbody2D>().MovePosition(new Vector2(newX, newY));

        */
    }

    // ---------- IParentModifiable Getters and Setters ----------

    /*
     * Sets this aika's parent to the given Transform, and establishes the starting offset (to preserve discrepancy between aika's center and parent's center)
     */
    public void SetParent(Transform tf)
    {
        if (tf == null)
        {
            // To set a parent as null, instead use SeverParent()
            Debug.Log("Error in Set Parent: tf should be a non-null value");
        }

        transform.SetParent(tf, true);
        m_OffsetUnderneath = transform.parent.position.x - transform.position.x;
    }

    /*
     * Sets this aika's parent to the given Transform, and establishes the starting offset (to preserve discrepancy between aika's center and parent's center)
     */
    public void SetOffset(float offset)
    {
        m_OffsetUnderneath = offset;
    }

    /*
     * Return this object's offset from its parent
     */
    public float GetOffset()
    {
        return m_OffsetUnderneath;
    }

    // TODO: determine if this code is necessary

    /*
     * Flags this object as blocked in the specified direction
     */
    public void DisableMovement(string dir)
    {
        if (dir == "left")
        {
            m_BlockedMovementLeft = true;
        }
        else if (dir == "right")
        {
            m_BlockedMovementRight = true;
        }
        else if (dir == "up")
        {
            m_BlockedMovementUp = true;
        }
    }

    /*
     * Flags this object as unblocked in the specified direction
     */
    public void EnableMovement(string dir)
    {
        if (dir == "left")
        {
            m_BlockedMovementLeft = false;
        }
        else if (dir == "right")
        {
            m_BlockedMovementRight = false;
        }
        else if (dir == "up")
        {
            m_BlockedMovementUp = false;
        }
    }

    /*
     * Returns whether this object is blocked in the specified direction
     */
    public bool GetBlockedMovement(string dir)
    {
        if (dir == "up")
        {
            return m_BlockedMovementUp;
        }
        if (dir == "left")
        {
            return m_BlockedMovementLeft;
        }
        if (dir == "right")
        {
            return m_BlockedMovementRight;
        }

        return false;
    }

}
