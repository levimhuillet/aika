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
public interface ICarriable
{
    /*
     * Unity Callback
     * FixedUpdate is called once per frame during physics calculations
     * Carrier updates whether its carrier has left it
     */
    void Update();

    /*
     * Sets this aika block's carrier
     */
    void SetCarrier(GameObject newCarrier);

    /*
     * Returns this aika block's carrier
     */
    GameObject GetCarrier();

}
