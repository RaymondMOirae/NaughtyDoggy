using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

namespace NaughtyDoggy.Fluid
{
    public class PBF_FluidNormalRenderer : MonoBehaviour
    {
        public RenderTexture FluidNormalTexture;
        [HideInInspector] public Vector2Int ScreenPixelSize;
        
        private int _verticalFilterPassIndex = 1;
        private int _horizontalFilterPassIndex = 2;
        private int _normalReconstructPassIndex = 3;
        
        private readonly uint[] _instancingArgs = new uint[5] {0, 0, 0, 0, 0};
        private ComputeBuffer _instancingArgsBuffer;
        private Bounds _instancingBounds;
        
        private Camera _depthCamera;
        
        [SerializeField] private int iterationPassNum;
        [Range(1.0f, 2.0f)] 
        [SerializeField] private float particleScale;
        [Range(0.0f, 0.5f)] 
        [SerializeField] private float blurScale;
        [SerializeField] private float blurRange;
        [SerializeField] private float blurDepthFallOff;
        
        [SerializeField] private int fluidLayer;
        [SerializeField] private float instancingBoundSize; 
        [SerializeField] private PBF_Solver fluidSolver;
        [SerializeField] private GameObject particleContainer;

        [SerializeField] private Material particleDepthMat;
        [SerializeField] private Material particleShadingMat;
        
        [SerializeField] private Mesh particleMesh;

        // Start is called before the first frame update
        void Start()
        {
            _depthCamera = GetComponent<Camera>(); 
            _depthCamera.clearFlags = CameraClearFlags.Color;
            
            ScreenPixelSize = new Vector2Int(_depthCamera.pixelWidth, _depthCamera.pixelHeight);
            FluidNormalTexture = RenderTexture.GetTemporary(ScreenPixelSize.x, ScreenPixelSize.y, 24,
                                                                          RenderTextureFormat.ARGBFloat);
            InitInstancingBuffer();
        }
        
        // Update is called once per frame
        void Update()
        {
            DrawParticle();
        }

        private void OnRenderImage(RenderTexture src, RenderTexture dest)
        {
            particleShadingMat.SetTexture("_MainTex", src);
            particleShadingMat.SetFloat("BlurRange", blurRange);
            particleShadingMat.SetFloat("BlurScale", blurScale);
            particleShadingMat.SetFloat("BlurDepthFallOff", blurDepthFallOff);
            
            Graphics.Blit(src, FluidNormalTexture);
            
            for (int i = 0; i < iterationPassNum; i++)
            {
                RenderTexture temp = RenderTexture.GetTemporary(src.width, src.height, 24, RenderTextureFormat.ARGBFloat);
                
                Graphics.Blit(FluidNormalTexture, temp, particleShadingMat, _horizontalFilterPassIndex);
                
                RenderTexture.ReleaseTemporary(FluidNormalTexture);
                FluidNormalTexture = temp;
            
                temp = RenderTexture.GetTemporary(src.width, src.height, 24, RenderTextureFormat.ARGBFloat);
                
                Graphics.Blit(FluidNormalTexture, temp, particleShadingMat, _verticalFilterPassIndex);
                RenderTexture.ReleaseTemporary(FluidNormalTexture);
                FluidNormalTexture = temp;
            }
            
            Graphics.Blit(FluidNormalTexture, dest, particleShadingMat,_normalReconstructPassIndex);
        }

        private void DrawParticle()
        {
            particleDepthMat.SetFloat("ParticleScale", particleScale);
            particleDepthMat.SetBuffer("ParticleBuffer", fluidSolver.PartilceBuffer);
            particleDepthMat.SetVector("ParentPos", particleContainer.transform.position);
            particleDepthMat.SetVector("ParentScale", particleContainer.transform.lossyScale);
            Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleDepthMat, _instancingBounds,
                _instancingArgsBuffer, 0, null, ShadowCastingMode.Off, false, fluidLayer, _depthCamera);
        }
        
        private void InitInstancingBuffer()
        {
            if (particleMesh != null)
            {
                _instancingArgs[0] = particleMesh.GetIndexCount(0);
                _instancingArgs[1] = (uint) fluidSolver.ParticleNum;
                _instancingArgs[2] = particleMesh.GetIndexStart(0);
                _instancingArgs[3] = particleMesh.GetBaseVertex(0);
            }

            _instancingArgsBuffer = new ComputeBuffer(1, _instancingArgs.Length * sizeof(uint),
                ComputeBufferType.IndirectArguments);
            _instancingArgsBuffer.SetData(_instancingArgs);
            _instancingBounds = new Bounds(fluidSolver.transform.position, Vector3.one * instancingBoundSize);
            particleDepthMat.SetBuffer("ParticleBuffer", fluidSolver.PartilceBuffer);
            particleDepthMat.enableInstancing = true;
        }

        private void OnDisable()
        {
            _instancingArgsBuffer.Release();
            FluidNormalTexture.Release();
        }

        private void OnDestroy()
        {
            _instancingArgsBuffer.Release();
            FluidNormalTexture.Release();
        }
    }

}
