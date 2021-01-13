using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Tracks which levels have been completed
 */
public class GameStateManager : MonoBehaviour
{
    public static GameStateManager instance;

    private int m_HighestLevelCompleted; // only one int is needed since players cannot skip levels. If the player could skip levels, a separate bool would be needed for each level

    /*
     * Unity Callback
     */
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
            m_HighestLevelCompleted = 0;
        }
        else if (this != instance)
        {
            Destroy(this.gameObject);
        }
    }

    /*
     * 
     */
    public void CompleteLevel(int levelNum)
    {
        // Case when the completed level is higher than the highest level completed
        if (levelNum > m_HighestLevelCompleted)
        {
            // Highest level must be updated to track the new highest level
            m_HighestLevelCompleted = levelNum;
        }
    }

    /*
     * 
     */
    public int GetHighestLevel()
    {
        return m_HighestLevelCompleted;
    }
}
