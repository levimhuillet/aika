using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Functionalities;
using AikaGame.GameData;

/*
 * A Block is an object an aika can pick up or stand on. Blocks stack on each other.
 */
[RequireComponent(typeof(PhysicsBody))]
[RequireComponent(typeof(Carriable))]
[RequireComponent(typeof(Transporter))]
public class Block : MonoBehaviour
{
    PhysicsBody _physicsBody;
    Carriable _carriable;
    Transporter _transporter;

    private void Awake()
    {
        _physicsBody = this.GetComponent<PhysicsBody>();
        _carriable = this.GetComponent<Carriable>();
        _transporter = this.GetComponent<Transporter>();
    }

    private void Start()
    {
        _physicsBody.gravity = PhysicsData.worldGravity;
    }

    private void Update()
    {
        Vector2 transportVelocity;
        if (!this._physicsBody.pausable.paused && !_carriable.carried)
        {
            if (_physicsBody.collisions.above || _physicsBody.collisions.below)
            {
                _physicsBody.velocity.y = 0f;
            }

            _physicsBody.velocity.y += _physicsBody.gravity * Time.deltaTime;
            transportVelocity = _physicsBody.Move(_physicsBody.velocity * Time.deltaTime);
        }
        else if (_carriable.carried)
        {
            // Apply Carrier velocity (applied in Carrier update)
            transportVelocity = new Vector2(0f, 0f);
        }
        else
        {
            // No velocity to apply when the game is paused
            transportVelocity = new Vector2(0f, 0f);
        }

        _transporter.Transport(transportVelocity);
    }

    public void Transport(Vector2 velocity)
    {
        _transporter.Transport(velocity);
    }

    public Vector2 Move(Vector2 velocity)
    {
        Vector2 transportVelocity = _physicsBody.Move(velocity);
        Transport(transportVelocity);
        return transportVelocity;
    }
}
