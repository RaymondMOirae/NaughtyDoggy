using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
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

        private int _verticalPass = 1;
        private int _horizontalPass = 2;


        [Range(1.0f, 2.0f)] public float ParticleScale;
        public float BlurRange;
        [Range(0.0f, 1.0f)] public float BlurScale;
        public float BlurDepthFallOff;
        public int IterationPassNum;

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
            RenderTexture _blurResult = RenderTexture.GetTemporary(src.width, src.height, 24, RenderTextureFormat.ARGBFloat);
            particleShadingMat.SetTexture("_MainTex", src);
            particleShadingMat.SetFloat("BlurRange", BlurRange);
            particleShadingMat.SetFloat("BlurScale", BlurScale);
            particleShadingMat.SetFloat("BlurDepthFallOff", BlurDepthFallOff);
            
            Graphics.Blit(src, _blurResult);
            

            for (int i = 0; i < IterationPassNum; i++)
            {
                RenderTexture temp = RenderTexture.GetTemporary(src.width, src.height, 24, RenderTextureFormat.ARGBFloat);
                
                Graphics.Blit(_blurResult, temp, particleShadingMat, _horizontalPass);
                
                RenderTexture.ReleaseTemporary(_blurResult);
                _blurResult = temp;
            
                temp = RenderTexture.GetTemporary(src.width, src.height, 24, RenderTextureFormat.ARGBFloat);
                
                Graphics.Blit(_blurResult, temp, particleShadingMat, _verticalPass);
                RenderTexture.ReleaseTemporary(_blurResult);
                _blurResult = temp;
            }
            
            
            Graphics.Blit(_blurResult, dest, particleShadingMat, 3);
            // Graphics.Blit(_blurResult, dest);
            RenderTexture.ReleaseTemporary(_blurResult);
            //Graphics.Blit(src, dest);
        }

        private void OnPreRender()
        {
            // DrawParticle();
        }

        private void DrawParticle()
        {
            particleDepthMat.SetFloat("ParticleScale", ParticleScale);
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
