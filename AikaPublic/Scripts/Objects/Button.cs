using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Functionalities;

/*
 * An object that signals to another object (currently only elevator) when to change _states
 */
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(Pausable))]
public class Button : HeightModulator
{
    public List<MovingPlatform> movingPlatforms; // the elevator this button controls
    protected Pausable _pausable;

    float _lowerDistance; // how far the button lowers

    BoxCollider2D _collider;
    RaycastOrigins _raycastOrigins;

    const float _topSkinWidth = .15f;
    const float _sideSkinWidth = 0.1f;
    public int verticalRayCount = 6;

    float _horizontalRaySpacing;
    float _verticalRaySpacing;

    public LayerMask atopMask;

    CollisionInfo collisions;

    struct RaycastOrigins
    {
        public Vector2 topLeft;
    }

    struct CollisionInfo
    {
        public bool above;

        public void Reset()
        {
            above = false;
        }
    }

    void Awake()
    {
        _collider = this.GetComponent<BoxCollider2D>();
        CalculateRaySpacing();
    }

    /*
     * Start is called before the first frame update
     */
    void Start()
    {
        _state = HMStates.max; // a button starts at its max position, then is pushed down
        _maxPos = this.transform.position;
        _lowerDistance = 0.5f; // 1f
        _minPos.y = _maxPos.y - _lowerDistance;
        _speed = new Vector2(0f, 7.5f); // 4

        _pausable = this.GetComponent<Pausable>();
    }

     /*
     * FixedUpdate is called once per frame during physics calculations
     */
    new void Update()
    {
        if (!_pausable.paused)
        {
            // Check if object atop by raycasting
            UpdateRaycastOrigins();
            collisions.Reset();

            Verticalcollisions();

            if (collisions.above)
            {
                _state = HMStates.decreasing; // button pressed down
                foreach (MovingPlatform mp in movingPlatforms)
                {
                    mp.SetState(MovingPlatform.MPStates.increasing); // elevator rises
                }
            }
            else
            {
                // No objects are on the button
                _state = HMStates.increasing; // button rises
                foreach (MovingPlatform mp in movingPlatforms)
                {
                    mp.SetState(MovingPlatform.MPStates.decreasing); // elevator lowers
                }
            }

            // -- Update button height according to its state
            base.Update(); // HeightModulator.FixedUpdate()
        }
    }

    void CalculateRaySpacing()
    {
        Bounds bounds = _collider.bounds;
        //bounds.Expand(_skinWidth * -2);

        verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

        _verticalRaySpacing = (bounds.size.x - 2 * _sideSkinWidth) / (verticalRayCount - 1);
    }

    void Verticalcollisions()
    {
        float rayLength = _topSkinWidth;

        for (int i = 0; i < verticalRayCount; ++i)
        {
            Vector2 rayOrigin = _raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (_verticalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up, rayLength, atopMask);

            // Debug.DrawRay(rayOrigin, Vector2.up * rayLength, Color.red);

            if (hit)
            {
                BoxCollider2D hitCollider = hit.collider.GetComponent<BoxCollider2D>();
                if (hitCollider.bounds.center.y - hitCollider.bounds.extents.y >= (_collider.bounds.center.y + _collider.bounds.extents.y - _topSkinWidth))
                {
                    rayLength = hit.distance;
                    collisions.above = true;

                    break;
                }
            }
        }
    }

    void UpdateRaycastOrigins()
    {
        Bounds bounds = _collider.bounds;
        //bounds.Expand(_skinWidth * -2);

        _raycastOrigins.topLeft = new Vector2(bounds.min.x + _sideSkinWidth, bounds.max.y);
    }
}
