using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

namespace NaughtyDoggy.Fluid
{
    public class PBF_FluidRenderer : MonoBehaviour
    {
        private readonly uint[] _instancingArgs = new uint[5] {0, 0, 0, 0, 0};
        private ComputeBuffer _instancingArgsBuffer;
        private Bounds instancingBounds;
        public float InstancingBoundSize; 
        
        private Camera DepthCamera;
        [SerializeField] private int FluidLayer;
        [SerializeField] private PBF_Solver Solver;

        [SerializeField] private Material particleDepthMat;
        [SerializeField] private Material particleShadingMat;
        
        [SerializeField] private Mesh particleMesh;

        // Start is called before the first frame update
        void Start()
        {
            // DepthCamera.enabled = false;
            // DepthCamera.cullingMask = 1 << FluidLayer;
            DepthCamera = GetComponent<Camera>();
            
            DepthCamera.clearFlags = CameraClearFlags.Color;
            
            // if (particleDepthShader != null)
            // {
            // Shader.SetGlobalBuffer("ParticleBuffer", Solver.PartilceBuffer);
            // DepthCamera.SetReplacementShader(ParticleDepthShader, "Opaque");
            // }
            
            InitInstancingBuffer();
        }
        
        // Update is called once per frame
        void Update()
        {
            DrawParticle();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            RenderTexture _temp = RenderTexture.GetTemporary(src.width, src.height, 24);
            // particleShadingMat.SetTexture("_MainTex", src);
            Graphics.Blit(src, _temp, particleShadingMat, 0);
            Graphics.Blit(_temp, dest);
            //Graphics.Blit(src, dest);
        }

        private void OnPreRender()
        {
            // DrawParticle();
        }

        private void DrawParticle()
        {
            particleDepthMat.SetBuffer("ParticleBuffer", Solver.PartilceBuffer);
            Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleDepthMat, instancingBounds,
                _instancingArgsBuffer, 0, null, ShadowCastingMode.Off, false, FluidLayer, DepthCamera);
        }
        
        private void InitInstancingBuffer()
        {
            if (particleMesh != null)
            {
                _instancingArgs[0] = particleMesh.GetIndexCount(0);
                _instancingArgs[1] = (uint) Solver.ParticleNum;
                _instancingArgs[2] = particleMesh.GetIndexStart(0);
                _instancingArgs[3] = particleMesh.GetBaseVertex(0);
            }

            _instancingArgsBuffer = new ComputeBuffer(1, _instancingArgs.Length * sizeof(uint),
                ComputeBufferType.IndirectArguments);
            _instancingArgsBuffer.SetData(_instancingArgs);
            instancingBounds = new Bounds(Solver.transform.position, Vector3.one * InstancingBoundSize);
            particleDepthMat.SetBuffer("ParticleBuffer", Solver.PartilceBuffer);
            particleDepthMat.enableInstancing = true;
        }

        private void OnDisable()
        {
            _instancingArgsBuffer.Release();
        }

        private void OnDestroy()
        {
            _instancingArgsBuffer.Release();
        }
    }

}
