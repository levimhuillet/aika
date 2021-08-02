using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AikaGame.Functionalities
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Carrier : MonoBehaviour
    {
        [HideInInspector]
        public bool carrying;
        Carriable _objCarrying;

        BoxCollider2D _boxCollider;

        private void Awake()
        {
            carrying = false;
            _objCarrying = null;
            _boxCollider = this.GetComponent<BoxCollider2D>();
        }

        private void Update()
        {
            if (carrying)
            {
                if (_objCarrying == null)
                {
                    // Should not occur
                    Debug.Log("carrying nothing -- should not occur");
                    carrying = false;
                }
            }
        }

        public void Carry(Carriable objToCarry)
        {
            objToCarry.BeCarried(this);
            carrying = true;
            _objCarrying = objToCarry;
            Pop();
            // Debug.Log(this + " is now carrying " + objToCarry);
        }

        public void Drop()
        {
            // Debug.Log(this + " has dropped " + objCarrying);
            _objCarrying.BeDropped();
            carrying = false;
            _objCarrying = null;
        }

        public void ForceDrop()
        {
            carrying = false;
            _objCarrying.BeDropped();
            _objCarrying = null;
        }

        void Pop()
        {
            BoxCollider2D otherCollider = _objCarrying.GetComponent<BoxCollider2D>();
            _objCarrying.gameObject.transform.position = this.transform.position + new Vector3(0f, _boxCollider.bounds.extents.y + otherCollider.bounds.extents.y, 0f);
        }

        public Vector2 Push(Vector2 velocity)
        {
            if (_objCarrying != null)
            {
                if (_objCarrying.GetComponent<Block>() != null)
                {
                    Vector2 finalVelocity = _objCarrying.GetComponent<Block>().Move(velocity);
                    // _objCarrying.GetComponent<Block>().Transport(finalVelocity);
                    return finalVelocity;
                }
            }
            return velocity;
        }

        public Carriable GetObjCarrying()
        {
            return _objCarrying;
        }
    }
}