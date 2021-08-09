using System;
using System.Collections;
using System.Collections.Generic;
using NaughtyDoggy.Fluid;
using UnityEngine;

namespace NaughtyDoggy.Interactive
{
    public class BreakablePool : MonoBehaviour
    {
        [SerializeField] private PBF_Solver _fluidContainer;
        [SerializeField] private bool _damaged = false;

        private void Start()
        {
            _fluidContainer = GetComponentInChildren<PBF_Solver>();
        }

        private void Response()
        {
            if (!_damaged)
            {
                _damaged = true;
                LoosePoolWall();
            }
        }

        private void LoosePoolWall()
        {
            // todo: redo hard-coded break extends as serialize field
            _fluidContainer.BoundaryMax.x = 89.93f;
            _fluidContainer.BoundaryMax.z = 35.4f;
        }
    }
}
