using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The interface for the Raycaster class.
 * A Racyaster often works in tandem with a Raycaster. It is responsible for detecting collisions.
 * In comparison, the Raybody is responsible for the movement side of things.
 * For example, an object that needs to handle collisions but never moves (such as a Key) only needs a Raycaster.
 * Together these are my alternative to using Unity's rigidbodies, which were causing me issues.
 */
public interface IRaycaster
{
    /*
     * Returns true if this is touching the given layer, false otherwise.
     */
    bool IsTouching(string layerMask);

    /*
     * Returns true if this is touching any of the given layers, false otherwise.
     */
    bool IsTouching(string[] layerMasks);

    /*
     * Returns the object instance this is touching of the given layer
     */
    Collider2D InstanceTouching(string layerMask);

    /*
     * Casts a ray from the top-left of this object's collider to the right,
     * covering the entire top of this object.
     */
    RaycastHit2D CastAbove(LayerMask mask, float collisionBuffer);

    /*
     * Casts a ray from the top-left of this object's collider downward,
     * covering the entire left side of this object.
     */
    RaycastHit2D CastLeft(LayerMask mask, float collisionBuffer);

    /*
     * Casts a ray from the top-right of this object's collider downward,
     * covering the entire right side of this object.
     */
    RaycastHit2D CastRight(LayerMask mask, float collisionBuffer);

    /*
     * Casts a ray from the bottom-left of this object's collider to the right,
     * covering the entire underside of this object.
     */
    RaycastHit2D CastBelow(LayerMask mask, float collisionBuffer);

}
