using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

/*
 * A passage that takes the player to a new level
 */ 
public class Portal : MonoBehaviour
{
    public int portalLevel; // the number value of this level
    public string destinationSceneName; // the level this passage takes the player to
    bool m_IsOpen; // whether this portal may be activated by the player

    SpriteRenderer m_SR; // this portal's sprite renderer
    private Sprite[] m_Sprites; // this portal's sprite resources

    /*
     * Unity Callback
     */
    void Awake()
    {
        // Portals start closed
        m_IsOpen = false;

        m_SR = GetComponent<SpriteRenderer>();
        m_Sprites = Resources.LoadAll<Sprite>("Sprites/Environment/Temp/Portal");
        m_SR.sprite = m_Sprites[0]; // m_Sprites[0] is the closed sprite (red)
    }

    // Start is called before the first frame update
    void Start()
    {
        // Case when the highest level completed is the level before this one
        if (GameStateManager.instance.GetHighestLevel() == this.portalLevel - 1)
        {
            // Portals open when their preceding level is completed
            m_IsOpen = true;
            m_SR.sprite = m_Sprites[1]; // m_Sprites[1] is the open sprite for uncompleted levels (green)
        }
        else if (GameStateManager.instance.GetHighestLevel() > this.portalLevel - 1)
        {
            // Portals open when their preceding level is completed
            m_IsOpen = true;
            m_SR.sprite = m_Sprites[2]; // m_Sprites[0] is the open sprite for completed levels (blue)
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    // --------- Member Functions ----------

    /*
     * Called by the player to enter this portal;
     * takes the player to this portal's destination
     */ 
    public void EnterPortal()
    {
        // Case when this portal is open
        if (m_IsOpen)
        {
            // TODO: may need to implement and call a lm.SetNextScene(string), then call lm.NextScene()
            // (would require a link to levelManager, or make lm static instance

            // Load correct level
            SceneManager.LoadScene(destinationSceneName);
        }
    }

    // ---------- Getters and Setters -----------

    /*
     * Returns whether this portal is open
     */ 
    public bool GetIsOpen()
    {
        return m_IsOpen;
    }
}

