using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using NaughtyDoggy.Helper;

namespace NaughtyDoggy.Interactive
{
    public class InteractionTargetDetector : MonoBehaviour
    {
        [SerializeField] private float checkRadius;
        [SerializeField] private LayerMask interactiveLayer;
        [SerializeField] private BoxCollider _contactor;

        private void Start()
        {
            _contactor = GetComponent<BoxCollider>();
        }

        private void FixedUpdate()
        {
            
        }

        private void EnableTargetSign()
        {
            Collider[] hits = Physics.OverlapBox(_contactor.transform.position, _contactor.size, Quaternion.identity,
                interactiveLayer);

            IEnumerable<Transform> filteredResult = from hit in hits
                                                    where hit.CompareTag("InteractiveEntity")
                                                    orderby OffetAngleInXZDimention(transform.position, hit.transform.position)
                                                    select hit.transform;
            if (filteredResult.Any())
            {
                filteredResult.First().BroadcastMessage("ActivateSign", SendMessageOptions.RequireReceiver);
                foreach (Transform t in filteredResult.Skip(1))
                {
                    t.BroadcastMessage("DeactivateSign", SendMessageOptions.RequireReceiver);
                }
            }
        }

        private float OffetAngleInXZDimention(Vector3 from, Vector3 to)
        { 
            return Mathf.Abs(Vector2.SignedAngle(MathHelper.Vec3XZ(from), MathHelper.Vec3XZ(to)));
        }

        private void OnTriggerStay(Collider other)
        {
            if(other.CompareTag("InteractiveEntity"))
                EnableTargetSign();
        }
        
        // private void OnTriggerStay(Collider other)
        // {
        //     EnableTargetSign();
        // }

        private void OnTriggerExit(Collider other)
        {
            if(other.CompareTag("InteractiveEntity"))
                other.BroadcastMessage("DeactivateSign", SendMessageOptions.RequireReceiver);
        }
    }

}
