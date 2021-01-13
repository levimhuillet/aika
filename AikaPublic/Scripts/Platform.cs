using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A Platform is the game's most basic structure.
 * Platforms hang in the air, may be walked on, and prevent objects from passing through.
 */ 
public class Platform : MonoBehaviour
{
    public LevelManager lm; // used to query whether other objects are above or below this block

    // ---------- Aika Interaction ----------

    // TODO: candidate for removal
    /*
     * Handles when the aika leaves the top but still touches the platform
     */
    public void AikaLeft(GameObject aikaObj)
    {
        // Case when the aika was previously on the top
        if (aikaObj.transform.parent == this.transform)
        {
            // No longer record the aika as being on top
            aikaObj.transform.parent = null;

            // Update the aika as one which has left the top (if it has not walked on to another surface, it will no longer be grounded)
            aikaObj.gameObject.GetComponent<Aika>().Leave();
        }
    }

    /*
     * Handles aika landing on this platform
     */ 
    public void AikaArrived(GameObject aikaObj)
    {
        // Case when the aika comes directly from another object

        /*
         * (They need to have their AikaLeft() methods called explicitly because
         * they are still in contact and will not register that the aika no longer carries their transform,
         * which messes up the calculations when the aika finally exists, because transforms have to match sometimes)
         */
        if (aikaObj.transform.parent != null)
        {
            // Case when that object is a button
            if (aikaObj.transform.parent.gameObject.tag == "Button")
            {
                // Explicitly tell the button its aika has left
                aikaObj.transform.parent.gameObject.GetComponent<Button>().AikaLeft(aikaObj);
            }
        }

        // Assign this platform's transform as the aika's parent
        aikaObj.transform.parent = this.transform;
    }
}
