using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace AikaGame.Functionalities
{
    [RequireComponent(typeof(BoxCollider2D))]
    public class Carriable : MonoBehaviour
    {
        public bool carried;
        Carrier _carrier;

        private void Awake()
        {
            carried = false;
            _carrier = null;
        }

        public void BeCarried(Carrier carrier)
        {
            if (_carrier != null)
            {
                _carrier.ForceDrop();
            }
            carried = true;

            _carrier = carrier;
            // Debug.Log(this + " is now being carried");
        }

        public void BeDropped()
        {
            carried = false;
            _carrier = null;
            // Debug.Log(this + " has been dropped");
        }
    }
}
