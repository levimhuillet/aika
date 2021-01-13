using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A platform that rises and falls as directed by an external object state (e.g. button pressed)
 * Inherits from the ICarrier class for its PopToPosition function,
 * which it uses if it would crush a block
 */
 [RequireComponent(typeof(Carrier))]
public class Elevator : HeightModulator, ICarrier
{
    private Carrier _carrier; // an instance of the Carrier class

    public float riseDistance; // the max distance this elevator will raise when its trigger is active
    public LevelManager lm; // used to query whether other objects are above or below this block

    // ---------- Unity Callbacks ----------

    /*
     * Called when aika is first instantiated
     */
    protected void Awake()
    {
        _carrier = this.GetComponent<Carrier>();
    }

    /*
     * Start is called before the first frame update
     */
    new void Start()
    {
        base.Start();

        m_State = "min"; // an elevator starts at its min position, then is raised up
        m_MinPos = m_BC.bounds.center;
        m_MaxPos.y = m_MinPos.y + riseDistance;
        m_Speed = 1.9f;
    }

    /*
     * FixedUpdate is called once per frame during physics calculations
     */
    new void FixedUpdate()
    {
        // -- Update elevator height according to its state
        base.FixedUpdate();
    }

    // ---------- Member Functions ----------

    /*
     * Handles when the aika leaves this elevator, whether from the top or by walking off
     */
    public void AikaArrived(GameObject aikaObj)
    {
        // Keep player at level with elevator
        aikaObj.GetComponent<Aika>().SetParent(this.transform);
    }

    /*
     * Handles when an aika lands on this elevator
     */
    public void AikaLeft(GameObject aikaObj)
    {
        // Case when the aika was previously on the top
        if (aikaObj.transform.parent == this.transform)
        {
            // No longer record the aika as being on top
            aikaObj.GetComponent<Aika>().SeverParent();
        }

        // Let the aika know it has left
        aikaObj.GetComponent<Aika>().Leave();
    }

    /*
     * Handles when a block lands on this elevator
     * TODO: candidate for removal
     */
    public void BlockArrived(GameObject blockObj)
    {
        /*
        // Keep block at level with elevator
        blockObj.GetComponent<Block>().SetParent(this.transform);

        // TODO: create a Block.Land() function
        // blockObj.gameObject.GetComponent<Block>().Land();
        blockObj.GetComponent<Rigidbody2D>().velocity = new Vector2(blockObj.GetComponent<Rigidbody2D>().velocity.x, 0f);
        */
    }

    /*
     * Handles when a block leaves this elevator
     */
    public void BlockLeft(GameObject blockObj)
    {
        // Case when this elevator was the block's parent
        if (blockObj.transform.parent == this.transform)
        {
            // Sever the block from its parent, since it is no longer on top
            blockObj.GetComponent<NewBlock>().SeverParent();
        }
    }

    // ---------- Getters and Setters ----------

    /*
     * Sets this elevator's state
     */ 
    public void SetState(string m_State)
    {
        this.m_State = m_State;
    }

    // ---------- ICarrier Methods ----------


    /*
     * Implementation of the ICarrier PopToPosition() method
     */
    public void PopToPosition(GameObject objCarrying)
    {
        _carrier.PopToPosition(objCarrying);
    }

    /*
     * Implementation of the ICarrier Carry() method
     */
    public void Carry(GameObject objCarrying)
    {
        _carrier.Carry(objCarrying);
    }

    /*
     * Sets the object the aika is carrying
     */
    public void SetObjectCarrying(GameObject carryingObj)
    {
        _carrier.SetObjectCarrying(carryingObj);
    }

    /*
     * Returns the object the aika is carrying
     */
    public GameObject GetObjectCarrying()
    {
        return _carrier.GetObjectCarrying();
    }
}
