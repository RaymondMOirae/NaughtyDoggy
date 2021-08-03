using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NaughtyDoggy.Fluid
{
    public class PBF_FluidRenderer : MonoBehaviour
    {
        private Material _particleMat;

            private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {

        }

        // private void DrawParticle()
        // {
        //     particleMat.SetBuffer("ParticleBuffer", _particleBufferREAD);
        //     Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMat, instancingBounds,
        //         _instancingArgsBuffer);
        // }

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }

}
