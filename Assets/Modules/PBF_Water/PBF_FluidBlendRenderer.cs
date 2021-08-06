using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaughtyDoggy.Fluid
{
    public class PBF_FluidBlendRenderer : MonoBehaviour
    {
        [SerializeField] private PBF_FluidShadingRenderer shadingRenderer;
        [SerializeField] private Material blendShadingMat;
        [Range(0.0f, 1.0f)]
        [SerializeField] private float fluidBlendWeight;

        private Camera _blendCam;

        // Start is called before the first frame update
        void Start()
        {
            
            shadingRenderer = GetComponentInChildren<PBF_FluidShadingRenderer>();
            _blendCam = Camera.main;
            _blendCam.depthTextureMode = DepthTextureMode.Depth;
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            blendShadingMat.SetTexture("_FluidTex", shadingRenderer.FluidTexture);
            blendShadingMat.SetFloat("_FluidBlendWeight", fluidBlendWeight);

            Graphics.Blit(src, dest, blendShadingMat, 0);
        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}

