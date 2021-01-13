using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * (This at the head of each relevant interface: ICarrier, ICarriable, and IParentModifiable)
 * Carriers hold one thing, which stays directly above them.
 * Carriables are the one thing held.
 * ParentModifiables are held by what I think of as Supports (though there is no explicit class for this).
 * Supports hold many things, which keep their offset when they land.
 */
public interface ICarrier
{
    /*
     * Called when a new carried object is first assigned.
     * Transports the block being carried to directly above the carrier.
     * This avoids pushing carrier up when the block is underneath if using MovePosition().
     */
    void PopToPosition(GameObject objCarrying);

    /*
     * Holds the carried object above this carrier using MovePosition();
     * Process for holding objects above: Move the physical object in FixedUpdate, then bring any children to its relative position
     */
    void Carry(GameObject objCarrying);

    /*
     * Sets the object the carrier is carrying
     */
    void SetObjectCarrying(GameObject carryingObj);

    /*
     * Returns the object the carrier is carrying
     */
    GameObject GetObjectCarrying();
}
