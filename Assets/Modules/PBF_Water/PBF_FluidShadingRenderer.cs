using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.Build.Content;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.Rendering;

namespace NaughtyDoggy.Fluid
{
    public class PBF_FluidShadingRenderer : MonoBehaviour
    {
        private RenderTexture _fluidTexture;
        private RenderTexture _camRenderTarget;
        public RenderTexture FluidTexture => _camRenderTarget;
        
        [HideInInspector] public Vector2Int ScreenPixelSize;
        
        private int _verticalFilterPassIndex = 1;
        private int _horizontalFilterPassIndex = 2;
        private int _normalReconstructPassIndex = 3;
        
        private readonly uint[] _instancingArgs = new uint[5] {0, 0, 0, 0, 0};
        private ComputeBuffer _instancingArgsBuffer;
        private Bounds _instancingBounds;
        
        private Camera _depthCamera;

        [SerializeField] private Color fluidSpecularColor;
        [SerializeField] private Color fluidDiffuseColor;
        [SerializeField] private Color reflectionColor;
        [SerializeField] private float gloss;
        [Range(0, 1)]
        [SerializeField] private float relectance;
        
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
            
            _camRenderTarget = RenderTexture.GetTemporary(ScreenPixelSize.x, ScreenPixelSize.y, 24,
                                                                          RenderTextureFormat.ARGBFloat);
            
            _fluidTexture = RenderTexture.GetTemporary(ScreenPixelSize.x, ScreenPixelSize.y, 24,
                                                                          RenderTextureFormat.ARGBFloat);
            _depthCamera.targetTexture = _camRenderTarget;
            InitInstancingBuffer();
        }
        
        // Update is called once per frame
        void Update()
        {
            DrawParticle();
        }

        private void OnPostRender()
        {
            particleShadingMat.SetTexture("_MainTex", _camRenderTarget);
            particleShadingMat.SetFloat("BlurRange", blurRange);
            particleShadingMat.SetFloat("BlurScale", blurScale);
            particleShadingMat.SetFloat("BlurDepthFallOff", blurDepthFallOff);
            particleShadingMat.SetVector("_SpecularColor", fluidSpecularColor);
            particleShadingMat.SetVector("_ReflectionColor", reflectionColor);
            particleShadingMat.SetVector("_DiffuseColor", fluidDiffuseColor);
            particleShadingMat.SetFloat("_Gloss", gloss);
            particleShadingMat.SetFloat("_Reflectance", relectance);

            Graphics.Blit(_camRenderTarget, _fluidTexture);

            
            for (int i = 0; i < iterationPassNum; i++)
            {
                RenderTexture temp = RenderTexture.GetTemporary(ScreenPixelSize.x, ScreenPixelSize.y, 24, RenderTextureFormat.ARGBFloat);
                
                Graphics.Blit(_fluidTexture, temp, particleShadingMat, _horizontalFilterPassIndex);
                
                RenderTexture.ReleaseTemporary(_fluidTexture);
                _fluidTexture = temp;
            
                temp = RenderTexture.GetTemporary(ScreenPixelSize.x, ScreenPixelSize.y, 24, RenderTextureFormat.ARGBFloat);
                
                Graphics.Blit(_fluidTexture, temp, particleShadingMat, _verticalFilterPassIndex);
                RenderTexture.ReleaseTemporary(_fluidTexture);
                _fluidTexture = temp;
            }
            
            Graphics.Blit(_fluidTexture, _camRenderTarget, particleShadingMat,_normalReconstructPassIndex);
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
            _fluidTexture.Release();
            _camRenderTarget.Release();
        }

        private void OnDestroy()
        {
            _instancingArgsBuffer.Release();
            _fluidTexture.Release();
            _camRenderTarget.Release();
        }
    }

}
