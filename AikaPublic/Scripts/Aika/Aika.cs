using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using AikaGame.Controllers;
using AikaGame.Functionalities;
using AikaGame.GameData;

namespace AikaGame.Aikas
{
    public enum ReplicateMove
    {
        left,
        right,
        jump,
        endJump,
        interact,
        enter
    }

    /*
     * Main driver of interaction in the scene.
     * Interacts with objects with the goal of unlocking and returning to the exit.
     * Classes of the Aika are Player and Replicate.
     * 
     * Inherits from Carrier class to carry objects (such as AikaBlocks)
     * Inherits from ParentModifiable class to maintain steady
     * movement on blocks (and other horizontally-moving structures)
     */
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(AikaController))]
    [RequireComponent(typeof(Carrier))]
    public class Aika : MonoBehaviour
    {
        protected AikaController _controller;
        protected Carrier _carrier;

        protected float _moveSpeed = 12.5f;
        protected float _minJumpHeight = 1.25f;
        protected float _maxJumpHeight = 7.5f;
        protected float _timeToJumpApex = .5f;

        protected float _minJumpVelocity;
        protected float _maxJumpVelocity;

        protected bool _hasFinishedMovement;

        protected int _frameNum;

        struct StartPos
        {
            public float x, y;
        }

        StartPos _startPos;

        protected SpriteRenderer _sr;

        public static UnityEvent Crushed = new UnityEvent();

        protected void Awake()
        {
            _controller = this.GetComponent<AikaController>();
            _controller.physicsBody = this.GetComponent<PhysicsBody>();
            _controller.physicsBody.Awake();

            _controller.physicsBody.gravity = -(2 * _maxJumpHeight) / Mathf.Pow(_timeToJumpApex, 2);
            PhysicsData.worldGravity = _controller.physicsBody.gravity;
            _maxJumpVelocity = Mathf.Abs(_controller.physicsBody.gravity) * _timeToJumpApex;
            _minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(_controller.physicsBody.gravity) * _minJumpHeight);

            _hasFinishedMovement = false;

            _startPos.x = this.transform.position.x;
            _startPos.y = this.transform.position.y;

            _sr = this.GetComponent<SpriteRenderer>();

            _carrier = this.GetComponent<Carrier>();
        }

        protected void Update()
        {
            if (_controller.physicsBody.collisions.above || _controller.physicsBody.collisions.below)
            {
                _controller.physicsBody.velocity.y = 0f;
            }
            if (_controller.crushedInfo.above && _controller.crushedInfo.below)
            {
                Aika.Crushed.Invoke();
            }
        }

        protected Vector2 Move(Vector2 velocity)
        {
            Vector2 prelimVelocity = _controller.PrecarryMove(_controller.physicsBody.velocity * Time.deltaTime);
            Vector2 finalVelocity = _carrier.Push(prelimVelocity);
            if (prelimVelocity.y > finalVelocity.y)
            {
                // carried object hit something
                _controller.physicsBody.velocity.y = 0f;
            }

            _controller.Move(finalVelocity);
            return finalVelocity;
        }

        /*
         * Resets the aika's variables to their initial state
         */
        protected void Reload()
        {
            this.gameObject.SetActive(true);
            _frameNum = 0;
            transform.position = new Vector2(_startPos.x, _startPos.y);
            _hasFinishedMovement = false;
            // m_ObjectOverlapping = null;
            // this.SetObjectCarrying(null);
            _sr.flipX = false;
        }

        protected void Jump()
        {
            if (_controller.physicsBody.collisions.below)
            {
                _controller.physicsBody.velocity.y = _maxJumpVelocity;
            }
        }

        protected void EndJump()
        {
            if (_controller.physicsBody.velocity.y > _minJumpVelocity)
            {
                _controller.physicsBody.velocity.y = _minJumpVelocity;
            }
        }

        protected void Interact()
        {
            if (_carrier.carrying)
            {
                // Drop the object being carried
                _carrier.Drop();
            }
            else
            {
                // Seek objects overlapping or underneath
                GameObject objInteracting;
                GameObject objOverlapping = _controller.GetInteractOverlap();
                if (objOverlapping == null)
                {
                    GameObject objUnder = _controller.GetInteractUnder();
                    if (objUnder == null)
                    {
                        // Debug.Log("Miss");
                        return;
                    }
                    else
                    {
                        // Debug.Log("Hit Under");
                        objInteracting = objUnder;
                    }
                }
                else
                {
                    // Debug.Log("Hit Overlap");
                    objInteracting = objOverlapping;
                }

                if (objInteracting.GetComponent<Block>() != null)
                {
                    // Interacting with a Block
                    Block blockInteracting = objInteracting.GetComponent<Block>();
                    _carrier.Carry(blockInteracting.GetComponent<Carriable>());
                }
                else
                {
                    Debug.Log("Interacting with unknown object: " + objInteracting);
                }
            }

        }
    }
}