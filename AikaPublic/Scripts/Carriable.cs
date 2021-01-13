using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Carriables are objects which can be held by a Carrier.
 */
public class Carriable : MonoBehaviour, ICarriable
{
    public GameObject m_Carrier; // this object's carrier
    public LevelManager lm; // used to query whether other objects are above, below, or to the side of this block

    /*
     * FixedUpdate is called once per frame during physics calculations
     * Carrier updates whether its carrier has left it
     */
    public void Update()
    {
        // Case when the block has a carrier
        // TODO: move to on collision exit
        if (this.m_Carrier != null)
        {
            // Case when carrier has moved out from under what it is carrying
            if (lm.IsToSide(this.m_Carrier, this.gameObject))
            {
                // Carrier has left while carrying the object; this means the carrier slipped in somewhere the block can't, causing separation
                // This should cause the carrier to drop the block

                // Case when the carrier is the player
                if (this.m_Carrier.GetComponent<Player>() != null)
                {
                    // The player is no longer holding this block
                    this.m_Carrier.GetComponent<Player>().SetObjectCarrying(null);
                }
                // Case when the carrier is a replicate
                else if (this.m_Carrier.GetComponent<Replicate>())
                {
                    // The replicate is no longer holding this block
                    this.m_Carrier.GetComponent<Replicate>().SetObjectCarrying(null);
                }

                // This block is no longer being carried
                this.SetCarrier(null);
            }
        }
    }

    /*
     * Sets this aika block's carrier
     */
    public void SetCarrier(GameObject newCarrier)
    {
        // if another aika was carrying this, tie up the loose ends
        // Case when this block currently has another carrier
        if (this.m_Carrier != null)
        {
            // Case when this block's carrier is the player
            if (this.m_Carrier.GetComponent<Player>() != null)
            {
                // Update the previous carrier to no longer be holding this object
                this.m_Carrier.GetComponent<Player>().SetObjectCarrying(null);
            }
            // Case when this block's carrier is a replicate
            else if (this.m_Carrier.GetComponent<Replicate>() != null)
            {
                // Update the previous carrier to no longer be holding this object
                this.m_Carrier.GetComponent<Replicate>().SetObjectCarrying(null);
            }
        }

        // Store the new carrier
        this.m_Carrier = newCarrier;
    }

    /*
     * Returns this aika block's carrier
     */
    public GameObject GetCarrier()
    {
        return m_Carrier;
    }
}
