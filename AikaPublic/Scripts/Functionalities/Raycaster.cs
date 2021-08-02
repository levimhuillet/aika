using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AikaGame.Functionalities
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Raycaster : MonoBehaviour
    {
        [HideInInspector]
        public BoxCollider2D boxCollider;

        [HideInInspector]
        public float skinWidth { get; set; }
        public int horizontalRayCount = 6;
        public int verticalRayCount = 6;

        [HideInInspector]
        public float horizontalRaySpacing;
        [HideInInspector]
        public float verticalRaySpacing;

        public struct RaycastOrigins
        {
            public Vector2 topLeft, topRight;
            public Vector2 bottomLeft, bottomRight;
            public Vector2 bottomCenter;
        }

        [HideInInspector]
        public RaycastOrigins raycastOrigins;

        public LayerMask collisionMask;
        public LayerMask verticalOnlyMask;


        public void Awake()
        {
            boxCollider = this.GetComponent<BoxCollider2D>();
        }

        public void Start()
        {
            CalculateRaySpacing();
        }

        public void UpdateRaycastOrigins()
        {
            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            raycastOrigins.bottomLeft = new Vector2(bounds.min.x, bounds.min.y);
            raycastOrigins.bottomRight = new Vector2(bounds.max.x, bounds.min.y);
            raycastOrigins.topLeft = new Vector2(bounds.min.x, bounds.max.y);
            raycastOrigins.topRight = new Vector2(bounds.max.x, bounds.max.y);
            raycastOrigins.bottomCenter = new Vector2(bounds.center.x, bounds.min.y);
        }

        public void CalculateRaySpacing()
        {
            Bounds bounds = boxCollider.bounds;
            bounds.Expand(skinWidth * -2);

            horizontalRayCount = Mathf.Clamp(horizontalRayCount, 2, int.MaxValue);
            verticalRayCount = Mathf.Clamp(verticalRayCount, 2, int.MaxValue);

            horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
            verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);
        }
    }
}
