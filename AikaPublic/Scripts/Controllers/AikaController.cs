using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Entries;
using AikaGame.Functionalities;


namespace AikaGame.Controllers
{
    // Much help from https://www.youtube.com/watch?v=MbWK8bCAU2w&list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz&index=2

    [RequireComponent(typeof(BoxCollider2D))]
    [RequireComponent(typeof(PhysicsBody))]
    public class AikaController : MonoBehaviour
    {
        [HideInInspector]
        public PhysicsBody physicsBody;

        public LayerMask portalMask;
        public LayerMask exitMask;
        public LayerMask interactMask;

        [HideInInspector]
        public CrushedInfo crushedInfo;

        public struct CrushedInfo
        {
            public bool above, below;

            public void Reset()
            {
                above = below = false;
            }
        }

        public void Awake()
        {
            physicsBody = this.GetComponent<PhysicsBody>();
        }

        public Vector2 Move(Vector2 velocity)
        {
            crushedInfo.Reset();
            Vector2 finalVelocity = physicsBody.Move(velocity);

            CrushedCollisions();

            return finalVelocity;
        }

        public Vector2 PrecarryMove(Vector2 velocity)
        {
            Vector2 prelimVelocity = physicsBody.PrecarryMove(velocity);

            CrushedCollisions();

            return prelimVelocity;
        }

        public void CrushedCollisions()
        {
            crushedInfo.above = physicsBody.collisions.above;
            crushedInfo.below = physicsBody.collisions.below;
            /*
            float rayLength = (this.GetComponent<BoxCollider2D>().bounds.extents.x * 2) - (physicsBody.raycaster.skinWidth * 4);

            Vector2 rayOrigin = physicsBody.raycaster.raycastOrigins.topLeft + new Vector2(physicsBody.raycaster.skinWidth * 2, 0f);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, physicsBody.raycaster.collisionMask);

            // Debug.DrawRay(rayOrigin, Vector2.right * rayLength, Color.red);

            if (hit)
            {
                crushedInfo.above = true;
            }

            rayOrigin = physicsBody.raycaster.raycastOrigins.bottomLeft + new Vector2(physicsBody.raycaster.skinWidth * 2, 0f);
            RaycastHit2D hitBottom = Physics2D.Raycast(rayOrigin, Vector2.right, rayLength, physicsBody.raycaster.collisionMask + physicsBody.raycaster.verticalOnlyMask);

            // Debug.DrawRay(rayOrigin, Vector2.right * rayLength, Color.red);

            if (hitBottom)
            {
                crushedInfo.below = true;
            }
            */
        }

        public Portal GetPortalOverlap()
        {
            // Portals (only need to check horizontal)
            for (int i = 0; i < physicsBody.raycaster.horizontalRayCount; ++i)
            {
                Vector2 rayOrigin = physicsBody.raycaster.raycastOrigins.bottomCenter;
                rayOrigin += Vector2.up * (physicsBody.raycaster.horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, physicsBody.raycaster.boxCollider.bounds.extents.x, portalMask);


                if (hit)
                {
                    return hit.collider.gameObject.GetComponent<Portal>();
                }
                else
                {
                    RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, physicsBody.raycaster.boxCollider.bounds.extents.x, portalMask);
                    if (hitLeft)
                    {
                        return hitLeft.collider.gameObject.GetComponent<Portal>();
                    }
                }
            }

            return null;
        }

        public bool IsOverlappingExit()
        {
            // Exits (only need to check horizontal
            for (int i = 0; i < physicsBody.raycaster.horizontalRayCount; ++i)
            {
                Vector2 rayOrigin = physicsBody.raycaster.raycastOrigins.bottomCenter;
                rayOrigin += Vector2.up * (physicsBody.raycaster.horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, physicsBody.raycaster.boxCollider.bounds.extents.x, exitMask);

                if (hit)
                {
                    return true;
                }
                else
                {
                    RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, physicsBody.raycaster.boxCollider.bounds.extents.x, exitMask);
                    if (hitLeft)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public GameObject GetInteractOverlap()
        {
            // Portals (only need to check horizontal)
            for (int i = 0; i < physicsBody.raycaster.horizontalRayCount; ++i)
            {
                Vector2 rayOrigin = physicsBody.raycaster.raycastOrigins.bottomCenter;
                rayOrigin += Vector2.up * (physicsBody.raycaster.horizontalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right, physicsBody.raycaster.boxCollider.bounds.extents.x, interactMask);

                // Debug.DrawRay(rayOrigin, Vector2.right * physicsBody.raycaster.boxCollider.bounds.extents.x * 100, Color.red);

                if (hit)
                {
                    return hit.collider.gameObject;
                }
                else
                {
                    RaycastHit2D hitLeft = Physics2D.Raycast(rayOrigin, Vector2.left, physicsBody.raycaster.boxCollider.bounds.extents.x, interactMask);
                    Debug.DrawRay(rayOrigin, Vector2.left * physicsBody.raycaster.boxCollider.bounds.extents.x * 100, Color.red);
                    if (hitLeft)
                    {
                        return hitLeft.collider.gameObject;
                    }
                }
            }

            return null;
        }

        public GameObject GetInteractUnder()
        {
            // Portals (only need to check horizontal)
            for (int i = 0; i < physicsBody.raycaster.verticalRayCount; ++i)
            {
                Vector2 rayOrigin = physicsBody.raycaster.raycastOrigins.bottomLeft;
                rayOrigin += Vector2.right * (physicsBody.raycaster.verticalRaySpacing * i);
                RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.down, physicsBody.raycaster.skinWidth, interactMask);

                // Debug.DrawRay(rayOrigin, Vector2.down * physicsBody.raycaster.skinWidth * 100, Color.blue);

                if (hit)
                {
                    return hit.collider.gameObject;
                }
            }

            return null;
        }
    }
}