using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Interactive
{
    public class SignBillboard : MonoBehaviour
    {
        private Camera _mainCam;
        public Vector3 offset = new Vector3(0, 180);

        private void Start()
        {
            _mainCam = Camera.main;
        }

        void Update()
        {
            transform.LookAt(_mainCam.transform);
            transform.Rotate(offset, Space.Self);
        }
    }

}
