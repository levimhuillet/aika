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
public interface IParentModifiable
{
    // ---------- Member Functions ----------

    /*
     * Sets this object's parent to null, and sets this object's relative offset to 0, because there is no parent to have an offset with
     */
    void SeverParent();

    /*
     * Keeps object on top of its parent
     */
    void BeSupported();

    // ---------- Getters and Setters ----------

    /*
     * Sets this object's parent to the given Transform, and establishes the starting offset (to preserve discrepancy between aika's center and parent's center)
     */
    void SetParent(Transform tf);

    /*
     * Set this object's offset from its parent
     */
    void SetOffset(float offset);

    /*
     * Return this object's offset from its parent
     */
    float GetOffset();

    /*
     * 
     */
    void DisableMovement(string dir);

    /*
     * 
     */
    void EnableMovement(string dir);

    /*
     * 
     */ 
    bool GetBlockedMovement(string dir);
}
