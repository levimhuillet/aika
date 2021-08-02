using AikaGame.Entries;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ReplicateKey : MonoBehaviour
{
    public Exit exit; // the exit this key unlocks
    public static UnityEvent Vanish = new UnityEvent();

    /*
     * Unity Callback
     */
    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<Replicate>() != null)
        {
            // Key has been grabbed successfully, so the exit unlocks one lock
            exit.ObtainedKey();

            // Having been obtained, this key disappears
            this.gameObject.SetActive(false);
        }
        else if (other.GetComponent<Player>() != null)
        {
            // Having been obtained, this key disappears
            this.gameObject.SetActive(false);

            Vanish.Invoke();
        }
    }
}
