using UnityEngine;
using NaughtyDoggy.Helper;

namespace NaughtyDoggy.Misc
{
    public class CameraFollow : MonoBehaviour
    {
        [SerializeField] private GameObject _player;
        [SerializeField] private Vector3 _offset;

        private void Start()
        {
            _offset = transform.position - _player.transform.position;
        }

        void Update()
        {
            transform.position = MathHelper.Vector3X0Z(_player.transform.position) + _offset;
        }
    }
}

