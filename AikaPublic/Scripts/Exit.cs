using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The portal through which an aika reloads the level (without a key)
 * or proceeds to the next level (with a key)
 */ 
public class Exit : MonoBehaviour
{
    public int numKeys; // the number of keys needed to unlock  this exit
    int m_ResetNumKeys; // stores the value of numKeys at the start (in prep for Reload() function)
    bool m_IsUnlocked; // whether this exit is unlocked
    Vector2 m_StartPos; // stores the starting position of this exit (in prep for Reload() function and player instantiation)
    SpriteRenderer m_SR; // this exit's sprite renderer
    private Sprite[] m_Sprites; // this exit's sprite resources

    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback
     */
    void Awake()
    {
        m_ResetNumKeys = numKeys;
        m_IsUnlocked = false;
        m_StartPos = new Vector2(transform.position.x, transform.position.y);
        m_SR = GetComponent<SpriteRenderer>();
        m_Sprites = Resources.LoadAll<Sprite>("Sprites/Environment/Temp/Exit");
        m_SR.sprite = m_Sprites[1]; // m_Sprites[1] is the locked sprite
    }

    /*
     * Unity Callback
     */
    protected void OnTriggerEnter2D(Collider2D other)
    {
        // Case when the aika is overlapping the exit
        if (other.gameObject.tag == "Aika")
        {
            // The aika may attempt to exit
            other.gameObject.GetComponent<Aika>().SetIsOverExit(true); // enables checks for exiting conditions
        }
    }

    /*
     * Unity Callback
     */
    protected void OnTriggerExit2D(Collider2D other)
    {
        // Case when the aika is overlapping the exit
        if (other.gameObject.tag == "Aika")
        {
            // The aika may no longer attempt to exit
            other.gameObject.GetComponent<Aika>().SetIsOverExit(false); // disbales checks for exiting conditions
        }
    }

    // ---------- External Signals ----------

    /*
     * Unlocks a lock on this exit
     */
    public void ObtainedKey()
    {
        numKeys -= 1; // a key has been used to unlock this exit

        // Case if all keys have been obtained
        if (numKeys == 0)
        {
            // Unlock this door
            Unlock();
        }
    }

    // ---------- Member Functions ----------

    /*
     * Unlocks this exit
     */
    void Unlock()
    {
        m_IsUnlocked = true;
        m_SR.sprite = m_Sprites[0]; // m_Sprites[0] is the unlocked sprite
    }

    // ---------- Getters and Setters ----------

    /*
     * Returns whether this exit is unlocked
     */
    public bool GetIsUnlocked()
    {
        return m_IsUnlocked;
    }

    // ---------- Scene Management ----------

    /*
     * Returns the start position of this exit (Used by aika to spawn)
     */ 
    public Vector2 GetStartPosition()
    {
        return m_StartPos;
    }
}
