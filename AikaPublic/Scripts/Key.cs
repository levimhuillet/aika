using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * An object that unlocks the exit when an aika passes through.
 */
[RequireComponent(typeof(Raycaster))]
public class Key : MonoBehaviour, IRaycaster
{
    private Raycaster _raycaster; // an instance of the Raycaster class

    public Exit exit; // the exit this key unlocks

    private string[] m_AikaMasks; // a list of tags for identifying an Aika object


    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback
     */
    void Awake()
    {
        _raycaster = this.GetComponent<Raycaster>();

        m_AikaMasks = new string[2];
        m_AikaMasks[0] = "Player"; // a Player is an Aika
        m_AikaMasks[1] = "Replicate"; // a Replicate is an Aika
    }

    /*
     * Unity Callback
     */
    void Update()
    {
        // Case when other object is an aika
        if (IsTouching(m_AikaMasks))
        {
            // Key has been grabbed successfully, so the exit unlocks one lock
            exit.ObtainedKey();

            // Having been obtained, this key disappears
            this.gameObject.SetActive(false);
        }
    }

    // ---------- IRaycaster Methods ---------- 

    /*
     * Returns true if this is touching the given layer, false otherwise.
     */
    public bool IsTouching(string layerMask)
    {
        return this._raycaster.IsTouching(layerMask);
    }

    /*
     * Returns true if this is touching any of the given layers, false otherwise.
     */
    public bool IsTouching(string[] layerMasks)
    {
        return this._raycaster.IsTouching(layerMasks);
    }

    /*
     * Returns the object instance this is touching of the given layer
     */
    public Collider2D InstanceTouching(string layerMask)
    {
        return this._raycaster.InstanceTouching(layerMask);
    }

    /*
     * Casts a ray from the top-left of this object's collider to the right,
     * covering the entire top of this object.
     */
    public RaycastHit2D CastAbove(LayerMask mask, float collisionBuffer)
    {
        return _raycaster.CastAbove(mask, collisionBuffer);
    }

    /*
     * Casts a ray from the top-left of this object's collider downward,
     * covering the entire left side of this object.
     */
    public RaycastHit2D CastLeft(LayerMask mask, float collisionBuffer)
    {
        return _raycaster.CastLeft(mask, collisionBuffer);
    }

    /*
     * Casts a ray from the top-right of this object's collider downward,
     * covering the entire right side of this object.
     */
    public RaycastHit2D CastRight(LayerMask mask, float collisionBuffer)
    {
        return _raycaster.CastRight(mask, collisionBuffer);
    }

    /*
     * Casts a ray from the bottom-left of this object's collider to the right,
     * covering the entire underside of this object.
     */
    public RaycastHit2D CastBelow(LayerMask mask, float collisionBuffer)
    {
        return _raycaster.CastBelow(mask, collisionBuffer);
    }

}
