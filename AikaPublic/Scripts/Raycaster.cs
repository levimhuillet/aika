using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A Racyaster often works in tandem with a Raycaster. It is responsible for detecting collisions.
 * In comparison, the Raybody is responsible for the movement side of things.
 * For example, an object that needs to handle collisions but never moves (such as a Key) only needs a Raycaster.
 * Together these are my alternative to using Unity's rigidbodies, which were causing me issues.
 */
[RequireComponent(typeof(BoxCollider2D))]
public class Raycaster : MonoBehaviour, IRaycaster
{
    protected BoxCollider2D m_BC; // this object's box collider

    // ---------- Unity Callbacks ----------

    // TODO: does this need to be called from each child class?
    /*
     * Unity Callback
     */ 
    public void Awake()
    {
        m_BC = GetComponent<BoxCollider2D>();
    }

    // ---------- IRaycasterMethods ----------

    /*
     * Returns true if this is touching the given layer, false otherwise.
     */
    public bool IsTouching(string layerMask)
    {
        LayerMask mask = LayerMask.GetMask(layerMask);

        return IsTouching_helper(mask);
    }

    /*
     * Returns true if this is touching any of the given layers, false otherwise.
     */
    public bool IsTouching(string[] layerMasks)
    {
        LayerMask mask = LayerMask.GetMask(layerMasks);

        return IsTouching_helper(mask);
    }

    /*
     * Helper method for both overloaded versions of IsTouching() to consolidate code
     */ 
    bool IsTouching_helper(LayerMask mask)
    {
        RaycastHit2D leftHit = CastLeft(mask, 0f);
        RaycastHit2D rightHit = CastRight(mask, 0f);
        RaycastHit2D upHit = CastAbove(mask, 0f);
        RaycastHit2D downHit = CastBelow(mask, 0f);

        return leftHit.collider != null || rightHit.collider != null || upHit.collider != null || downHit.collider != null;
    }

    /*
     * Checks for an overlap with an object of the given layermask in every direction
     * Returns the object instance this is touching of the given layer
     */
    public Collider2D InstanceTouching(string layerMask)
    {
        LayerMask mask = LayerMask.GetMask(layerMask);

        // Check left
        RaycastHit2D leftHit = CastLeft(mask, 0f);
        if (leftHit.collider != null)
        {
            return leftHit.collider;
        }

        // Check right
        RaycastHit2D rightHit = CastRight(mask, 0f);
        if (rightHit.collider != null)
        {
            return rightHit.collider;
        }

        // Check up
        RaycastHit2D upHit = CastAbove(mask, 0f);
        if (upHit.collider != null)
        {
            return upHit.collider;
        }

        // Check down
        RaycastHit2D downHit = CastBelow(mask, 0f);
        if (downHit.collider != null)
        {
            return downHit.collider;
        }

        return null; // no overlaping instance was detected
    }

    /*
     * Casts a ray from the top-left of this object's collider to the right,
     * covering the entire top of this object.
     */
    public RaycastHit2D CastAbove(LayerMask mask, float collisionBuffer)
    {
        return Physics2D.Raycast(
               new Vector2(m_BC.bounds.center.x - m_BC.bounds.extents.x, m_BC.bounds.center.y + m_BC.bounds.extents.y + collisionBuffer),
               Vector2.right,
               2 * m_BC.bounds.extents.x,
               mask
               );
    }

    /*
     * Casts a ray from the top-left of this object's collider downward,
     * covering the entire left side of this object.
     */
    public RaycastHit2D CastLeft(LayerMask mask, float collisionBuffer)
    {
        return Physics2D.Raycast(
            new Vector2(m_BC.bounds.center.x - m_BC.bounds.extents.x - collisionBuffer, m_BC.bounds.center.y + m_BC.bounds.extents.y),
            Vector2.down,
            2 * m_BC.bounds.extents.y,
            mask
            );
    }

    /*
     * Casts a ray from the top-right of this object's collider downward,
     * covering the entire right side of this object.
     */
    public RaycastHit2D CastRight(LayerMask mask, float collisionBuffer)
    {
        return Physics2D.Raycast(
                new Vector2(m_BC.bounds.center.x + m_BC.bounds.extents.x + collisionBuffer, m_BC.bounds.center.y + m_BC.bounds.extents.y),
                Vector2.down,
                2 * m_BC.bounds.extents.y,
                mask
                );
    }

    /*
     * Casts a ray from the bottom-left of this object's collider to the right,
     * covering the entire underside of this object.
     */
    public RaycastHit2D CastBelow(LayerMask mask, float collisionBuffer)
    {
        return Physics2D.Raycast(
                new Vector2(m_BC.bounds.center.x - m_BC.bounds.extents.x, m_BC.bounds.center.y - m_BC.bounds.extents.y - collisionBuffer), // origin
                Vector2.right, // direction
                2 * m_BC.bounds.extents.x, // magnitude
                mask // layer mask
                );
    }

}
