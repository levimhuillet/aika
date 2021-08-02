using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

using AikaGame.Aikas;
using AikaGame.Entries;
using AikaGame.Managers;

/*
 * An Aika whose actions are pulled from a copy of one iteration of the player's movement.
 */
public class Replicate : Aika
{
    List<List<ReplicateMove>> _movementRecord; // A list of instructions per frame, inside a list of frame instructions per lifetime. Replicate actions are pulled from here

    public static UnityEvent Paradox = new UnityEvent();

    /*
     * Unity Callback
     */
    new void Awake()
    {
        base.Awake();
    }

    /*
     * Unity Callback
     */
    new void Update()
    {
        if (!this._controller.physicsBody.pausable.paused)
        {
            base.Update();

            if (!_hasFinishedMovement)
            {
                // Replicate player movement
                if (_frameNum >= _movementRecord.Count)
                {
                    // replicate ran through all movement instructions and failed to exit: paradox
                    Paradox.Invoke();
                }
                else
                {
                    List<ReplicateMove> thisFrameMovement = _movementRecord[_frameNum];
                    Vector2 input = new Vector2(0f, 0f);

                    foreach (ReplicateMove instruction in thisFrameMovement)
                    {
                        // read in left
                        if (instruction.Equals(ReplicateMove.left))
                        {
                            //apply left force
                            input.x = -1;

                            // TODO: call left movement animation
                        }
                        // read in right
                        if (instruction.Equals(ReplicateMove.right))
                        {
                            //apply right force
                            input.x = 1;

                            // TODO: call right movement animation
                        }
                        // read in jump
                        if (instruction.Equals(ReplicateMove.jump))
                        {
                            Jump();

                            // TODO: call jump movement animation
                        }
                        // read in end jumping
                        if (instruction.Equals(ReplicateMove.endJump))
                        {
                            // end jump
                            EndJump();

                            // call jump movement animation

                        }
                        // read in interact
                        if (instruction.Equals(ReplicateMove.interact))
                        {
                            Interact();
                        }
                        // read in exit
                        if (instruction.Equals(ReplicateMove.enter))
                        {
                            Portal overlappingPortal = _controller.GetPortalOverlap();
                            if (overlappingPortal != null)
                            {
                                // overlappingPortal.EnterPortal();
                            }
                            else
                            {
                                if (_controller.IsOverlappingExit())
                                {
                                    //replicate may exit

                                    //shut off replicate
                                    _hasFinishedMovement = true;
                                    if (_carrier.carrying)
                                    {
                                        _carrier.ForceDrop();
                                    }
                                    this.gameObject.SetActive(false);
                                    // decrement the number of remaining replicates
                                    LevelManager.ReplicateExiting.Invoke();
                                }
                            }
                        }
                    }
                    ++_frameNum;

                    // For smoothing option, see https://www.youtube.com/watch?v=PlT44xr0iW0&list=PLFt_AvWsXl0f0hqURlhyIoAabKPgRsqjz&index=3 at 13:30

                    _controller.physicsBody.velocity.x = input.x * _moveSpeed;
                    if (_controller.physicsBody.velocity.x < 0) { _sr.flipX = true; }
                    else if (_controller.physicsBody.velocity.x > 0) { _sr.flipX = false; }
                    _controller.physicsBody.velocity.y += _controller.physicsBody.gravity * Time.deltaTime;
                    Vector2 prelimVelocity = _controller.PrecarryMove(_controller.physicsBody.velocity * Time.deltaTime);
                    Vector2 finalVelocity = _carrier.Push(prelimVelocity);
                    _controller.Move(finalVelocity);
                    if (prelimVelocity.y > finalVelocity.y)
                    {
                        // carried object hit something
                        _controller.physicsBody.velocity.y = 0f;
                    }

                }
            }
        }
    }

    /*
     * Resets the replicate's variables to their initial state
     */
    new public void Reload()
    {
        base.Reload();
    }

    // ---------- Getters and Setters ----------

    /*
     * Sets the replicate's movement record
     */
    public void SetMovementRecord(List<List<ReplicateMove>> movementRecord)
    {
        this._movementRecord = movementRecord;
    }
}
