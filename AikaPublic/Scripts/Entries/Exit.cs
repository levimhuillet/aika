using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AikaGame.Entries
{
    /*
     * The portal through which an aika reloads the level (without a key)
     * or proceeds to the next level (with a key)
     */
    public class Exit : MonoBehaviour
    {
        public bool isUnlocked { get; set; }
        public int numKeys; // the number of keys needed to unlock  this exit
        int _resetNumKeys; // stores the value of numKeys at the start (in prep for Reload() function)
        SpriteRenderer _sr; // this exit's sprite renderer
        Sprite[] _sprites; // this exit's sprite resources

        // ---------- Unity Callbacks ----------

        /*
         * Unity Callback
         */
        void Awake()
        {
            _resetNumKeys = numKeys;
            isUnlocked = false;
            _sr = GetComponent<SpriteRenderer>();
            _sprites = Resources.LoadAll<Sprite>("Sprites/Environment/Temp/Exit");
            _sr.sprite = _sprites[1]; // m_Sprites[1] is the locked sprite
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
            isUnlocked = true;
            _sr.sprite = _sprites[0]; // m_Sprites[0] is the unlocked sprite
        }
    }
}