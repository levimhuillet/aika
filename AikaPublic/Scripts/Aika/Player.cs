using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Aikas;
using AikaGame.Entries;
using AikaGame.Managers;

/*
 * The player is the aika controlled by the user in a scene.
 * Player actions are stored over the course of a scene, and when the level is reloaded a Replicate repeats their actions without user input.
 */
public class Player : Aika
{
    List<List<ReplicateMove>> _movementRecord; // A list of instructions per frame, inside a list of frame instructions per lifetime. Future replicate movements are pulled from here

    // ---------- Unity Callbacks ----------

    /*
     * Unity Callback
     */
    new void Awake()
    {
        base.Awake();

        // Only the player begins with an empty movement record
        _movementRecord = new List<List<ReplicateMove>>();
    }

    new void Update()
    {
        if (!this._controller.physicsBody.pausable.paused)
        {
            base.Update();

            List<ReplicateMove> thisFrameMovement = new List<ReplicateMove>(); // tracks all movement made during the present frame

            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

            if (input.x < 0)
            {
                thisFrameMovement.Add(ReplicateMove.left);
            }
            else if (input.x > 0)
            {
                thisFrameMovement.Add(ReplicateMove.right);
            }

            if (Input.GetKey("w") || input.y > 0)
            {
                Jump();
                thisFrameMovement.Add(ReplicateMove.jump);
            }
            else if (Input.GetKeyUp("w") || input.y == 0)
            {
                // end jump
                EndJump();

                thisFrameMovement.Add(ReplicateMove.endJump);
            }

            // Case when user enters input to interact
            if (Input.GetKeyDown("i"))
            {
                // Save interact instruction to replicate
                thisFrameMovement.Add(ReplicateMove.interact); // m_FrameNum - 1 because frameNum starts at 1, while m_MovementRecord index starts at 0

                // Interact with object
                Interact();
            }

            bool isExiting = false;
            if (Input.GetKey("space"))
            {
                Portal overlappingPortal = _controller.GetPortalOverlap();
                if (overlappingPortal != null)
                {
                    overlappingPortal.EnterPortal();
                }
                else
                {
                    if (_controller.IsOverlappingExit() && this._frameNum > 40)
                    {
                        isExiting = true;
                    }
                }

                thisFrameMovement.Add(ReplicateMove.enter); // m_FrameNum - 1 because frameNum starts at 1, while m_MovementRecord index starts at 0
            }

            // Store all movements into m_MovementRecord
            _movementRecord.Add(thisFrameMovement);
            ++_frameNum;

            if (isExiting)
            {
                LevelManager.ExitSequence.Invoke();
            }

            _controller.physicsBody.velocity.x = input.x * _moveSpeed;
            if (_controller.physicsBody.velocity.x < 0) { _sr.flipX = true; }
            else if (_controller.physicsBody.velocity.x > 0) { _sr.flipX = false; }
            _controller.physicsBody.velocity.y += _controller.physicsBody.gravity * Time.deltaTime;
            Move(_controller.physicsBody.velocity * Time.deltaTime);
        }
    }

    // ---------- Scene Management ----------

    /*
     * Removes the player from the level
     */
    public void LeaveLevel()
    {
        // TODO: Call player exit animation
        gameObject.SetActive(false);
        _hasFinishedMovement = true;
    }

    /*
     * Returns the player's movement record
     */
    public List<List<ReplicateMove>> GetMovementRecord()
    {
        return _movementRecord;
    }
}
