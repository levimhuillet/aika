using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AikaGame.Functionalities
{
    [RequireComponent(typeof(Raycaster))]
    [RequireComponent(typeof(Pausable))]
    public class PhysicsBody : MonoBehaviour
    {
        [HideInInspector]
        public Pausable pausable;
        [HideInInspector]
        public Raycaster raycaster;

        [HideInInspector]
        public Vector2 velocity;
        [HideInInspector]
        public float gravity;

        [HideInInspector]
        public CollisionInfo collisions;

        public struct CollisionInfo
        {
            public bool above, below, left, right;

            public void Reset()
            {
                above = below = false;
                left = right = false;
            }
        }

        public void Awake()
        {
            pausable = this.GetComponent<Pausable>();

            raycaster = this.GetComponent<Raycaster>();
            raycaster.Awake();
            raycaster.skinWidth = 0.015f;
        }

        public Vector2 Move(Vector2 velocity)
        {
            raycaster.UpdateRaycastOrigins();
            if (this.GetComponent<Carrier>() == null)
            {
                collisions.Reset();
            }

            HorizontalCollisions(ref velocity);
            VerticalCollisions(ref velocity);

            transform.Translate(velocity);
            return velocity;
        }

        public Vector2 PrecarryMove(Vector2 velocity)
        {
            raycaster.UpdateRaycastOrigins();
            collisions.Reset();

            HorizontalCollisions(ref velocity);
            VerticalCollisions(ref velocity);

            return velocity;
        }

        public void HorizontalCollisions(ref Vector2 velocity)
        {
            float directionX = Mathf.Sign(velocity.x);
            float rayLength = Mathf.Abs(velocity.x) + raycaster.skinWidth;

            // collisions
            for (int i = 0; i < raycaster.horizontalRayCount; ++i)
            {
                Vector2 rayOrigin = (directionX == -1) ? raycaster.raycastOrigins.bottomLeft : raycaster.raycastOrigins.bottomRight;
                rayOrigin += Vector2.up * (raycaster.horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, raycaster.collisionMask);

                // Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength, Color.red);

                if (hit)
                {
                    velocity.x = (hit.distance - raycaster.skinWidth) * directionX;
                    rayLength = hit.distance;

                    if (directionX == 1) { collisions.right = true; }
                    else { collisions.left = true; }
                }
            }
        }

        public void VerticalCollisions(ref Vector2 velocity)
        {
            float directionY = Mathf.Sign(velocity.y);
            float rayLength = Mathf.Abs(velocity.y) + raycaster.skinWidth;

            for (int i = 0; i < raycaster.verticalRayCount; ++i)
            {
                Vector2 rayOrigin = (directionY == -1) ? raycaster.raycastOrigins.bottomLeft : raycaster.raycastOrigins.topLeft;
                rayOrigin += Vector2.right * (raycaster.verticalRaySpacing * i + velocity.x);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, raycaster.collisionMask);

                //Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength, Color.red);

                if (hit)
                {
                    velocity.y = (hit.distance - raycaster.skinWidth) * directionY;
                    rayLength = hit.distance;

                    if (directionY == 1) { collisions.above = true; }
                    else { collisions.below = true; }
                }
            }

            VerticalOnlyCollisions(ref velocity);
        }

        public void VerticalOnlyCollisions(ref Vector2 velocity)
        {
            float directionY = Mathf.Sign(velocity.y);
            if (directionY == 1) { return; }

            float rayLength = Mathf.Abs(velocity.y) + raycaster.skinWidth;
            for (int i = 0; i < raycaster.verticalRayCount; ++i)
            {
                Vector2 rayOrigin = raycaster.raycastOrigins.bottomLeft;// - new Vector2(0f, raycaster.skinWidth);
                rayOrigin += Vector2.right * (raycaster.verticalRaySpacing * i + velocity.x);
                RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.down, rayLength, raycaster.verticalOnlyMask);

                Debug.DrawRay(rayOrigin, Vector2.down * rayLength * 50, Color.blue);

                foreach (RaycastHit2D hit in hits)
                {
                    if (hit && hit.collider.gameObject != this.gameObject)
                    {
                        BoxCollider2D hitCollider = hit.collider.gameObject.GetComponent<BoxCollider2D>();
                        // If the hit object is completely below the physics body
                        if (hitCollider.bounds.center.y + hitCollider.bounds.extents.y <= (raycaster.boxCollider.bounds.center.y - raycaster.boxCollider.bounds.extents.y + raycaster.skinWidth))
                        {
                            velocity.y = (hit.distance - raycaster.skinWidth) * directionY;
                            rayLength = hit.distance;

                            collisions.below = true;
                        }
                        else
                        {
                            //Debug.Log("Out of place");
                        }
                    }
                }
            }
        }
    }
}