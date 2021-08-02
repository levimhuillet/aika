using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Entries;

/*
 * An object that unlocks the exit when an aika passes through.
 */
public class Key : MonoBehaviour
{
    public Exit exit; // the exit this key unlocks

    /*
     * Unity Callback
     */
    protected void OnTriggerEnter2D(Collider2D other)
    {
        // Key has been grabbed successfully, so the exit unlocks one lock
        exit.ObtainedKey();

        // Having been obtained, this key disappears
        this.gameObject.SetActive(false);
    }
}
