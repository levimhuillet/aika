using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Functionalities;

/*
 * CURRENTLY NOT IN USE
 */ 

namespace AikaGame.Functionalities
{
    [RequireComponent(typeof(Transporter))]
    [RequireComponent(typeof(Pausable))]
    public class MovingPlatform : MonoBehaviour
    {
        Transporter _transporter;
        [HideInInspector]
        public Pausable pausable;

        public enum MPStates
        {
            max,
            increasing,
            decreasing,
            min
        }

        public Vector2 minPos; // the starting position of this object (in prep for Reload() function and raising/lowering)
        public Vector2 maxPos; // the destination/max position of this object (in prep for Reload() function and raising/lowering)
        public MPStates state; // stores the object's state (e.g. min, rising, max, lowering)
        public Vector2 baseVelocity; // the speed at which this object rises and falls

        // Start is called before the first frame update
        void Awake()
        {
            _transporter = this.GetComponent<Transporter>();
            pausable = this.GetComponent<Pausable>();
        }

        private void Update()
        {
            MovePlatform();
        }

        public void MovePlatform()
        {
            if (!pausable.paused)
            {
                _transporter.raycaster.UpdateRaycastOrigins();

                Vector2 velocity = Modulate() * Time.deltaTime;

                _transporter.Transport(velocity);
            }
        }

        /*
         * Causes object to move toward min or max according to its state
         */
        protected Vector2 Modulate()
        {
            Vector2 modulateVector = new Vector2(0f, 0f);
            // Case when object is rising
            if (state == MPStates.increasing)
            {
                // Y
                // Case when object has reached its max height
                if (transform.position.y >= maxPos.y)
                {
                    // Set the height to exactly its max
                    transform.position = new Vector2(transform.position.x, maxPos.y);

                    // Change the state from "rising" to "max"
                    state = MPStates.max;
                }
                // Case when object has not reached its max height
                else
                {
                    // Continue rising
                    transform.Translate(new Vector2(0f, baseVelocity.y * Time.deltaTime));

                    modulateVector.y += baseVelocity.y;
                }

                // X
                // Case when object has reached its max height
                if (transform.position.x >= maxPos.x)
                {
                    // Set the height to exactly its max
                    transform.position = new Vector2(maxPos.x, transform.position.y);

                    // Change the state from "rising" to "max"
                    state = MPStates.max;
                }
                // Case when object has not reached its max height
                else
                {
                    // Continue rising
                    transform.Translate(new Vector2(baseVelocity.x * Time.deltaTime, 0f));

                    modulateVector.x += baseVelocity.x;
                }

                return modulateVector;

            }
            // Case when object is lowering
            else if (state == MPStates.decreasing)
            {
                // Y
                // Case when object has reached its min height
                if (transform.position.y <= minPos.y)
                {
                    // Set the height to exactly its max
                    transform.position = new Vector2(transform.position.x, minPos.y);

                    // Change the state from "rising" to "max"
                    state = MPStates.min;
                }
                else
                {
                    // Continue lowering
                    transform.Translate(new Vector2(0f, -baseVelocity.y * Time.deltaTime));

                    modulateVector.y -= baseVelocity.y;
                }

                // X
                // Case when object has reached its max height
                if (transform.position.x <= minPos.x)
                {
                    // Set the height to exactly its max
                    transform.position = new Vector2(minPos.x, transform.position.y);

                    // Change the state from "lowering" to "min"
                    state = MPStates.min;
                }
                // Case when object has not reached its min height
                else
                {
                    // Continue lowering
                    transform.Translate(new Vector2(-baseVelocity.x * Time.deltaTime, 0f));

                    modulateVector.x -= baseVelocity.x;
                }

                return modulateVector;
            }
            else
            {
                return new Vector2(0f, 0f);
            }
        }

        /*
     * Sets this elevator's state
     */
        public void SetState(MovingPlatform.MPStates state)
        {
            this.state = state;
        }


    }
}