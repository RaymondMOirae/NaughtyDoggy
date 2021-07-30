using System.Linq;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace NaughtyDoggy.PlayerControls
{
    public class IKFootBehavior : MonoBehaviour
    {
        [SerializeField] private GameObject _rfFootRig;
        [SerializeField] private GameObject _lfFootRig;
        [SerializeField] private GameObject _rbFootRig;
        [SerializeField] private GameObject _lbFootRig;

        [SerializeField] private Transform _rfFootTransform;
        [SerializeField] private Transform _lfFootTransform;
        [SerializeField] private Transform _rbFootTransform;
        [SerializeField] private Transform _lbFootTransform;

        [SerializeField] private Transform _rfFootTargetTransform;
        [SerializeField] private Transform _lfFootTargetTransform;
        [SerializeField] private Transform _rbFootTargetTransform;
        [SerializeField] private Transform _lbFootTargetTransform;

        [SerializeField] private int _ikLayer;
        private Animator _animator;

        private TwoBoneIKConstraint[] _footIKConstrains;
        private Transform[] _footTargetTransforms;
        private Transform[] _footTransforms;
        private Vector3[] _footTargetOffset;
        private Vector3[] _hitNormals;
        private bool[] _groundSpherecastHits;

        [SerializeField] private float _maxHitDistance = 1.45f;
        [SerializeField] private float _addedHeight = 0.45f;
        [SerializeField] private float _yOffset = 0.05f;

        private float _angleAboutX;
        private float _angleAboutZ;

        // Start is called before the first frame update
        void Start()
        {
            _animator = GetComponent<Animator>();

            _footIKConstrains = new TwoBoneIKConstraint[4];
            _footIKConstrains[0] = _rfFootRig.GetComponent<TwoBoneIKConstraint>();
            _footIKConstrains[1] = _lfFootRig.GetComponent<TwoBoneIKConstraint>();
            _footIKConstrains[2] = _rbFootRig.GetComponent<TwoBoneIKConstraint>();
            _footIKConstrains[3] = _lbFootRig.GetComponent<TwoBoneIKConstraint>();

            _footTransforms = new Transform[4];
            _footTransforms[0] = _rfFootTransform;
            _footTransforms[1] = _lfFootTransform;
            _footTransforms[2] = _rbFootTransform;
            _footTransforms[3] = _lbFootTransform;

            _footTargetTransforms = new Transform[4];
            _footTargetTransforms[0] = _rfFootTargetTransform;
            _footTargetTransforms[1] = _lfFootTargetTransform;
            _footTargetTransforms[2] = _rbFootTargetTransform;
            _footTargetTransforms[3] = _lbFootTargetTransform;

            _groundSpherecastHits = new bool[4];
            _hitNormals = new Vector3[4];
            _footTargetOffset =
                (from offset in _footTargetTransforms
                    select offset.localPosition).ToArray();
        }

        // Update is called once per frame
        void FixedUpdate()
        {
            ProjectedCharacterFeet();
            //RotateCharacterBody();
        }

        private void CheckGroundBelow(out Vector3 hitPoint, out bool gotGroundSpherecastHit, out Vector3 hitNormal,
            out float currentHitDistance, Transform objectTransform,
            int checkForLayerMask, float maxHitDistance, float addedHeight)
        {
            RaycastHit hit;
            Vector3 startSpherecast = objectTransform.position + new Vector3(0.0f, addedHeight, 0.0f);
            if (checkForLayerMask == -1)
            {
                Debug.LogError("Layer does not exist!");
                gotGroundSpherecastHit = false;
                currentHitDistance = 0.0f;
                hitNormal = Vector3.up;
                hitPoint = objectTransform.position;
            }
            else
            {
                // this will only cast
                int layerMask = (1 << checkForLayerMask);
                if (Physics.SphereCast(startSpherecast, 0.05f, Vector3.down, out hit, maxHitDistance, layerMask,
                    QueryTriggerInteraction.UseGlobal))
                {
                    Debug.DrawLine(startSpherecast, startSpherecast + Vector3.down * 1.0f, Color.magenta, 1.0f);
                    //Gizmos.DrawSphere(startSpherecast,0.2f);
                    currentHitDistance = hit.distance - addedHeight;
                    hitNormal = hit.normal;
                    gotGroundSpherecastHit = true;
                    hitPoint = hit.point;
                }
                else
                {
                    gotGroundSpherecastHit = false;
                    currentHitDistance = 0.0f;
                    hitNormal = Vector3.up;
                    hitPoint = objectTransform.position;
                }
            }

        }

        private Vector3 ProjectOnContactPlane(Vector3 vector, Vector3 hitNormal)
        {
            return vector - hitNormal * Vector3.Dot(vector, hitNormal);
        }

        private void ProjectedAxisAngles(out float angleAboutX, out float angleAboutZ, Transform footTargetTransform,
            Vector3 hitNormal)
        {
            Vector3 xAxisProjected = ProjectOnContactPlane(footTargetTransform.forward, hitNormal).normalized;
            Vector3 zAxisProjected = ProjectOnContactPlane(footTargetTransform.right, hitNormal).normalized;

            angleAboutX = Vector3.SignedAngle(footTargetTransform.forward, xAxisProjected, footTargetTransform.right);
            angleAboutZ = Vector3.SignedAngle(footTargetTransform.right, zAxisProjected, footTargetTransform.forward);
        }

        private void ProjectedCharacterFeet()
        {
            for (int i = 0; i < 4; i++)
            {
                CheckGroundBelow(out Vector3 hitPoint, out _groundSpherecastHits[i], out Vector3 hitNormal,
                    out _, _footTransforms[i], _ikLayer, _maxHitDistance, _addedHeight);
                _hitNormals[i] = hitNormal;

                if (_groundSpherecastHits[i] == true)
                {
                    ProjectedAxisAngles(out _angleAboutX, out _angleAboutZ, _footTransforms[i], _hitNormals[i]);

                    _footTargetTransforms[i].position = new Vector3(_footTransforms[i].position.x,
                        hitPoint.y + _yOffset,
                        _footTransforms[i].position.z);
                }
                else
                {
                    _footTargetTransforms[i].position = _footTransforms[i].position + _footTargetOffset[i];
                    //_footTargetTransforms[i].position = _footTransforms[i].position;
                    //_footTargetTransforms[i].rotation = _footTransforms[i].rotation;
                }
            }
        }

        [SerializeField] private float _maxRotationX = 20.0f;

        [SerializeField] private float _maxRotationZ = 50.0f;

        [SerializeField] private float _maxRotationStep = 1.0f;

        private void RotateCharacterBody()
        {
            Vector3 avgFootNormal = new Vector3();
            for (int i = 0; i < 4; i++)
            {
                avgFootNormal += _hitNormals[i] / 4.0f;
            }

            avgFootNormal = avgFootNormal.normalized;
            ProjectedAxisAngles(out _angleAboutX, out _angleAboutZ, transform, avgFootNormal);

            Vector3 playerRotation = transform.eulerAngles;

            playerRotation.x -= playerRotation.x > 180 ? 360 : 0;
            playerRotation.z -= playerRotation.z > 180 ? 360 : 0;

            if (playerRotation.x + _angleAboutX < -_maxRotationX)
            {
                _angleAboutX = _maxRotationX + playerRotation.x;
            }
            else if (playerRotation.x + _angleAboutX > _maxRotationX)
            {
                _angleAboutX = _maxRotationX - playerRotation.x;
            }

            if (playerRotation.z + _angleAboutZ < -_maxRotationZ)
            {
                _angleAboutZ = _maxRotationZ + playerRotation.z;
            }
            else if (playerRotation.z + _angleAboutZ > _maxRotationZ)
            {
                _angleAboutZ = _maxRotationZ - playerRotation.z;
            }

            float eulerXToTurn = Mathf.MoveTowardsAngle(0, _angleAboutX, _maxRotationStep);
            float eulerZToTurn = Mathf.MoveTowardsAngle(0, _angleAboutZ, _maxRotationStep);
            //transform.eulerAngles = transform.eulerAngles + new Vector3(eulerXToTurn, eulerZToTurn,0.0f);
            transform.eulerAngles = transform.eulerAngles + new Vector3(eulerXToTurn, 0.0f, eulerZToTurn);
        }
    }
}