using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AikaGame.Functionalities;

/*
 * A platform that rises and falls as directed by an external object state (e.g. button pressed)
 * Inherits from the ICarrier class for its PopToPosition function,
 * which it uses if it would crush a block
 */
[RequireComponent(typeof(MovingPlatform))]
public class Elevator : MonoBehaviour
{
    public float riseDistance; // the max distance this elevator will raise when its trigger is active
    protected MovingPlatform _movingPlatform;

    private void Awake()
    {
        _movingPlatform = this.GetComponent<MovingPlatform>();
        // riseDistance = 11.5f;
    }

    /*
     * Start is called before the first frame update
     */
    void Start()
    {
        _movingPlatform.state = MovingPlatform.MPStates.min;
        _movingPlatform.minPos = this.transform.position;
        _movingPlatform.maxPos = new Vector2(this.transform.position.x, this.transform.position.y + riseDistance);
        _movingPlatform.baseVelocity = new Vector2(0f, 8f);
    }

    /*
     * Sets this elevator's state
     */
    public void SetState(MovingPlatform.MPStates state)
    {
        this._movingPlatform.SetState(state);
    }
}
