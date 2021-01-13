using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Main driver of interaction in the scene.
 * Interacts with objects with the goal of unlocking and returning to the exit.
 * Classes of the Aika are Player and Replicate.
 * 
 * Inherits from Carrier class to carry objects (such as AikaBlocks)
 * Inherits from ParentModifiable class to maintain steady
 * movement on blocks (and other horizontally-moving structures)
 */
[RequireComponent(typeof(Carrier))]
[RequireComponent(typeof(ParentModifiable))]
[RequireComponent(typeof(Raycaster))]
[RequireComponent(typeof(Raybody))]
public class Aika : MonoBehaviour, IParentModifiable, ICarrier
{
    private Carrier _carrier; // an instance of the Carrier class
    private ParentModifiable _parentMod; // an instance of the ParentModifiable class
    private Raycaster _raycaster; // an instance of the Raycaster class
    protected Raybody _raybody; // an instance of the Raybody class

    // Level Management
    public LevelManager lm; // used to signal to the lm that the level needs to be reloaded in certain scenarios, as well as query about relative object locations

    // Movement
    protected float m_Speed; // m_Speed with which aika moves
    // protected int slowForce; // force applied to slow the aika (when turning, for example)
    protected float m_JumpHeight;
    protected float m_JumpForce; // force applied to send the aika into the air
    protected float m_JumpTime; // the time an Aika is allowed to extend its jump
    protected float m_JumpTimeCounter; // tracks how long the Aika has been jumping
    protected bool m_IsJumping; // tracks whether the Aika is jumping
    protected bool m_HasHorizontalMovement; // whether any horizontal input was received (if not, horizontal momentum will be halted)

    // Relative Positioning
    protected bool m_IsOverExit; // whether the aika overlaps the exit
    protected Portal m_PortalOverlapping; // Used in MetaWorld; the portal the aika is overlapping
    protected GameObject m_ObjectOverlapping; // the object aika is overlapping, which it may interact with
    protected Button m_ButtonOn; // the Button instance this Aika is on, if any
    protected float m_StartX; // x value of the starting location (determined by exit starting location)
    protected float m_StartY; // y value of the starting location (determined by exit starting location)

    // Replication
    protected int m_FrameNum; // tracks which frame to store movement to
    protected bool m_HasFinishedMovement; // whether the aika has reached the end of its movement record

    protected bool m_CrushedAbove; // whether the aika is receiving pressure from above
    protected bool m_CrushedBelow; // whether the aika is receiving pressure from below

    // Physics
    BoxCollider2D m_BC; // aika's BoxCollider

    // Sprites and Animation
    SpriteRenderer m_SR; // aika's sprite renderer

    // DEBUGGING AND DEVELOPMENT
    bool PICKING_UP_ONE = false;
    /*
    bool FINE_TUNED_MOVEMENT = false;
    bool BLOCKS_ATOP = true;
    bool DROP_WHEN_CONFINED = false;
    */


    // -------- Unity Callbacks --------

    /*
     * Unity Callback
     */
    protected void Awake()
    {
        // Initialize member values

        _carrier = this.GetComponent<Carrier>();
        _parentMod = this.GetComponent<ParentModifiable>();
        _raycaster = this.GetComponent<Raycaster>();
        _raybody = this.GetComponent<Raybody>();

        _parentMod.Awake(); // Call ParentMod Awake() function
        _raybody.Awake();

        // Physics
        m_BC = GetComponent<BoxCollider2D>();
        m_Speed = 4.5f;
        m_JumpHeight = 1f; // 0.35f; //4f;
        m_JumpForce = Mathf.Sqrt(2 * m_JumpHeight * -_raybody.m_Gravity);
        m_IsJumping = false;
        m_JumpTime = .35f; // .5f;
        m_HasHorizontalMovement = false;

        m_CrushedAbove = false;
        m_CrushedBelow = false;

        // Interaction 
        m_ObjectOverlapping = null;

        // Location
        m_IsOverExit = false;
        m_StartX = transform.position.x;
        m_StartY = transform.position.y;

        // Movement Record Management
        m_FrameNum = 0;
        m_HasFinishedMovement = false;

        // Visuals
        m_SR = GetComponent<SpriteRenderer>();
    }

    /*
     * Unity Callback
     */
    public void Start()
    {

    }

    /*
     * Unity Callback
     */
    public void Update()
    {
        // Raycast for collisions

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
        if (!_raybody.m_IsGrounded)
        {
            Vector2 compareVelocity = _raybody.m_Velocity;

            Vector2 gravityMove = new Vector2(0f, _raybody.m_Gravity * Time.deltaTime);
            _raybody.m_Velocity += gravityMove;
        }


    }

    /*
     * Unity Callback
     * FixedUpdate is called once per frame in line with physics calculations
     */
    protected void FixedUpdate()
    {

    }

    /*
     * Handles all entering collisions as detected by raycasts
     */
    public void RayOnCollisionEnter2D(GameObject collisionObj)
    {
        if (collisionObj.tag == "Wall")
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

        //-- Case when this aika hits an object surface above, and is standing on a block held by another aika

        // If this aika hits a surface
        if (collisionObj.tag == "Platform" || collisionObj.tag == "Elevator")
        {
            // Case when aika lands above the surface
            if (lm.IsDirectlyAbove(this.gameObject, collisionObj))
            {
                // Case when the structure is a Platform
                if (collisionObj.tag == "Platform")
                {
                    collisionObj.GetComponent<Platform>().AikaArrived(this.gameObject);
                }
                // Case when the structure is an Elevator
                else if (collisionObj.tag == "Elevator")
                {
                    collisionObj.GetComponent<Elevator>().AikaArrived(this.gameObject);
                }
                Land();
            }
            // Case when aika hits from the left
            else if (lm.IsToLeft(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                DisableMovement("right");
                SeverParent();
            }
            // Case when aika hits from the right
            else if (lm.IsToRight(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                DisableMovement("left");
                SeverParent();
            }
            // Case when aika hits from below
            else
            {
                // Case when aika is standing on a block
                if (transform.parent != null && transform.parent.gameObject.tag == "NewBlock")
                {
                    // Prevent the aika's movement
                    transform.parent.gameObject.GetComponent<NewBlock>().HaltCarrierUpward(); // HaltCarrier recurses down the the lowest carrier.
                }

                DisableMovement("up");
            }

        }

        // Case when Aika collides with a NewBlock
        if (collisionObj.tag == "NewBlock")
        {
            // Case when aika entered from any direction but the top
            if (!lm.IsAbove(this.gameObject, collisionObj))
            {
                if (collisionObj != m_ObjectOverlapping)
                {
                    // Track that the aika is now overlapping this block
                    SetObjectOverlapping(collisionObj);
                }
            }
        }
    }

    /*
     * Handles all continuing collisions as detected by raycasts
     */
    public void RayOnCollisionStay2D(GameObject collisionObj)
    {
        if (collisionObj.tag == "Wall")
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

        // Case when aika stays in contact with structure
        if (collisionObj.tag == "Platform" || collisionObj.tag == "Elevator")
        {
            // Case when aika is above this structure
            if (lm.IsDirectlyAbove(this.gameObject, collisionObj))
            {
                if (!lm.IsToSide(this.gameObject, collisionObj))
                {
                    m_CrushedBelow = true;
                }
            }
            // Case when aika is not above this structure
            else
            {
                // Case when aika is below this structure and getting crushed
                if (!lm.IsToSide(this.gameObject, collisionObj) && IsCrushedAbove(collisionObj) && !lm.CenterIsAbove(this.gameObject, collisionObj))
                {
                    // TODO: doesn't track IsToSide perfectly
                    m_CrushedAbove = true;
                }
                // Case when aika may have left the top but still touches the structure on the side or on the bottom in a non-lethal way
                else
                {
                    // Case when the structure is a Platform
                    if (collisionObj.tag == "Platform")
                    {
                        collisionObj.GetComponent<Platform>().AikaLeft(this.gameObject);
                    }
                    // Case when the structure is an Elevator
                    else if (collisionObj.tag == "Elevator")
                    {
                        collisionObj.GetComponent<Elevator>().AikaLeft(this.gameObject);
                    }

                    // Disable sideways movement
                    if (lm.IsToLeft(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
                    {
                        LayerMask mask = LayerMask.GetMask("NewBlock");

                        RaycastHit2D hitBelow = CastBelow(mask, _raybody.m_CollisionBuffer);

                        if (hitBelow.collider != null)
                        {
                            GameObject hitBelowObj = hitBelow.collider.gameObject;

                            if (this.transform.parent == null
                                && hitBelowObj.GetComponent<NewBlock>() != null
                                && hitBelowObj.GetComponent<NewBlock>().GetLowestAika() != null
                                && hitBelowObj.GetComponent<NewBlock>().GetLowestAika().GetComponent<Aika>().GetPreviousVelocity().x < 0)
                            {
                                this.transform.parent = hitBelowObj.transform;
                                EnableMovement("right");
                            }
                            else
                            {
                                DisableMovement("right");
                                SeverParent();
                            }
                        }
                        else
                        {
                            DisableMovement("right");
                            SeverParent();
                        }

                        /*
                        DisableMovement("right");
                        SeverParent();
                        */

                    }
                    else if (lm.IsToRight(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
                    {
                        LayerMask mask = LayerMask.GetMask("NewBlock");

                        RaycastHit2D hitBelow = CastBelow(mask, _raybody.m_CollisionBuffer);

                        if (hitBelow.collider != null)
                        {
                            GameObject hitBelowObj = hitBelow.collider.gameObject;

                            if (this.transform.parent == null
                                && hitBelowObj.GetComponent<NewBlock>() != null
                                && hitBelowObj.GetComponent<NewBlock>().GetLowestAika() != null
                                && hitBelowObj.GetComponent<NewBlock>().GetLowestAika().GetComponent<Aika>().GetPreviousVelocity().x > 0)
                            {
                                this.transform.parent = hitBelowObj.transform;
                                EnableMovement("left");
                            }
                            else
                            {
                                DisableMovement("left");
                                SeverParent();
                            }
                        }
                        else
                        {
                            DisableMovement("left");
                            SeverParent();
                        }

                        /*
                        DisableMovement("left");
                        SeverParent();
                        */
                    }

                    // Update aika to reflect that it left its parent
                    Leave();
                }
            }
        }


        // If this aikas hits a block
        if (collisionObj.tag == "NewBlock")
        {
            // Case when aika lands above the surface
            if (lm.CenterIsAbove(this.gameObject, collisionObj) && collisionObj.GetComponent<NewBlock>().GetLowestAika() == null)
            {
                m_CrushedBelow = true;
            }
            else
            {
                m_CrushedBelow = false;
            }
        }
    }

    /*
     * Handles all exiting collisions as detected by raycasts
     */
    public void RayOnCollisionExit2D(GameObject collisionObj)
    {
        if (collisionObj.tag == "Wall")
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

        // Case when aika exits a structure (Platform or Elevator)
        if (collisionObj.tag == "Platform" || collisionObj.tag == "Elevator")
        {
            // TODO: candidate for removal
            if (!collisionObj.activeSelf || !this.gameObject.activeSelf) { return; }

            // Case when Aika has left from the top
            if (lm.IsDirectlyAbove(this.gameObject, collisionObj))
            {
                // Case when the structure is a Platform
                if (collisionObj.tag == "Platform")
                {
                    collisionObj.GetComponent<Platform>().AikaLeft(this.gameObject);
                }
                // Case when the structure is an Elevator
                else if (collisionObj.tag == "Elevator")
                {
                    collisionObj.GetComponent<Elevator>().AikaLeft(this.gameObject);
                }

                // Update aika to reflect that it left its parent
                Leave();
            }
            else if (lm.CenterIsAbove(collisionObj, this.gameObject))
            {
                // Aika has left from the bottom
                m_CrushedAbove = false;

                EnableMovement("up");
            }

            if (lm.IsToLeft(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                EnableMovement("right");
            }
            else if (lm.IsToRight(this.gameObject.GetComponent<BoxCollider2D>(), collisionObj.GetComponent<BoxCollider2D>()))
            {
                EnableMovement("left");
            }
        }

        // Case when the Aika exits a NewBlock
        if (collisionObj.tag == "NewBlock")
        {
            // Case when the aika is tracking that it overlaps this block
            // (the aika could be overlapping other objects, so this prevents overriding them)
            if (GetObjectOverlapping() == collisionObj)
            {
                // By exiting, the aika is no longer overlapping this object
                SetObjectOverlapping(null);
            }

            // Case when the aika leaves from the top or the side (must consider side also because platform effector is not perfectly 180)
            if (lm.IsDirectlyAbove(this.gameObject, collisionObj) || lm.IsToSide(this.gameObject, collisionObj))
            {
                collisionObj.GetComponent<NewBlock>().AikaLeft(this.gameObject); // Causes the player to lose grounded status if this block was its parent
            }
        }
    }


    // -------- Member functions --------

    /*
     * Moves the Aika left
     */
    protected void MoveLeft()
    {
        Vector2 leftMovement = new Vector2(-1f, 0f) * m_Speed;
        _raybody.m_Velocity += leftMovement;

        // Default sprite is facing right, so it must be flipped when moving left
        m_SR.flipX = true;
    }

    /*
     * Moves the Aika right
     */
    protected void MoveRight()
    {
        Vector2 rightMovement = new Vector2(1f, 0f) * m_Speed;
        _raybody.m_Velocity += rightMovement;

        // Default sprite is facing right
        m_SR.flipX = false;
    }

    /*
     * Causes the aika to jump
     */
    protected void Jump()
    {
        // Aika may only jump from the ground
        if (_raybody.m_IsGrounded)
        {
            Vector2 jumpMovement = new Vector2(0f, m_JumpForce);
            _raybody.m_Velocity += jumpMovement;

            _raybody.m_IsGrounded = false;
            m_IsJumping = true;
            m_JumpTimeCounter = m_JumpTime;
        }

        if (m_IsJumping)
        {
            if (m_JumpTimeCounter > 0)
            {
                Vector2 jumpMovement = new Vector2(0f, m_JumpForce * 3f * Time.deltaTime);
                _raybody.m_Velocity += jumpMovement;
                m_JumpTimeCounter -= Time.deltaTime;
            }
            else
            {
                m_IsJumping = false;
            }
        }
    }

    /*
     * Stops Aika from extending its jump
     */
    protected void EndJumping()
    {
        m_IsJumping = false;
    }

    // ---------- External Signals ----------

    /*
     * Causes Aika to land
     */
    public void Land()
    {
        _raybody.m_IsSomethingBelowThisTurn = true;

        // there is now something exerting pressure from below
        m_CrushedBelow = true;
    }

    /*
     * Called by a parent surface when the aika walks off it
     * TODO: determine if this is necessary
     */
    public void Leave()
    {
        // Case when parent has no new parent
        if (transform.parent == null)
        {
            _raybody.m_IsSomethingBelowThisTurn = false;

            m_CrushedBelow = false;
        }
    }

    /*
     * Aika interacts with the object it is carrying, overlapping, or standing on top of
     */
    public void Interact()
    {
        // Check whether the object is being carried or not
        if (this.GetObjectCarrying() == null)
        {
            // Aika is holding nothing; they may interact freely

            // Aika will try to interact with an object behind itself before checking below
            if (m_ObjectOverlapping != null)
            {
                // Case when the object behind is a block
                if (m_ObjectOverlapping.tag == "NewBlock")
                {
                    // TODO: Check if enough space above to pick up

                    // TODO: make this an inverse mask (all but Aika or block or AikaBlock)

                    BoxCollider2D aikaBC = GetComponent<BoxCollider2D>();
                    BoxCollider2D overlapBC = m_ObjectOverlapping.GetComponent<BoxCollider2D>();
                    BoxCollider2D overlapTopBC = m_ObjectOverlapping.GetComponent<NewBlock>().GetTopOfStackBC();

                    // The following is untested for more than one block
                    // Case when can pick up entire stack
                    if (IsSpaceAbove(aikaBC, overlapBC, overlapTopBC))
                    {
                        PickUpOverlapStack();
                    }
                    // Case when can pick up one block
                    else if (IsSpaceAbove(aikaBC, overlapBC, overlapBC))
                    {
                        if (PICKING_UP_ONE)
                        {
                            PickUpOverlapOne();
                        }
                        else
                        {
                            //PickUpOverlapStack();
                        }
                    }
                }
            }
            // Else check for an object beneath the aika
            else if (transform.parent != null)
            {
                // Case when object beneath is a block
                if (transform.parent.gameObject.tag == "NewBlock")
                {
                    // TODO: Check if enough space above to pick up
                    // TODO: make this an inverse mask (all but Aika or block or AikaBlock)

                    BoxCollider2D aikaBC = GetComponent<BoxCollider2D>();
                    BoxCollider2D beneathBC = transform.parent.gameObject.GetComponent<BoxCollider2D>();
                    BoxCollider2D beneathTopBC = transform.parent.gameObject.GetComponent<NewBlock>().GetTopOfStackBC();

                    // The following is untested for more than one block
                    // Case when can pick up entire stack
                    if (IsSpaceAbove(aikaBC, beneathBC, beneathTopBC))
                    {
                        PickUpUnderneathStack();
                    }
                    // Case when can pick up one block
                    else if (IsSpaceAbove(aikaBC, beneathBC, beneathBC))
                    {
                        if (PICKING_UP_ONE)
                        {
                            PickUpUnderneathOne();
                        }
                        else
                        {
                            // PickUpUnderneathStack();
                        }
                    }
                }
            }
        }
        else
        {
            // Aika is holding something; use it

            GameObject objCarrying = this.GetObjectCarrying();

            // Case when object held is a block
            if (objCarrying.tag == "NewBlock")
            {
                objCarrying.GetComponent<Raybody>().m_Velocity = new Vector2(0f, 0f);

                // The previously carried object is no longer carried by this aika.
                NewBlock blockCarrying = objCarrying.GetComponent<NewBlock>();
                blockCarrying.SeverParent();
                blockCarrying.SetCarrier(null);

                // Drop the block and make room for a new object to be interacted with
                this.SetObjectCarrying(null);
            }
        }
    }

    // ---------- Positional Checks ----------

    /*
     * Calculates whether the aika is currently being crushed above
     */
    public bool IsCrushedAbove(GameObject other)
    {
        // Establish calculation values
        float otherYCenter = other.GetComponent<BoxCollider2D>().bounds.center.y; // the y position of the crushing object's transform
        float otherYExtents = other.GetComponent<BoxCollider2D>().bounds.extents.y; // the distance from the crushing object's center y to its edge

        float aikaYCenter = m_BC.bounds.center.y; // the y position of the aika's transform
        float aikaYExtents = m_BC.bounds.extents.y; // the distance from the aika's center y to its edge

        float crushBuffer = 0; // 0.15f; // the buffer distance objects may crush an aika before it officially gets crushed (use shrink animation instead)
        float parentYOffset = 0; // the vertical distance added to the calculation to compensate for the parent's position (because using transform.parent means this transform is only relative)

        // Case when the object has a Transform
        if (transform.parent != null)
        {
            parentYOffset = transform.parent.transform.position.y;
        }

        // Calculate whether this aika is crushed

        if (otherYCenter - otherYExtents
            <=
            aikaYCenter - parentYOffset + aikaYExtents - crushBuffer)
        {
            // Aika is crushed
            return true;
        }

        // Aika is not crushed
        return false;
    }

    /*
     * If Aika is crushed, crushes the Aika and displays Crushed menu
     */
    protected void UpdateCrush()
    {
        if (m_CrushedAbove && m_CrushedBelow)
        {
            // TODO: Trigger Crushed animation

            this.gameObject.SetActive(false);

            lm.DisplayCrushedMenu();

            m_HasFinishedMovement = true; // the aika will no longer be moving once it is crushed
                                          // TODO: halt all other objects in scene
        }
    }

    /*
     * Checks if there is enough space above this Aika to accomodate a stack of blocks
     * TODO: add comments to this function
     */
    bool IsSpaceAbove(BoxCollider2D carrierBC, BoxCollider2D carriableBC, BoxCollider2D carriableTopBC)
    {
        LayerMask mask = LayerMask.GetMask("Structures");

        float carriableTopY = carriableTopBC.bounds.center.y + carriableTopBC.bounds.extents.y;
        float stackHeight = carriableTopY - (carriableBC.bounds.center.y - carriableBC.bounds.extents.y);
        float stackMaxLeft = Mathf.Min(
            carriableBC.bounds.center.x - carriableBC.bounds.extents.x,
            carriableTopBC.bounds.center.x - carriableTopBC.bounds.extents.x
            );
        float stackLeftExtents = carriableBC.bounds.center.x - stackMaxLeft;

        RaycastHit2D hitLeft = Physics2D.Raycast(
            new Vector2(carrierBC.bounds.center.x - stackLeftExtents, carrierBC.bounds.center.y + carrierBC.bounds.extents.y),
            Vector2.up,
            (stackHeight),
            mask
            );

        if (hitLeft.collider != null)
        {
            return false;
        }

        float stackMaxRight = Mathf.Max(
            carriableBC.bounds.center.x + carriableBC.bounds.extents.x,
            carriableTopBC.bounds.center.x + carriableTopBC.bounds.extents.x
            );
        float stackRightExtents = stackMaxRight - carriableBC.bounds.center.x;

        RaycastHit2D hitRight = Physics2D.Raycast(
            new Vector2(carrierBC.bounds.center.x + stackRightExtents, carrierBC.bounds.center.y + carrierBC.bounds.extents.y),
            Vector2.up,
            (stackHeight),
            mask
            );

        if (hitRight.collider != null)
        {
            return false;
        }

        return true;
    }

    /*
     * Aika picks up a stack of blocks it is overlapping
     */
    public void PickUpOverlapStack()
    {
        // Pick up the block
        SetObjectCarrying(m_ObjectOverlapping);

        // Assign this object as the object's carrier (which is important to determine whether the object is above its carrier in the object's (fixed) update function)
        NewBlock blockOverlapping = m_ObjectOverlapping.GetComponent<NewBlock>();
        blockOverlapping.SetCarrier(this.gameObject);

        // Pop object above the aika
        PopToPosition(m_ObjectOverlapping);

        // Sever the physical block from any parent surface it may have lay on
        blockOverlapping.SeverParent();

        // Since the aika is now carrying the object, it cannot be overlapping that object.
        m_ObjectOverlapping = null;
    }

    /*
     * Aika picks up one block is it overlapping
     */
    public void PickUpOverlapOne()
    {

        // Pick up the block
        SetObjectCarrying(m_ObjectOverlapping);

        // Assign this object as the object's carrier (which is important to determine whether the object is above its carrier in the object's (fixed) update function)
        NewBlock blockOverlapping = m_ObjectOverlapping.GetComponent<NewBlock>();
        blockOverlapping.SetCarrier(this.gameObject);
        blockOverlapping.DivideStack();

        // Pop object above the aika
        PopToPosition(m_ObjectOverlapping);

        // Sever the physical block from any parent surface it may have lay on
        SeverParent();

        // Since the aika is now carrying the object, it cannot be overlapping that object.
        m_ObjectOverlapping = null;
    }

    /*
     * Aika picks up a stack of blocks beneath itself
     */
    public void PickUpUnderneathStack()
    {
        // Disable jumping after picking up a block the aika stood on
        _raybody.m_IsSomethingBelowThisTurn = false;

        // Pick up the object
        SetObjectCarrying(this.transform.parent.gameObject);

        // Since the aika is now carrying the object, the aika is no longer on top
        this.SeverParent();
        m_CrushedBelow = false;

        // Assign this object as the object's carrier
        GameObject objCarrying = GetObjectCarrying();
        NewBlock blockCarrying = objCarrying.GetComponent<NewBlock>();
        blockCarrying.SetCarrier(this.gameObject);

        // Pop object above the aika
        PopToPosition(objCarrying);

        // Retrieve the physical block (for readability)

        // Sever it from any transforms it may have had on a platform
        blockCarrying.SeverParent();
    }

    /*
     * Aika picks up one block beneath itself
     */
    public void PickUpUnderneathOne()
    {

        // Disable jumping after picking up a block the aika stood on
        _raybody.m_IsSomethingBelowThisTurn = false;

        // Pick up the object
        SetObjectCarrying(this.transform.parent.gameObject);

        // Since the aika is now carrying the object, the aika is no longer on top
        this.SeverParent();
        m_CrushedBelow = false;

        // Assign this object as the object's carrier
        GameObject objCarrying = GetObjectCarrying();
        NewBlock blockCarrying = objCarrying.GetComponent<NewBlock>();
        blockCarrying.SetCarrier(this.gameObject);
        blockCarrying.DivideStack();

        // Pop object above the aika
        PopToPosition(objCarrying);

        // Sever it from any transforms it may have had on a platform
        SeverParent();
    }

    protected void ApplyUpdate()
    {

        // Horizontal
        Move(_raybody.m_Velocity);

        GameObject objCarrying = GetObjectCarrying();
        if (objCarrying != null)
        {
            if (objCarrying.tag == "NewBlock")
            {
                //GetObjectCarrying().GetComponent<Raybody>().Move(_raybody.m_Velocity);

                Carry(objCarrying);
            }
        }

        _raybody.m_PreviousVelocity = _raybody.m_Velocity;

        // reset horizontal movement each frame
        _raybody.m_Velocity = new Vector2(0f, _raybody.m_Velocity.y);

        // --------- Interactions

        UpdateExitContact(); // TODO: only if in level
        UpdatePortalContact(); // TODO: only if in metaworld
        UpdateCrush(); // TODO: only if in level
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

            bool lowestAikaMovingUp = false;
            Transform thisParent = this.transform.parent;
            if (thisParent != null)
            {
                if (thisParent.gameObject.tag == "NewBlock")
                {
                    GameObject thisParentLowestAika = thisParent.GetComponent<NewBlock>().GetLowestAika();
                    if (thisParentLowestAika != null)
                    {
                        lowestAikaMovingUp = thisParentLowestAika.GetComponent<Aika>().GetVelocity().y > 0;
                    }
                }
            }

            if (lm.IsDirectlyAbove(hitAboveObj, this.gameObject))
            {
                if (this._raybody.m_Velocity.y > 0 || lowestAikaMovingUp)
                {
                    _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);
                    DisableMovement("up");

                    RayOnCollisionEnter2D(hitAboveObj);
                }
                else if (_raybody.m_Velocity.y <= 0 || lowestAikaMovingUp)
                {
                    RayOnCollisionExit2D(hitAboveObj);
                    EnableMovement("up");
                }
                else
                {
                    _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);
                    RayOnCollisionStay2D(hitAboveObj);
                    DisableMovement("up");
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
            if (!lm.IsDirectlyAbove(this.gameObject, hitLeftObj) && !lm.IsDirectlyAbove(hitLeftObj, this.gameObject))
            {
                // Case when Aika moves left
                if (_raybody.m_Velocity.x < 0)
                {
                    RayOnCollisionEnter2D(hitLeftObj);

                    _raybody.m_Velocity = new Vector2(0f, _raybody.m_Velocity.y);
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
        }

        // check for blocked path right

        RaycastHit2D hitRight = CastRight(mask, _raybody.m_CollisionBuffer);

        if (hitRight.collider != null)
        {
            GameObject hitRightObj = hitRight.collider.gameObject;
            if (!lm.IsDirectlyAbove(this.gameObject, hitRightObj))
            {
                // Case when Aika moves right
                if (_raybody.m_Velocity.x > 0)
                {
                    RayOnCollisionEnter2D(hitRightObj);

                    _raybody.m_Velocity = new Vector2(0f, _raybody.m_Velocity.y);
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
            if (_raybody.m_Velocity.y > 0)
            {
                // hits from below

                _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);

                float fallOffset = 0;
                HeightModulator modulator = hitAboveObj.GetComponent<HeightModulator>();
                if (modulator.GetState() == "lowering")
                {
                    fallOffset = modulator.GetSpeed() * Time.deltaTime;
                }

                this.transform.position = new Vector2(
                    this.transform.position.x,
                    hitAbove.collider.bounds.center.y
                    - hitAbove.collider.bounds.extents.y
                    - m_BC.bounds.extents.y
                    - _raybody.m_CollisionBuffer
                    - 2 * fallOffset
                    );

                RayOnCollisionEnter2D(hitAboveObj);
                DisableMovement("up");
            }
            else if (_raybody.m_Velocity.y == 0)
            {
                RayOnCollisionStay2D(hitAboveObj);
                DisableMovement("up");
            }
            else
            {
                RayOnCollisionExit2D(hitAboveObj);
                EnableMovement("up");
            }
        }
    }

    /*
     * Raycasts for objects with the tag "MovingStructures" to both sides of this Raybody
     */
    public void RaycastMovingStructuresSide()
    {
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
                if (lm.CenterIsAbove(this.gameObject, hitBelowObj))
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
                        m_ButtonOn.AikaLeft(this.gameObject);
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
                        m_ButtonOn.AikaLeft(this.gameObject);
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
                if (lm.CenterIsAbove(this.gameObject, hitBelowObj))
                {
                    Button hitButton = hitBelowObj.GetComponent<Button>();
                    hitButton.AikaArrived(this.gameObject);
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
                    m_ButtonOn.AikaLeft(this.gameObject);
                    m_ButtonOn = null;
                }
            }
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
     * Raycasts for objects with the tag "NewBlock" above this Raybody
     * Current implementation only requires sides and below.
     */
    public void RaycastNewBlocksAbove()
    {

    }

    /*
     * Raycasts for objects with the tag "NewBlock" to both sides of this Raybody
     * Specific implementation is always coded within the individual scripts of objects with Raybodies
     */
    public void RaycastNewBlocksSide()
    {
        // check for blocked path left
        LayerMask mask = LayerMask.GetMask("NewBlock");

        RaycastHit2D hitLeft = CastLeft(mask, _raybody.m_CollisionBuffer);

        if (hitLeft.collider != null)
        {
            GameObject hitLeftObj = hitLeft.collider.gameObject;

            if (!lm.IsAbove(this.gameObject, hitLeftObj) && hitLeftObj != this._carrier.GetObjectCarrying())
            {
                if (m_ObjectOverlapping == null)
                {
                    m_ObjectOverlapping = hitLeftObj;
                }
            }
        }

        // check for blocked path right
        RaycastHit2D hitRight = CastRight(mask, _raybody.m_CollisionBuffer);

        if (hitRight.collider != null)
        {
            GameObject hitRightObj = hitRight.collider.gameObject;

            if (!lm.IsAbove(this.gameObject, hitRightObj) && hitRightObj != this._carrier.GetObjectCarrying())
            {
                if (m_ObjectOverlapping == null)
                {
                    m_ObjectOverlapping = hitRightObj;
                }
            }
        }

        if (hitLeft.collider == null && hitRight.collider == null)
        {
            // TODO: differentiate between different carried objects
            if (m_ObjectOverlapping != null)
            {
                m_ObjectOverlapping = null;
            }
        }
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
            NewBlock hitBlock = hitBelowObj.GetComponent<NewBlock>();

            bool lowestAikaMovingUp = false;
            GameObject lowestAika = hitBlock.GetLowestAika();
            if (lowestAika != null)
            {
                lowestAikaMovingUp = lowestAika.GetComponent<Aika>().GetVelocity().y > 0;
            }

            if (_raybody.m_Velocity.y < 0 || lowestAikaMovingUp)
            {
                if (lm.IsAbove(this.gameObject, hitBelowObj))
                {
                    _raybody.m_Velocity = new Vector2(_raybody.m_Velocity.x, 0f);
                    _raybody.m_IsSomethingBelowThisTurn = true;
                    this.transform.position = new Vector2(
                        this.transform.position.x,
                        hitBelow.collider.bounds.center.y
                        + hitBelow.collider.bounds.extents.y
                        + m_BC.bounds.extents.y
                        );

                    if (GetBlockedMovement("up") && lowestAikaMovingUp)
                    {
                        hitBlock.HaltCarrierUpward();
                    }

                    // Case when aika lands above the surface
                    if (lm.CenterIsAbove(this.gameObject, hitBelowObj) && lowestAika == null)
                    {
                        m_CrushedBelow = true;
                    }
                    else
                    {
                        m_CrushedBelow = false;
                    }

                    // Track that the aika landed
                    SetParent(hitBelowObj.transform);
                    Land();

                    if (hitBelowObj == m_ObjectOverlapping)
                    {
                        // Track that the aika is now overlapping this block
                        SetObjectOverlapping(null);
                    }

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

                if (GetBlockedMovement("up") && lowestAikaMovingUp)
                {
                    hitBlock.HaltCarrierUpward();
                }

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
     * Returns this Raybody's velocity
     */
    public Vector2 GetVelocity()
    {
        return _raybody.m_Velocity;
    }

    /*
     * Returns this Raybody's velocity last frame
     */
    public Vector2 GetPreviousVelocity()
    {
        return _raybody.m_PreviousVelocity;
    }

    /*
     * Adds a velocity to this Raybody's velocity
     */
    public void AddVelocity(Vector2 velocity)
    {
        _raybody.AddVelocity(velocity);
    }

    // ---------- Additional Member Raycasts ---------- 

    /*
     * Flgas whether this Aika is currently over the exit
     */
    public void UpdateExitContact()
    {
        bool isContact = IsTouching("Exit");

        if (m_IsOverExit && !isContact)
        {
            m_IsOverExit = false;
        }
        else if (!m_IsOverExit && isContact)
        {
            m_IsOverExit = true;
        }
    }

    /*
     * Flags whether this Aika is currently over a portal
     */
    public void UpdatePortalContact()
    {
        if (m_PortalOverlapping == null)
        {
            Collider2D portalTouching = InstanceTouching("Portal");

            if (portalTouching != null)
            {
                m_PortalOverlapping = portalTouching.gameObject.GetComponent<Portal>();
            }
        }
        else if (!IsTouching("Portal"))
        {
            m_PortalOverlapping = null;
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


    // --------- Setters and Getters ------------

    /*
     * Enables/disables checks for exiting conditions.
     * Changed by the Exit object when the aika enters or leaves its trigger collider
     */
    public void SetIsOverExit(bool isOverExit)
    {
        this.m_IsOverExit = isOverExit;
    }

    /*
     * Returns whether the aika overlaps the exit
     */
    public bool GetIsOverExit()
    {
        return m_IsOverExit;
    }

    /*
     * Returns the current frame number
     */
    public int GetFrameNum()
    {
        return m_FrameNum;
    }

    /*
     * Sets the aika's LevelManager
     */
    public void SetLevelManager(LevelManager lm)
    {
        this.lm = lm;
    }

    /*
     * Sets the object the aika is overlapping
     */
    public void SetPortalOverlapping(Portal portal)
    {
        m_PortalOverlapping = portal;
    }

    /*
     * Returns the object the aika is overlapping
     * TODO: Is this ever used?
     */
    public Portal GetPortalOverlapping()
    {
        return m_PortalOverlapping;
    }

    /*
     * Sets the object the aika is overlapping
     */
    public void SetObjectOverlapping(GameObject overlappingObj)
    {
        m_ObjectOverlapping = overlappingObj;
    }

    /*
     * Returns the object the aika is overlapping
     */
    public GameObject GetObjectOverlapping()
    {
        return m_ObjectOverlapping;
    }

    /*
     * Returns whether the aika is grounded
     * TODO: rename
     */
    public bool GetIsGrounded()
    {
        return this._raybody.m_IsSomethingBelowThisTurn;
    }

    /*
     * Returns whether this Aika has any horizontal movement
     * TODO: candidate for removal
     */
    public bool GetHasHorizontalMovement()
    {
        return m_HasHorizontalMovement;
    }

    // ---------- Scene Management ----------

    /*
     * Resets the aika's variables to their initial state
     */
    protected void Reload()
    {
        this.gameObject.SetActive(true);
        SeverParent();
        m_FrameNum = 0;
        transform.position = new Vector2(m_StartX, m_StartY);
        m_HasFinishedMovement = false;
        m_ObjectOverlapping = null;
        this.SetObjectCarrying(null);
        m_CrushedAbove = false;
        m_CrushedBelow = false;
        m_SR.flipX = false;
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
        return this._parentMod.GetBlockedMovement("up");
    }

    /*
     * Keeps object on top of its parent
     * // TODO: candidate for removal
     */
    public void BeSupported()
    {
        _parentMod.BeSupported();
    }

}