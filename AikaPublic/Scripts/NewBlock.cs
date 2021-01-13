using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A NewBlock is an object an aika can pick up or stand on. NewBlocks stack on each other.
 * The name "NewBlock" comes from the fact that there was once a "Block" object,
 * but this is the improved version. Future updates may rename this to "Block".
 */ 
[RequireComponent(typeof(Carriable))]
[RequireComponent(typeof(ParentModifiable))]
[RequireComponent(typeof(Raycaster))]
[RequireComponent(typeof(Raybody))]
public class NewBlock : MonoBehaviour, IParentModifiable, IRaycaster, IRaybody, ICarriable
{
    private Carriable _carriable; // and instance of the Carriable class
    private ParentModifiable _parentMod; // an instance of the ParentModifiable class
    private Raycaster _raycaster; // an instance of the Raycaster class
    private Raybody _raybody; // an instance of the Raybody class

    public LevelManager lm; // used to query whether other objects are above or below this block

    List<NewBlock> m_BlocksAtop; // a stack of all the blocks atop this AikaBlock

    BoxCollider2D m_BC; // NewBlock's BoxCollider

    protected Button m_ButtonOn; // the button this block lands on, if any

    // Debugging
    bool BLOCKS_ATOP = true; // not sold on the efficiency of the implementation just yet

    // -------- Unity Callbacks --------

    /*
     * Unity Callback
     */
    protected void Awake()
    {
        _carriable = this.GetComponent<Carriable>();
        _parentMod = this.GetComponent<ParentModifiable>();
        _raycaster = this.GetComponent<Raycaster>();
        _raybody = this.GetComponent<Raybody>();

        _parentMod.Awake();
        _raybody.Awake();

        m_BC = GetComponent<BoxCollider2D>();

        if (BLOCKS_ATOP) { m_BlocksAtop = new List<NewBlock>(); }
    }

    /*
     * Unity Callback
     */ 
    public void Update()
    {
        // Raycast for Static Structures
        RaycastStructures();

        // Raycast for Moving Structures
        RaycastMovingStructures();

        // Raycast for Buttons
        RaycastButtons();

        // Raycast for NewBlocks
        RaycastNewBlocks();

        if (_raybody.m_IsSomethingBelowThisTurn)
        {
            _raybody.m_IsGrounded = true;
            // m_Gravity = m_DefaultGravity;
        }
        else
        {
            _raybody.m_IsGrounded = false;
        }
        _raybody.m_IsSomethingBelowThisTurn = false;

        // Apply Gravity

        // Block should fall when it is not grounded and not being carried
        if (!_raybody.m_IsGrounded && GetCarrier() == null)
        {
            Vector2 compareVelocity = _raybody.m_Velocity;

            Vector2 gravityMove = new Vector2(0f, _raybody.m_Gravity * Time.deltaTime);
            _raybody.m_Velocity += gravityMove;

            Move(_raybody.m_Velocity);
        }

        // reset horizontal movement each frame
        _raybody.m_Velocity = new Vector2(0f, _raybody.m_Velocity.y);

        _carriable.Update();
        if (transform.parent != null)
        {
            // _parentMod.BeSupported();
        }
    }

    /*
     * Unity Callback
     */ 
    public void FixedUpdate()
    {
        if (BLOCKS_ATOP)
        {
            if (this.gameObject.name == "NewBlocktest")
            {
                Debug.Log(this.m_BlocksAtop.Count);
            }
        }
    }

    /*
     * Handles all entering collisions as detected by raycasts
     */
    void RayOnCollisionEnter2D(GameObject collisionObj)
    {
        // Case when collision object is a Platform, Elevator, or Wall
        if (collisionObj.tag == "Platform" || collisionObj.tag == "Elevator" || collisionObj.tag == "Wall")
        {
            // Disable sideways movement
            if (lm.IsToLeft(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                DisableMovement("right");
            }
            else if (lm.IsToRight(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                DisableMovement("left");
            }
        }
        
        // Case when block collides with a Platform or Elevator when being carried
        if ((collisionObj.tag == "Platform" || collisionObj.tag == "Elevator") && this.GetCarrier() != null)
        {
            // Case when the structure is above
            if (lm.IsDirectlyAbove(collisionObj, this.gameObject))
            {
                // The aika's movement should be halted when they hit something above with the block they hold
                HaltCarrierUpward();
            }
            // Case when structure is below
            else
            {
                //TODO: determine if this is necessary
                this.SetCarrier(null);
                this.SeverParent();
            }
        }

        // Case when entering object is an elevator
        if (collisionObj.tag == "Elevator")
        {
            // Case when the block has no carrier
            if (this.GetCarrier() == null)
            {
                // Case when block is directly below the elevator
                if (!lm.IsDirectlyAbove(this.gameObject, collisionObj) && !lm.IsToSide(this.gameObject, collisionObj))
                {
                    collisionObj.GetComponent<Elevator>().PopToPosition(this.gameObject);
                    this.SetParent(collisionObj.transform);
                }
            }
            // Case when block is directly above the elevator
            if (lm.IsDirectlyAbove(this.gameObject, collisionObj))
            {
                // collisionObj.GetComponent<Elevator>().BlockArrived(this.gameObject);
                SetParent(collisionObj.transform);
                _raybody.m_Velocity = new Vector2(this._raybody.m_Velocity.x, 0f);
            }
        }
        
        // Case when entering object is a block
        if (collisionObj.tag == "Button")
        {
            // Case when block is above this button
            if (lm.IsDirectlyAbove(this.gameObject, collisionObj))
            {
                collisionObj.GetComponent<Button>().BlockArrived(this.gameObject);
            }
        }

        // Case when another block collides with this block
        if (collisionObj.tag == "NewBlock")
        {
            // Case when the other block entered above this one
            if (lm.IsDirectlyAbove(this.gameObject, collisionObj))
            {
                // TODO: Create a BlockArrived() function
                // If the other block is not being carried by a grounded aika, this Block should carry it

                if (this.GetCarrier() == null)
                {
                    this.SetParent(collisionObj.transform);
                    this.SetCarrier(collisionObj);

                    if (BLOCKS_ATOP) { collisionObj.GetComponent<NewBlock>().AddBlockAtop(this); }
                }
                else
                {
                    Aika transformAika = GetCarrier().gameObject.GetComponent<Aika>();
                    if (transformAika != null)
                    {
                        if (!transformAika.GetIsGrounded())
                        {
                            this.SetParent(collisionObj.transform);
                            this.SetCarrier(collisionObj);

                            if (BLOCKS_ATOP) { collisionObj.GetComponent<NewBlock>().AddBlockAtop(this); }
                        }
                    }
                }
            }
        }

    }

    /*
     * Handles all continuing collisions as detected by raycasts
     */
    void RayOnCollisionStay2D(GameObject collisionObj)
    {
        // Case when collision object is a Platform, Elevator, or Wall
        if (collisionObj.tag == "Platform" || collisionObj.tag == "Elevator" || collisionObj.tag == "Wall")
        {
            // Disable sideways movement
            if (lm.IsToLeft(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                DisableMovement("right");
            }
            else if (lm.IsToRight(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                DisableMovement("left");
            }
        }
        
        // Case when staying object is a block but has left the top
        if (collisionObj.tag == "Elevator" && !lm.IsDirectlyAbove(this.gameObject, collisionObj))
        {
            // Block has left the top but still touches the structure
            collisionObj.GetComponent<Elevator>().BlockLeft(collisionObj);
        }

        // Case when the staying object is a block and the block is not directly above this button
        if (collisionObj.tag == "Button" && !lm.IsDirectlyAbove(this.gameObject, collisionObj))
        {
            // Block has left the top but still touches the structure
            collisionObj.GetComponent<Button>().BlockLeft(this.gameObject);
        }
    }

    /*
     * Handles all exiting collisions as detected by raycasts
     */
    void RayOnCollisionExit2D(GameObject collisionObj)
    {
        // Case when collision object is a Platform, Elevator, or Wall
        if (collisionObj.tag == "Platform" || collisionObj.tag == "Elevator" || collisionObj.tag == "Wall")
        {
            // Disable sideways movement
            if (lm.IsToLeft(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                EnableMovement("right");
            }
            else if (lm.IsToRight(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                EnableMovement("left");
            }
        }

        // Case when collision object is an Aika, Platform, or Elevator
        if (collisionObj.tag == "Aika" || collisionObj.tag == "Platform" || collisionObj.tag == "Elevator")
        {
            // Movement that was blocked because of something above should no longer be blocked
            if (lm.IsAbove(collisionObj, this.gameObject))
            {
                this.EnableCarrierUpward();
            }
        }

        // Case when the block is exiting an elevator
        if (collisionObj.tag == "Elevator")
        {
            // Case when the block is above this elevator
            if (lm.IsDirectlyAbove(this.gameObject, collisionObj))
            {
                // Block has left from the top
                collisionObj.GetComponent<Elevator>().BlockLeft(collisionObj);
            }
        }

        // Case when the block is exiting a button
        if (collisionObj.tag == "Button")
        {
            // TODO: understand why this is getting called twice

            // Case when block is above this button
            if (lm.IsAbove(this.gameObject, collisionObj))
            {
                collisionObj.GetComponent<Button>().BlockLeaveAbove(this.gameObject);
            }
        }

        // Case when  this block is exiting another block
        if (collisionObj.tag == "NewBlock")
        {
            // Case when the other block entered above this one
            if (lm.IsAbove(collisionObj, this.gameObject))
            {
                // TODO: Create a BlockArrived() function
                // If the other block is not being carried by a grounded aika, this aikaBlock should carry it

                GameObject collisionCarrier = collisionObj.GetComponent<NewBlock>().GetCarrier();

                if (collisionCarrier == this.gameObject.GetComponent<NewBlock>())
                {
                    if (BLOCKS_ATOP) { m_BlocksAtop.Remove(collisionObj.GetComponent<NewBlock>()); }
                }
            }
        }
    }

    // ---------- IParentModifiable Methods ----------

    /*
     * Sets this object's parent to null, and sets this object's relative offset to 0, because there is no parent to have an offset with
     */
    public void SeverParent()
    {
        this._parentMod.SeverParent();
    }

    // ---------- IParentModifiable Getters and Setters ----------

    /*
     * Sets this aika's parent to the given Transform, and establishes the starting offset (to preserve discrepancy between aika's center and parent's center)
     */
    public void SetParent(Transform tf)
    {
        this._parentMod.SetParent(tf);
    }

    /*
     * Sets this object's offset from its parent
     */
    public void SetOffset(float offset)
    {
        this._parentMod.SetOffset(offset);
    }

    /*
     * Return this object's offset from its parent
     */
    public float GetOffset()
    {
        return this._parentMod.GetOffset();
    }

    /*
     * Flags this object as blocked in the specified direction
     */
    public void DisableMovement(string dir)
    {
        this._parentMod.DisableMovement(dir);
    }

    /*
     * Flags this object as unblocked in the specified direction
     */
    public void EnableMovement(string dir)
    {
        this._parentMod.EnableMovement(dir);
    }

    /*
     * Returns whether this object is blocked in the specified direction
     */
    public bool GetBlockedMovement(string dir)
    {
        return this._parentMod.GetBlockedMovement(dir);
    }

    /*
     * Keeps object on top of its parent
     */
    public void BeSupported()
    {
        _parentMod.BeSupported();
    }

    // ---------- IRaycaster ---------- 

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

    // ---------- IRaybody ---------- 

    /*
    * Move's the object to its new position after all physics calculations have been performed
    */
    public void Move(Vector2 dir)
    {
        this._raybody.Move(dir);
    }

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "Structures".
     */
    public void RaycastStructures()
    {
        // Raycast Above
        RaycastStructuresAbove();

        // Raycast Sides
        RaycastStructuresSide();

        // Raycast Below
        RaycastStructuresBelow();
    }

    /*
     * Raycasts for objects with the tag "Structures" above this Raybody
     */
    public void RaycastStructuresAbove()
    {

        // check for anything above
        LayerMask mask = LayerMask.GetMask("Structures");

        RaycastHit2D hitAbove = CastAbove(mask, _raybody.m_CollisionBuffer);

        if (hitAbove.collider != null)
        {
            GameObject hitAboveObj = hitAbove.collider.gameObject;

            GameObject thisCarrier = this.GetCarrier();
            if (thisCarrier != null)
            {
                if (thisCarrier.GetComponent<Raybody>().m_Velocity.y > 0)
                {
                    _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);

                    RayOnCollisionEnter2D(hitAboveObj);
                }
                else if (thisCarrier.GetComponent<Raybody>().m_Velocity.y == 0)
                {
                    RayOnCollisionStay2D(hitAboveObj);
                }
                else
                {
                    RayOnCollisionExit2D(hitAboveObj);
                }
            }

        }

    }

    /*
     * Raycasts for objects with the tag "Structures" to both sides of this Raybody
     */
    public void RaycastStructuresSide()
    {
        // check for blocked path left
        LayerMask mask = LayerMask.GetMask("Structures");

        RaycastHit2D hitLeft = CastLeft(mask, _raybody.m_CollisionBuffer);

        if (hitLeft.collider != null)
        {
            GameObject hitLeftObj = hitLeft.collider.gameObject;

            // Case when block moves left
            if (_raybody.m_Velocity.x < 0)
            {
                RayOnCollisionEnter2D(hitLeftObj);

                // Case when aika comes into contact from side or below
                if (!lm.IsDirectlyAbove(this.gameObject, hitLeftObj))
                {
                    _raybody.m_Velocity = new Vector2(0f, _raybody.m_Velocity.y);
                }
            }
            else if (_raybody.m_Velocity.x == 0)
            {
                RayOnCollisionStay2D(hitLeftObj);
            }
            else
            {
                RayOnCollisionExit2D(hitLeftObj);
            }
        }

        // check for blocked path right

        RaycastHit2D hitRight = CastRight(mask, _raybody.m_CollisionBuffer);

        if (hitRight.collider != null)
        {
            GameObject hitRightObj = hitRight.collider.gameObject;

            // Case when Block moves right
            if (_raybody.m_Velocity.x > 0)
            {
                RayOnCollisionEnter2D(hitRightObj);

                // Case when aika comes into contact from side or below
                if (!lm.IsDirectlyAbove(this.gameObject, hitRightObj))
                {
                    _raybody.m_Velocity = new Vector2(0f, _raybody.m_Velocity.y);
                }
            }
            else if (_raybody.m_Velocity.x == 0)
            {
                RayOnCollisionStay2D(hitRightObj);
            }
            else
            {
                RayOnCollisionExit2D(hitRightObj);
            }
        }
        
        if (hitLeft.collider == null)
        {
            if (GetBlockedMovement("left"))
            {
                EnableMovement("left");
            }
        }

        if (hitRight.collider == null)
        {
            if (GetBlockedMovement("right"))
            {
                EnableMovement("right");
            }
        }
        

    }

    /*
     * Raycasts for objects with the tag "Structures" below this Raybody 
     */
    public void RaycastStructuresBelow()
    {
        LayerMask mask = LayerMask.GetMask("Structures");

        RaycastHit2D hitBelow = CastBelow(mask, _raybody.m_CollisionBuffer);

        if (hitBelow.collider != null)
        {
            GameObject hitBelowObj = hitBelow.collider.gameObject;

            if (_raybody.m_Velocity.y < 0)
            {
                // check for landing
                if (lm.CenterIsAbove(this.gameObject, hitBelowObj))
                {
                    _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);
                    _raybody.m_IsSomethingBelowThisTurn = true;

                    RayOnCollisionEnter2D(hitBelowObj);

                    float landingHeight = hitBelow.collider.bounds.center.y + hitBelow.collider.bounds.extents.y + m_BC.bounds.extents.y + _raybody.m_CollisionBuffer;
                    if (this.transform.position.y < landingHeight)
                    {
                        this.transform.position = new Vector2(this.transform.position.x, landingHeight);
                    }
                }
            }
            // Case when aika is supposedly standing on something
            else if (_raybody.m_Velocity.y == 0)
            {
                // check for falling
                if (lm.CenterIsAbove(this.gameObject, hitBelowObj))
                {
                    _raybody.m_IsSomethingBelowThisTurn = true;
                    RayOnCollisionStay2D(hitBelowObj);
                }
            }
            else
            {
                RayOnCollisionExit2D(hitBelowObj);
            }
        }

    }

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "MovingStructures".
     */
    public void RaycastMovingStructures()
    {
        // Raycast Above
        RaycastMovingStructuresAbove();

        // Raycast Sides
        RaycastMovingStructuresSide();

        // Raycast Below
        RaycastMovingStructuresBelow();
    }

    /*
     * Raycasts for objects with the tag "MovingStructures" above this Raybody
     */
    public void RaycastMovingStructuresAbove()
    {
        // check for anything above
        LayerMask mask = LayerMask.GetMask("MovingStructures");

        RaycastHit2D hitAbove = CastAbove(mask, _raybody.m_CollisionBuffer);
        if (hitAbove.collider != null)
        {
            GameObject hitAboveObj = hitAbove.collider.gameObject;

            GameObject thisCarrier = this.GetCarrier();
            if (thisCarrier == null || (thisCarrier != null && thisCarrier.GetComponent<Aika>().GetVelocity().y > 0))
            {
                // hits from below

                _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);

                HeightModulator modulator = hitAboveObj.GetComponent<HeightModulator>();
                float fallOffset = 0;
                if (modulator.GetState() == "lowering")
                {
                    fallOffset = modulator.GetSpeed() * Time.deltaTime;
                }

                this.transform.position = new Vector2(
                    this.transform.position.x,
                    hitAbove.collider.bounds.center.y
                    + hitAbove.collider.bounds.extents.y
                    + m_BC.bounds.extents.y
                    + _raybody.m_CollisionBuffer /*- 2 * fallOffset*/
                    );

                RayOnCollisionEnter2D(hitAboveObj);
            }
            else if (thisCarrier != null && thisCarrier.GetComponent<Aika>().GetVelocity().y == 0)
            {
                if (hitAboveObj.GetComponent<HeightModulator>().GetState() == "lowering")
                {
                    RayOnCollisionEnter2D(hitAboveObj);
                }
                else
                {
                    RayOnCollisionStay2D(hitAboveObj);
                }
            }
            else
            {
                RayOnCollisionExit2D(hitAboveObj);
            }
        }
    }

    /*
     * Raycasts for objects with the tag "MovingStructures" to both sides of this Raybody
     */
    public void RaycastMovingStructuresSide()
    {
        //TODO: compare to Aika
        // Case when Aika moves left
        if (_raybody.m_Velocity.x < 0)
        {
            // check for blocked path left
            LayerMask mask = LayerMask.GetMask("MovingStructures");

            RaycastHit2D hitLeft = CastLeft(mask, _raybody.m_CollisionBuffer);

            if (hitLeft.collider != null)
            {
                GameObject hitLeftObj = hitLeft.collider.gameObject;

                // Case when aika comes into contact from side or below
                if (!lm.IsDirectlyAbove(this.gameObject, hitLeftObj))
                {
                    _raybody.m_Velocity = new Vector2(0f, _raybody.m_Velocity.y);
                }
            }
        }
        // Case when Aika moves right
        else if (_raybody.m_Velocity.x > 0)
        {
            // check for blocked path right
            LayerMask mask = LayerMask.GetMask("MovingStructures");

            RaycastHit2D hitRight = CastRight(mask, _raybody.m_CollisionBuffer);

            if (hitRight.collider != null)
            {
                GameObject hitRightObj = hitRight.collider.gameObject;

                // Case when aika comes into contact from side or below
                if (!lm.IsDirectlyAbove(this.gameObject, hitRightObj))
                {
                    _raybody.m_Velocity = new Vector2(0f, _raybody.m_Velocity.y);
                }
            }
        }
    }

    /*
     * Raycasts for objects with the tag "MovingStructures" below this Raybody
     */
    public void RaycastMovingStructuresBelow()
    {
        // check for landing
        LayerMask mask = LayerMask.GetMask("MovingStructures");

        RaycastHit2D hitBelow = CastBelow(mask, _raybody.m_CollisionBuffer);

        if (hitBelow.collider != null)
        {
            GameObject hitBelowObj = hitBelow.collider.gameObject;

            if (_raybody.m_Velocity.y < 0)
            {
                if (lm.IsDirectlyAbove(this.gameObject, hitBelowObj))
                {
                    _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);
                    _raybody.m_IsSomethingBelowThisTurn = true;

                    this.transform.position = new Vector2(
                        this.transform.position.x,
                        hitBelow.collider.bounds.center.y
                        + hitBelow.collider.bounds.extents.y
                        + m_BC.bounds.extents.y
                        );

                    RayOnCollisionEnter2D(hitBelowObj);
                }

            }
            // Case when aika is supposedly standing on something
            else if (_raybody.m_Velocity.y == 0f)
            {
                // check for falling
                _raybody.m_IsSomethingBelowThisTurn = true;

                RayOnCollisionStay2D(hitBelowObj);
            }
            else
            {
                RayOnCollisionExit2D(hitBelowObj);
            }
        }
    }

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "Button".
     */
    public void RaycastButtons()
    {
        // Raycast Sides
        RaycastButtonsSide();

        // Raycast Below
        RaycastButtonsBelow();
    }

    /*
     * Raycasts for objects with the tag "Button" to both sides of this Raybody
     */
    public void RaycastButtonsSide()
    {
        //TODO: compare to Aika
        // Case when Aika moves left
        if (_raybody.m_Velocity.x < 0)
        {
            // check for blocked path left
            LayerMask mask = LayerMask.GetMask("Button");

            RaycastHit2D hitLeft = CastLeft(mask, _raybody.m_CollisionBuffer);

            if (hitLeft.collider == null)
            {
                if (m_ButtonOn != null)
                {
                    // Case when the staying object is an aika and the aika is not directly above this button
                    if (!lm.IsDirectlyAbove(this.gameObject, m_ButtonOn.gameObject))
                    {
                        // Aika has left the top but still touches the structure
                        m_ButtonOn.BlockLeft(this.gameObject);
                        m_ButtonOn = null;
                    }
                }
            }
        }
        // Case when Aika moves right
        else if (_raybody.m_Velocity.x > 0)
        {
            // check for blocked path right
            LayerMask mask = LayerMask.GetMask("Button");

            RaycastHit2D hitRight = CastRight(mask, _raybody.m_CollisionBuffer);

            if (hitRight.collider == null)
            {
                if (m_ButtonOn != null)
                {
                    // Case when the staying object is an aika and the aika is not directly above this button
                    if (!lm.IsDirectlyAbove(this.gameObject, m_ButtonOn.gameObject))
                    {
                        // Aika has left the top but still touches the structure
                        m_ButtonOn.BlockLeft(this.gameObject);
                        m_ButtonOn = null;
                    }
                }
            }
        }

    }

    /*
     * Raycasts for objects with the tag "Button" below this Raybody
     */
    public void RaycastButtonsBelow()
    {
        LayerMask mask = LayerMask.GetMask("Button");

        RaycastHit2D hitBelow = CastBelow(mask, _raybody.m_CollisionBuffer);

        if (hitBelow.collider != null)
        {
            GameObject hitBelowObj = hitBelow.collider.gameObject;

            if (_raybody.m_Velocity.y < 0)
            {
                // check for landing

                // Case when aika is above this button
                if (lm.IsDirectlyAbove(this.gameObject, hitBelowObj))
                {
                    Button hitButton = hitBelowObj.GetComponent<Button>();
                    hitButton.BlockArrived(this.gameObject);
                    this._raybody.m_Velocity = new Vector2(this._raybody.m_Velocity.x, 0f);

                    m_ButtonOn = hitButton;
                }

                _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);
                _raybody.m_IsSomethingBelowThisTurn = true;

                this.transform.position = new Vector2(
                    this.transform.position.x,
                    hitBelow.collider.bounds.center.y
                    + hitBelow.collider.bounds.extents.y
                    + m_BC.bounds.extents.y
                    );
            }
            // Case when aika is supposedly standing on something
            else if (_raybody.m_Velocity.y == 0f)
            {
                // check for falling
                _raybody.m_IsSomethingBelowThisTurn = true;
            }
            // Case when Aika is rising
            else
            {
                // check for falling
                if (m_ButtonOn != null)
                {
                    // Aika has left from the top
                    m_ButtonOn.BlockLeft(this.gameObject);
                    m_ButtonOn = null;
                }
            }
        }
        else if (m_ButtonOn != null)
        {
            // Aika has left from the top
            m_ButtonOn.BlockLeft(this.gameObject);
            m_ButtonOn = null;
        }

    }

    /*
     * One succinct function which calls each relevant raycasting function below
     * for identifying nearby objects with the tag "NewBlock".
     */
    public void RaycastNewBlocks()
    {
        // Raycast Above
        // RaycastNewBlocksAbove();

        // Raycast Sides
        RaycastNewBlocksSide();

        // Raycast Below
        RaycastNewBlocksBelow();
    }

    /*
     * Raycasts for objects with the tag "NewBlock" above this Raybody.
     * Current implementation only requires sides and below.
     */
    public void RaycastNewBlocksAbove()
    {

    }

    /*
     * Raycasts for objects with the tag "NewBlock" to both sides of this Raybody
     */
    public void RaycastNewBlocksSide()
    {

    }

    /*
     * Raycasts for objects with the tag "NewBlock" below this Raybody
     */
    public void RaycastNewBlocksBelow()
    {
        // check for landing
        LayerMask mask = LayerMask.GetMask("NewBlock");

        RaycastHit2D hitBelow = CastBelow(mask, _raybody.m_CollisionBuffer);

        if (hitBelow.collider != null)
        {
            GameObject hitBelowObj = hitBelow.collider.gameObject;

            if (hitBelowObj != this.gameObject)
            {
                if (_raybody.m_Velocity.y < 0)
                {
                    if (lm.IsDirectlyAbove(this.gameObject, hitBelowObj))
                    {
                        _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);
                        _raybody.m_IsSomethingBelowThisTurn = true;

                        this.transform.position = new Vector2(
                            this.transform.position.x,
                            hitBelow.collider.bounds.center.y
                            + hitBelow.collider.bounds.extents.y
                            + m_BC.bounds.extents.y
                            );

                        RayOnCollisionEnter2D(hitBelowObj);

                        float landingHeight = hitBelow.collider.bounds.center.y + hitBelow.collider.bounds.extents.y + m_BC.bounds.extents.y + _raybody.m_CollisionBuffer;
                        if (this.transform.position.y < landingHeight)
                        {
                            this.transform.position = new Vector2(this.transform.position.x, landingHeight);
                        }
                    }
                }
                // Case when aika is supposedly standing on something
                else if (_raybody.m_Velocity.y == 0f)
                {
                    // check for falling
                    _raybody.m_IsSomethingBelowThisTurn = true;

                    RayOnCollisionStay2D(hitBelowObj);
                }
                else
                {
                    RayOnCollisionExit2D(hitBelowObj);
                }
            }
        }

    }

    /*
     * Returns this Raybody's velocity
     */
    public Vector2 GetVelocity()
    {
        return _raybody.m_Velocity;
    }

    /*
     * Adds a velocity to this Raybody's velocity
     */
    public void AddVelocity(Vector2 velocity)
    {
        _raybody.AddVelocity(velocity);
    }


    // ---------- ICarriable Methods ----------

    /*
     * Sets this aika block's carrier
     */
    public void SetCarrier(GameObject newCarrier)
    {
        /* TODO: maybe work this into blocks atop implementation
        GameObject currCarrier = this.GetCarrier();
        if (currCarrier != null)
        {
            NewBlock currBlock = currCarrier.GetComponent<NewBlock>();
            if (currBlock != null)
            {
                currBlock.RemoveBlockAtop(this.GetComponent<NewBlock>());
            }
        }
        */

        _carriable.SetCarrier(newCarrier);

        if (newCarrier != null)
        {
            _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);
        }
    }

    /*
     * Returns this aika block's carrier
     */
    public GameObject GetCarrier()
    {
        return _carriable.GetCarrier();
    }

    // ---------- Member Functions ----------

    /*
     *  Handles when the aika lands on the top of the block
     */
    public void AikaArrived(GameObject aikaObj)
    {
        // Tie the aika to the block
        aikaObj.GetComponent<Aika>().SetParent(transform); // sets parent with positional offset
        aikaObj.GetComponent<Aika>().Land(); // Let the aika know it landed
    }

    /*
     *  Handles when the aika leaves the top but still touches the block
     */
    public void AikaLeft(GameObject aikaObj)
    {
        // Case when this block was the parent of the aika
        if (aikaObj.transform.parent == this.transform)
        {
            // Sever the tie between this block and the aika who left
            aikaObj.GetComponent<Aika>().SeverParent();
            aikaObj.GetComponent<Aika>().Leave(); // remove the player's grounded status
        }
    }

    /*
     * Halts this block's carrier's upward velocity (usually when they collide with a structure above0
     */
    public void HaltCarrierUpward()
    {
        GameObject thisCarrier = this._carriable.m_Carrier;

        // Case when this block has no carrier
        if (thisCarrier == null)
        {
            return;
        }

        // Case when this block's carrier is an aika
        // TODO: consolidate into Aika, not Player and Replicate
        if (thisCarrier.GetComponent<Player>() != null || thisCarrier.GetComponent<Replicate>() != null)
        {
            // Halt the aika's velocity
            Raybody m_CarrierRb = thisCarrier.gameObject.GetComponent<Raybody>();
            m_CarrierRb.m_Velocity = new Vector2(m_CarrierRb.m_Velocity.x, 0f); // vertical velocity set to 0
            thisCarrier.GetComponent<Aika>().DisableMovement("up");
        }
        // Case when this block's carrier is another block
        else if (thisCarrier.GetComponent<NewBlock>() != null)
        {
            // Recursively call this function to reach the lowermost carrier and halt their velocity
            thisCarrier.GetComponent<NewBlock>().HaltCarrierUpward();
        }
    }

    /*
     * Halts this block's carrier's upward velocity (usually when they collide with a structure above0
     */
    public void EnableCarrierUpward()
    {
        GameObject thisCarrier = this._carriable.m_Carrier;

        // Case when this block has no carrier
        if (thisCarrier == null)
        {
            return;
        }

        // Case when this block's carrier is an aika
        // TODO: consolidate into Aika, not Player and Replicate
        if (thisCarrier.GetComponent<Player>() != null || thisCarrier.GetComponent<Replicate>() != null)
        {
            thisCarrier.GetComponent<Aika>().EnableMovement("up");
        }
        // Case when this block's carrier is another block
        else if (thisCarrier.GetComponent<NewBlock>() != null)
        {
            // Recursively call this function to reach the lowermost carrier and halt their velocity
            thisCarrier.GetComponent<NewBlock>().EnableCarrierUpward();
        }
    }

    /*
     * TODO: Incomplete. To be complete, must work in tandem with Aika's BlockMovementLeft and BlockMovementRight
     */
    public void HaltCarrierSideways()
    {
        GameObject thisCarrier = this._carriable.m_Carrier;

        // Case when this block has no carrier
        if (thisCarrier == null)
        {
            // If this block has no carrier, there is no carrier to halt
            return;
        }

        // Case when this block's carrier is an aika
        if (thisCarrier.GetComponent<Aika>() != null)
        {
            Rigidbody2D m_CarrierRb = thisCarrier.gameObject.GetComponent<Rigidbody2D>(); // for readability

            // abolish sideways velocity; broken because it only does one frame. TODO: Must interact with aika's m_BlockMovement<Dir>'s
            m_CarrierRb.velocity = new Vector2(0f, m_CarrierRb.velocity.y);
        }
    }

    /*
     * Adds a block to the top of this block's list,
     * then does the same for each block already atop this block
     */
    public void AddBlockAtop(NewBlock collBlock)
    {
        m_BlocksAtop.Add(collBlock);

        foreach (NewBlock block in collBlock.GetBlocksAtop())
        {
            m_BlocksAtop.Add(block);
        }

        Transform blockParent = this.transform.parent;

        if (blockParent != null && blockParent.gameObject.tag == "NewBlock")
        {
            blockParent.gameObject.GetComponent<NewBlock>().AddBlockAtop(collBlock);
        }
    }

    /*
     * Removes a block to the top of this block's list,
     * then does the same for each block below this block
     * TODO: doesn't work with stacks
     */
    public void RemoveBlockAtop(NewBlock removeBlock)
    {
        m_BlocksAtop.Remove(removeBlock);

        foreach (NewBlock block in removeBlock.GetBlocksAtop())
        {
            m_BlocksAtop.Remove(block);
        }

        Transform blockParent = this.transform.parent;

        if (blockParent != null && blockParent.gameObject.tag == "NewBlock")
        {
            blockParent.gameObject.GetComponent<NewBlock>().RemoveBlockAtop(removeBlock);
        }
    }

    /* TODO: maybe work this into blocks atop implementation
    public void PopBlock(GameObject blockObj)
    {
        float transportX = this.gameObject.transform.position.x; // the x value of the carrier's position

        float transportY = this.gameObject.transform.position.y // carrier center y
           + this.gameObject.GetComponent<BoxCollider2D>().bounds.extents.y // distance from center of carrier sprite to carrier sprite edge
           + blockObj.GetComponent<BoxCollider2D>().bounds.extents.y; // distance from center of block sprite to block sprite edge

        // ------------ Elevator Modifications
        // Case when object being carried is a Block
        if (blockObj.tag == "NewBlock")
        {
            if (this.gameObject.GetComponent<Aika>() == null)
            {
                // offset is applied when the carrier is anything but an aika
                // transportX -= objCarrying.GetComponent<Block>().GetOffset(); // apply offset
            }

            // Sets the block above this carrier
            blockObj.transform.position = new Vector2(transportX, transportY);
        }

        // ------------ Aika Modifications

        // Case when object being carried is an aika block
        if (blockObj.tag == "NewBlock")
        {
            // Sets the block above this carrier
            blockObj.transform.position = new Vector2(transportX, transportY);
        }
    }
    */

    /*
     * Returns the list of blocks atop this block 
     */
    public List<NewBlock> GetBlocksAtop()
    {
        return m_BlocksAtop;
    }

    /*
     * Returns the box collider of the block at the top of this stack of blocks
     */
    public BoxCollider2D GetTopOfStackBC()
    {
        NewBlock topBlock;
        int numBlocks = m_BlocksAtop.Count;
        if (numBlocks == 0)
        {
            topBlock = this;
        }
        else
        {
            topBlock = m_BlocksAtop[numBlocks - 1];
        }

        return topBlock.GetComponent<BoxCollider2D>();
    }

    /*
     * Divides the stack so that the only the number
     * of blocks that fit under a confined space are picked up.
     */
    public void DivideStack()
    {
        int numBlocks = m_BlocksAtop.Count;
        if (numBlocks == 0)
        {
            return;
        }

        NewBlock nextBlock = m_BlocksAtop[0];

        Vector2 transportPos = new Vector2(nextBlock.gameObject.transform.position.x, this.gameObject.transform.position.y);
        nextBlock.SeverParent();
        nextBlock.transform.position = transportPos;
        //nextBlock.SetParent(this.transform.parent);
    }

    /*
     * Returns the lowest carrier/support/transform. Used for velocity and similar things.
     * TODO: Does not yet consider [[[an aika carrying a block] on a block] being carried by an aika].
     */
    public GameObject GetLowestAika()
    {
        GameObject thisCarrier = this._carriable.m_Carrier;

        if (thisCarrier != null) // NewBlocks do not count as carriers for this function's purposes
        {
            if (thisCarrier.tag == "NewBlock")
            {
                return thisCarrier.GetComponent<NewBlock>().GetLowestAika();
            }
            else
            {
                // TODO: might this be an elevator?
                return thisCarrier;
            }
        }
        else if (this.transform.parent != null && this.transform.parent.gameObject.tag == "NewBlock")
        {
            return this.transform.parent.gameObject.GetComponent<NewBlock>().GetLowestAika();
        }
        else
        {
            return null;
        }
    }
}
