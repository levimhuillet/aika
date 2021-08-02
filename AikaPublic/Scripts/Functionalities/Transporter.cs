using AikaGame.Controllers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AikaGame.Functionalities
{
    [RequireComponent(typeof(Raycaster))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class Transporter : MonoBehaviour
    {
        [HideInInspector]
        public Raycaster raycaster;
        public LayerMask transportMask;
        public bool passengerOnly;

        // Start is called before the first frame update
        void Awake()
        {
            raycaster = this.GetComponent<Raycaster>();
            raycaster.Awake();
            raycaster.skinWidth = 0.015f;
        }

        public void Transport(Vector2 velocity)
        {
            HashSet<Transform> transportedObjects = new HashSet<Transform>();

            float directionX = Mathf.Sign(velocity.x);
            float directionY = Mathf.Sign(velocity.y);

            if (!passengerOnly)
            {
                // Vertically-moving platform
                if (velocity.y != 0)
                {
                    float rayLength = Mathf.Abs(velocity.y) + raycaster.skinWidth;

                    for (int i = 0; i < raycaster.verticalRayCount; ++i)
                    {
                        Vector2 rayOrigin = (directionY == -1) ? raycaster.raycastOrigins.bottomLeft : raycaster.raycastOrigins.topLeft;
                        rayOrigin += Vector2.right * (raycaster.verticalRaySpacing * i); // - velocity.x
                        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, transportMask);

                        if (hit)
                        {
                            if (!transportedObjects.Contains(hit.transform))
                            {
                                float pushX = (directionY == 1) ? velocity.x : 0f;
                                float pushY = velocity.y - (hit.distance - raycaster.skinWidth) * directionY;

                                //hit.collider.gameObject.GetComponent<PhysicsBody>().Move(new Vector2(pushX, pushY));
                                AikaController hitController = hit.collider.gameObject.GetComponent<AikaController>();
                                if (hitController != null)
                                {
                                    if (velocity.y > 0) { hitController.crushedInfo.below = true; }
                                    if (velocity.y < 0) { hitController.crushedInfo.above = true; }
                                }

                                transportedObjects.Add(hit.transform);

                                //hitPhysics.Move(new Vector2(pushX, pushY));
                                Vector2 pushVector = new Vector2(pushX, pushY);

                                // hit.collider.gameObject.GetComponent<PhysicsBody>().Move(new Vector2(pushX, pushY));
                                hit.transform.Translate(pushVector);
                                --i;

                                if (hit.collider.gameObject.tag == "Aika")
                                {
                                    hit.collider.gameObject.GetComponent<Carrier>().Push(pushVector);
                                }
                                else if (hit.collider.gameObject.tag == "Block")
                                {
                                    hit.collider.gameObject.GetComponent<Transporter>().Transport(pushVector);
                                }
                            }
                        }
                    }
                }

                // Horizontally-moving platform
                if (velocity.x != 0)
                {
                    float rayLength = Mathf.Abs(velocity.y) + raycaster.skinWidth;

                    for (int i = 0; i < raycaster.verticalRayCount; ++i)
                    {
                        Vector2 rayOrigin = (directionY == -1) ? raycaster.raycastOrigins.bottomLeft : raycaster.raycastOrigins.topLeft;
                        rayOrigin += Vector2.right * (raycaster.verticalRaySpacing * i); // - velocity.x
                        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, transportMask);

                        if (hit)
                        {
                            if (!transportedObjects.Contains(hit.transform))
                            {
                                float pushX = velocity.x - (hit.distance - raycaster.skinWidth) * directionX;
                                float pushY = 0f;

                                transportedObjects.Add(hit.transform);
                                //hitPhysics.Move(new Vector2(pushX, pushY));
                                Vector2 pushVector = new Vector2(pushX, pushY);

                                // hit.collider.gameObject.GetComponent<PhysicsBody>().Move(new Vector2(pushX, pushY));
                                hit.transform.Translate(pushVector);
                                --i;

                                if (hit.collider.gameObject.tag == "Aika")
                                {
                                    hit.collider.gameObject.GetComponent<Carrier>().Push(pushVector);
                                }
                                else if (hit.collider.gameObject.tag == "Block")
                                {
                                    hit.collider.gameObject.GetComponent<Transporter>().Transport(pushVector);
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Vertically-moving platform
                if (velocity.y > 0)
                {
                    float rayLength = Mathf.Abs(velocity.y) + raycaster.skinWidth;

                    for (int i = 0; i < raycaster.verticalRayCount; ++i)
                    {
                        Vector2 rayOrigin = (directionY == -1) ? raycaster.raycastOrigins.bottomLeft : raycaster.raycastOrigins.topLeft;
                        rayOrigin += Vector2.right * (raycaster.verticalRaySpacing * i); // - velocity.x
                        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.up * directionY, rayLength, transportMask);

                        foreach (RaycastHit2D hit in hits)
                        {
                            if (hit && hit.collider.gameObject != this.gameObject)
                            {
                                Carriable hitCarriable = hit.collider.gameObject.GetComponent<Carriable>();
                                if (hitCarriable == null || !hitCarriable.carried)
                                {
                                    if (!transportedObjects.Contains(hit.transform))
                                    {
                                        BoxCollider2D hitCollider = hit.collider.gameObject.GetComponent<BoxCollider2D>();
                                        // If the hit object is completely below the physics body
                                        if (hitCollider.bounds.center.y - hitCollider.bounds.extents.y >= (raycaster.boxCollider.bounds.center.y + raycaster.boxCollider.bounds.extents.y - raycaster.skinWidth))
                                        {
                                            float pushX = (directionY == 1) ? velocity.x : 0f;
                                            //if (hitCarriable != null) { pushX = 0f; }
                                            float pushY = velocity.y - (hit.distance - raycaster.skinWidth) * directionY;

                                            transportedObjects.Add(hit.transform);

                                            //hitPhysics.Move(new Vector2(pushX, pushY));
                                            Vector2 pushVector = new Vector2(pushX, pushY);

                                            // hit.collider.gameObject.GetComponent<PhysicsBody>().Move(new Vector2(pushX, pushY));
                                            hit.transform.Translate(pushVector);
                                            --i;

                                            if (hit.collider.gameObject.tag == "Aika")
                                            {
                                                Carrier aikaCarrier = hit.collider.gameObject.GetComponent<Carrier>();
                                                if (this.gameObject.GetComponent<Carriable>() != aikaCarrier.GetObjCarrying())
                                                {
                                                    aikaCarrier.Push(pushVector);
                                                }
                                            }
                                            else if (hit.collider.gameObject.tag == "Block")
                                            {
                                                hit.collider.gameObject.GetComponent<Transporter>().Transport(pushVector);
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            // Passenger on top
            if (directionY == -1 || (velocity.y == 0 && velocity.x != 0))
            {
                float rayLength = raycaster.skinWidth * 2; // changed

                for (int i = 0; i < raycaster.verticalRayCount; ++i)
                {
                    Vector2 rayOrigin = raycaster.raycastOrigins.topLeft;
                    rayOrigin += Vector2.right * (raycaster.verticalRaySpacing * i); // - velocity.x
                    RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector2.up, rayLength, transportMask); // changed

                    // Debug.DrawRay(rayOrigin, Vector2.up * rayLength * 200f, Color.blue);

                    foreach (RaycastHit2D hit in hits)
                    {
                        if (hit && hit.collider.gameObject != this.gameObject)
                        {
                            Carriable hitCarriable = hit.collider.gameObject.GetComponent<Carriable>();
                            if (hitCarriable == null || !hitCarriable.carried)
                            {
                                if (!transportedObjects.Contains(hit.transform))
                                {
                                    BoxCollider2D hitCollider = hit.collider.gameObject.GetComponent<BoxCollider2D>();
                                    // If the hit object is completely below the physics body
                                    if (hitCollider.bounds.center.y - hitCollider.bounds.extents.y >= (raycaster.boxCollider.bounds.center.y + raycaster.boxCollider.bounds.extents.y - raycaster.skinWidth))
                                    {
                                        transportedObjects.Add(hit.transform);
                                        float pushX = velocity.x; // changed
                                        float pushY = velocity.y; // changed

                                        PhysicsBody hitPhysics = hit.collider.gameObject.GetComponent<PhysicsBody>();
                                        if (pushX > 0 && hitPhysics.collisions.right)
                                        {
                                            pushX = 0f;
                                        }
                                        else if (pushX < 0 && hitPhysics.collisions.left)
                                        {
                                            pushX = 0f;
                                        }
                                        if (pushY > 0 && hitPhysics.collisions.above)
                                        {
                                            pushY = 0f;
                                        }

                                        //hitPhysics.Move(new Vector2(pushX, pushY));
                                        Vector2 pushVector = new Vector2(pushX, pushY);

                                        // hit.collider.gameObject.GetComponent<PhysicsBody>().Move(new Vector2(pushX, pushY));
                                        hit.transform.Translate(pushVector);
                                        --i;

                                        if (hit.collider.gameObject.tag == "Aika")
                                        {
                                            Carrier aikaCarrier = hit.collider.gameObject.GetComponent<Carrier>();
                                            if (this.gameObject.GetComponent<Carriable>() != aikaCarrier.GetObjCarrying())
                                            {
                                                aikaCarrier.Push(pushVector);
                                            }
                                        }
                                        else if (hit.collider.gameObject.tag == "Block")
                                        {
                                            hit.collider.gameObject.GetComponent<Transporter>().Transport(pushVector);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}