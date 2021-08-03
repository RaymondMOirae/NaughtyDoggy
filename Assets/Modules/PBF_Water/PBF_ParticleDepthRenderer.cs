using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

namespace NaughtyDoggy.Fluid
{
    public class PBF_ParticleDepthRenderer : MonoBehaviour
    {
        public Shader ParticleDepthShader;
        public Material ParticleDepthMat;
        public Camera DepthCamera;
        public int FluidLayer;
        public PBF_Solver Solver;
        
        private readonly uint[] _instancingArgs = new uint[5] {0, 0, 0, 0, 0};
        private ComputeBuffer _instancingArgsBuffer;
        private Bounds instancingBounds;
        public float InstancingBoundSize;
        
        [SerializeField] private Mesh particleMesh;
        [SerializeField] private Material particleMat;
        [SerializeField] private RenderTexture ParticleTexture;

        // Start is called before the first frame update
        void Start()
        {
            // DepthCamera.enabled = false;
            // DepthCamera.cullingMask = 1 << FluidLayer;
            DepthCamera.clearFlags = CameraClearFlags.Depth;
            
            if (ParticleDepthShader != null)
            {
                // Shader.SetGlobalBuffer("ParticleBuffer", Solver.PartilceBuffer);
                // DepthCamera.SetReplacementShader(ParticleDepthShader, "Opaque");
            }
            
            InitInstancingBuffer();
        }
        
        // Update is called once per frame
        void Update()
        {
            DrawParticle();
        }
        
        private void DrawParticle()
        {
            particleMat.SetBuffer("ParticleBuffer", Solver.PartilceBuffer);
            Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMat, instancingBounds,
                _instancingArgsBuffer, 0, null, ShadowCastingMode.On, true, FluidLayer, DepthCamera);
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
            particleMat.SetBuffer("ParticleBuffer", Solver.PartilceBuffer);
            particleMat.enableInstancing = true;
        }

        private void OnPreRender()
        {
            
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
