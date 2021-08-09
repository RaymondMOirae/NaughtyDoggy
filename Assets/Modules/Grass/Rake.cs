using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NaughtyDoggy.Interactive;

namespace NaughtyDoggy.Grass
{
    public class Rake : MonoBehaviour
    {
        private PickableItem _rakeBody;

        private void Start()
        {
            _rakeBody = GetComponentInParent<PickableItem>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("CollisionBasedInteract") && _rakeBody.BeHeld)
            {
                other.BroadcastMessage("Response");
            }
        }

        private void OnCollisionEnter(Collision other)
        {
            if (other.collider.CompareTag("CollisionBasedInteract") && _rakeBody.BeHeld)
            {
                other.gameObject.BroadcastMessage("Response", SendMessageOptions.DontRequireReceiver);
            }

        }
    }

}

