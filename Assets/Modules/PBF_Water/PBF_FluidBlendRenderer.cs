using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaughtyDoggy.Fluid
{
    public class PBF_FluidBlendRenderer : MonoBehaviour
    {
        [SerializeField] private PBF_FluidNormalRenderer normalRenderer;
        
        [SerializeField] private Material blendShadingMat;

        // Start is called before the first frame update
        void Start()
        {
            normalRenderer = GetComponentInChildren<PBF_FluidNormalRenderer>();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

