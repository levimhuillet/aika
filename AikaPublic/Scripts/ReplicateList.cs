using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Holds a list of all replicates in a scene
 */ 
public class ReplicateList : MonoBehaviour
{
    public static ReplicateList instance; // 

    public List<GameObject> replicates; // 

    // ---------- Unity Callbacks ---------- 

    /*
     * Unity Callback
     */
    void Awake()
    {
        // Ensure there is only one instance of the ReplicateList
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
        else if (this != instance)
        {
            Destroy(this.gameObject);
        }
    }
}
