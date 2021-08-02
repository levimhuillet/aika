using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using AikaGame.Managers;

namespace AikaGame.Entries
{
    /*
     * A passage that takes the player to a new level
     */
    public class Portal : MonoBehaviour
    {
        public float portalLevel; // the number value of this level
        public string destinationSceneName; // the level this passage takes the player to
        bool _isOpen; // whether this portal may be activated by the player

        SpriteRenderer _sr; // this portal's sprite renderer
        private Sprite[] _sprites; // this portal's sprite resources

        /*
         * Unity Callback
         */
        void Awake()
        {
            // Portals start closed
            _isOpen = false;

            _sr = GetComponent<SpriteRenderer>();
            _sprites = Resources.LoadAll<Sprite>("Sprites/Environment/Temp/Portal");
            _sr.sprite = _sprites[0]; // _sprites[0] is the closed sprite (red)
        }

        // Start is called before the first frame update
        void Start()
        {
            // Case when the highest level completed is the level before this one
            if (GameStateManager.instance.GetHighestLevel() == this.portalLevel - 1)
            {
                // Portals open when their preceding level is completed
                _isOpen = true;
                _sr.sprite = _sprites[1]; // _sprites[1] is the open sprite for uncompleted levels (green)
            }
            else if (GameStateManager.instance.GetHighestLevel() > this.portalLevel - 1)
            {
                // Portals open when their preceding level is completed
                _isOpen = true;
                _sr.sprite = _sprites[2]; // _sprites[0] is the open sprite for completed levels (blue)
            }
        }

        // --------- Member Functions ----------

        /*
         * Called by the player to enter this portal;
         * takes the player to this portal's destination
         */
        public void EnterPortal()
        {
            // Case when this portal is open
            if (_isOpen)
            {
                // Load correct level
                SceneManager.LoadScene(destinationSceneName);
            }
        }
    }
}