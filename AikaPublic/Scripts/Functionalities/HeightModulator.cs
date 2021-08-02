using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AikaGame.Functionalities
{
    /*
     * The parent class for any objects which rise and fall according to their state
     */
    //[RequireComponent(typeof(BoxCollider2D))]
    public class HeightModulator : MonoBehaviour
    {
        public enum HMStates
        {
            max,
            increasing,
            decreasing,
            min
        }

        // protected Rigidbody2D m_BC; // this object's RigidBody2D
        protected Vector2 _minPos; // the starting position of this object (in prep for Reload() function and raising/lowering)
        protected Vector2 _maxPos; // the destination/max position of this object (in prep for Reload() function and raising/lowering)
        protected HMStates _state; // stores the object's state (e.g. min, rising, max, lowering)
        protected Vector2 _speed; // the speed at which this object rises and falls

        // ---------- Unity Callbacks ----------

        /*
         * FixedUpdate is called once per frame during physics calculations
         */
        protected void Update()
        {
            Modulate();
        }

        // ---------- Member Functions ----------

        /*
         * Causes object to rise or fall according to its state
         */
        protected void Modulate()
        {
            // Case when object is rising
            if (_state == HMStates.increasing)
            {
                // Case when object has reached its max height
                if (transform.position.y >= _maxPos.y)
                {
                    // Set the height to exactly its max
                    transform.position = new Vector2(transform.position.x, _maxPos.y);

                    // Change the state from "rising" to "max"
                    _state = HMStates.max;
                }
                // Case when object has not reached its max height
                else
                {
                    // Continue rising
                    transform.Translate(_speed * Time.deltaTime);
                    //transform.position = new Vector2(transform.position.x, transform.position.y + _speed * Time.deltaTime);
                }

            }
            // Case when object is lowering
            else if (_state == HMStates.decreasing)
            {
                // Case when object has reached its min height
                if (transform.position.y <= _minPos.y)
                {
                    // Set the height to exactly its min
                    transform.position = new Vector2(transform.position.x, _minPos.y);

                    // Change the state from "lowering" to "min"
                    _state = HMStates.min;
                }
                else
                {
                    // Continue lowering
                    transform.Translate(-_speed * Time.deltaTime);
                    //transform.position = new Vector2(transform.position.x, transform.position.y - _speed * Time.deltaTime);
                }

            }
        }

        /*
         * Used by other objects for physics calculations
         */
        public float GetSpeed()
        {
            return _speed.y;
        }

        /*
         * Used by other objects for physics calculations, determining crushing, etc.
         */
        public HMStates GetState()
        {
            return _state;
        }
    }

}