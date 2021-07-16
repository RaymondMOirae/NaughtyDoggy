using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

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
            transform.position = _player.transform.position + _offset;
        }
    }
}

