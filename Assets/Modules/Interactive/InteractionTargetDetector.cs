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
        [SerializeField] private LayerMask interactiveLayer;
        [SerializeField] private BoxCollider _detector;
        [SerializeField] private InteractiveItemBase _target;
        public InteractiveItemBase FocusTarget => _target;

        private void Start()
        {
            _detector = GetComponent<BoxCollider>();
        }

        private void FixedUpdate()
        {
            EnableTargetSign();
        }

        private void EnableTargetSign()
        {
            Collider[] hits = Physics.OverlapBox(_detector.transform.position + _detector.center, _detector.size / 2, transform.rotation,
                interactiveLayer);

            IEnumerable<Transform> filteredResult = from hit in hits
                                                    where hit.CompareTag("BtnBasedInteract")
                                                    orderby OffetAngleInXZDimention(transform.forward, hit.transform.position - transform.position)
                                                    select hit.transform;
            if (filteredResult.Any())
            {
                if(_target)
                    _target.SendMessage("DeactivateSign", SendMessageOptions.RequireReceiver);
                _target = filteredResult.First().GetComponent<InteractiveItemBase>();
                filteredResult.First().SendMessage("ActivateSign", SendMessageOptions.RequireReceiver);
                foreach (Transform t in filteredResult.Skip(1))
                {
                    t.SendMessage("DeactivateSign", SendMessageOptions.RequireReceiver);
                }
            }
            else
            {
                if(_target)
                    _target.SendMessage("DeactivateSign", SendMessageOptions.RequireReceiver);
            }
        }

        private float OffetAngleInXZDimention(Vector3 from, Vector3 to)
        { 
            return Mathf.Abs(Vector2.SignedAngle(MathHelper.Vec3XZ(from), MathHelper.Vec3XZ(to)));
        }
        
    }

}
