
using NaughtyDoggy.Helper;
using UnityEngine;

using Random = UnityEngine.Random;

public class PBF_Solver : MonoBehaviour
{
    private struct ParticleData
    {
        public Vector3 curPosition;
        public Vector3 nextPosition;
        public Vector3 deltaPosition;
        public Vector3 extForce;
        public Vector3 velocity;
        public float lambda;
    }
    private int ParticleDataSize => sizeof(float) * (3 * 5 + 1);
    
    [SerializeField] private int solverIteration = 3;
    [SerializeField] private float restDensity = 1.0f;
    [SerializeField] private int particleNum = 3200;
    [SerializeField] private int wallParticleNum = 0;
    [SerializeField] private Vector4 gravity = Vector4.zero;
    [SerializeField] private float DeltaTime => Time.deltaTime;
    [SerializeField] private float particleMass;
    
    [SerializeField] private float interactRadius;
    [SerializeField] private float neighborRadius;
    
    [SerializeField] private float k = 1.0f;
    [SerializeField] private float n = 4.0f;
    [SerializeField] private float epsilonLambda = 1.0f;
    [SerializeField] private float c = 0.01f;
    private float Poly6Term => 315.0f / (64.0f * Mathf.PI * Mathf.Pow(interactRadius, 9.0f));
    private float SpikyTerm => -45.0f / (Mathf.PI * Mathf.Pow(interactRadius, 6.0f));
    private float DeltaQ => 0.2f * interactRadius;
    private float WDeltaQ => Poly6Term * Mathf.Pow(interactRadius * interactRadius - DeltaQ * DeltaQ, 3.0f);
    
    [SerializeField] private Vector4 boundaryMax; 
    [SerializeField] private Vector4 boundaryMin; 
    
    [SerializeField] private Bounds instancingBounds;
    [SerializeField] private Mesh particleMesh;
    [SerializeField] private Material particleMat;
    
    private ParticleData[] _particleData;
    private ParticleData[] _wallParticleData;
    private readonly uint[] _instancingArgs = new uint[5] {0, 0, 0, 0, 0};
    private ComputeBuffer _instancingArgsBuffer;

    private ComputeBuffer _particleBufferREAD;
    private ComputeBuffer _particleBufferWRITE;
    private ComputeBuffer _wallParticleBuffer;
    
    private int _predictPositionKernel;
    private int _calculateLambdaKernel;
    private int _calculateDeltaPosKernel;
    private int _updatePositionKernel;
    [SerializeField] private ComputeShader pbfSimulationCS;

    void Start()
    {
        SpawnFluidParticles();
        // SpawnWalls();
        InitSimulatorComputeBuffer();
        InitInstancingBuffer();
    }
    
    private void Update()
    {
        SetSimulatorValues();
        ComputeParticleBuffer();
        DrawParticle();
    }

    private void InitSimulatorComputeBuffer()
    {
        _predictPositionKernel = pbfSimulationCS.FindKernel("PredictPosition");
        _calculateLambdaKernel = pbfSimulationCS.FindKernel("CalculateLambda");
        _calculateDeltaPosKernel = pbfSimulationCS.FindKernel("CalculateDeltaPos");
        _updatePositionKernel = pbfSimulationCS.FindKernel("UpdatePosition");
        _pbfBufferSwapPass = pbfSimulationCS.FindKernel("BufferSwap");
        _particleBufferREAD = new ComputeBuffer(particleNum, ParticleDataSize, ComputeBufferType.Default);
        _particleBufferWRITE = new ComputeBuffer(particleNum, ParticleDataSize, ComputeBufferType.Default);
        // _wallParticleBuffer = new ComputeBuffer(wallParticleNum, ParticleDataSize, ComputeBufferType.Default);
        _particleBufferREAD.SetData(_particleData);
        _particleBufferWRITE.SetData(_particleData);
        // _wallParticleBuffer.SetData(_wallParticleData);
        SetSimulatorValues();
    }

    private void SetSimulatorValues()
    {
        pbfSimulationCS.SetInt("ParticleNum", particleNum);
        // pbfSimulationCS.SetInt("WallParticleNum", wallParticleNum);
        pbfSimulationCS.SetInt("SolverIteration", solverIteration);
        pbfSimulationCS.SetFloat("RestDensity", restDensity);
        pbfSimulationCS.SetFloat("DeltaTime", DeltaTime);
        pbfSimulationCS.SetFloat("ParticleMass", particleMass);
        pbfSimulationCS.SetFloat("KernelRadius", interactRadius);
        pbfSimulationCS.SetFloat("KernelRadius_SQU", interactRadius * interactRadius);
        //pbfSimulationCS.SetFloat("InteractRadius_CUBE", interactRadius * interactRadius * interactRadius);
        pbfSimulationCS.SetFloat("DeltaQ", DeltaQ);
        pbfSimulationCS.SetFloat("NeighborRadius", neighborRadius);
        pbfSimulationCS.SetFloat("K", k);
        pbfSimulationCS.SetFloat("N", n);
        pbfSimulationCS.SetFloat("EpsilonLambda", epsilonLambda);
        pbfSimulationCS.SetFloat("C", c);
        pbfSimulationCS.SetFloat("Poly6Term", Poly6Term);
        pbfSimulationCS.SetFloat("SpikyTerm", SpikyTerm);
        pbfSimulationCS.SetFloat("WDeltaQ", WDeltaQ);
        pbfSimulationCS.SetVector("Gravity", gravity);
        pbfSimulationCS.SetVector("BoundaryMax", boundaryMax);
        pbfSimulationCS.SetVector("BoundaryMin", boundaryMin);
    }

    private void InitInstancingBuffer()
    {
        if (particleMesh != null)
        {
            _instancingArgs[0] = particleMesh.GetIndexCount(0);
            _instancingArgs[1] = (uint) particleNum;
            _instancingArgs[2] = particleMesh.GetIndexStart(0);
            _instancingArgs[3] = particleMesh.GetBaseVertex(0);
        }
        
        _instancingArgsBuffer = new ComputeBuffer(1, _instancingArgs.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _instancingArgsBuffer.SetData(_instancingArgs);
        
        particleMat.SetBuffer("ParticleBuffer", _particleBufferREAD);
        particleMat.enableInstancing = true;
    }

    private int _pbfBufferSwapPass;
    private void ComputeParticleBuffer()
    {
        DispatchKernel(_pbfBufferSwapPass);
        DispatchKernel(_predictPositionKernel);

        for (int i = 0; i < solverIteration; i++)
        {
            DispatchKernel(_calculateLambdaKernel);
            DispatchKernel(_calculateDeltaPosKernel);
        }
        
        DispatchKernel(_updatePositionKernel);

        // only used for debug
        // _particleBufferWRITE.GetData(_particleData);

        SwapBuffer(ref _particleBufferREAD, ref _particleBufferWRITE);
    }

    private void DispatchKernel(int kernel)
    {
        pbfSimulationCS.SetBuffer(kernel, "ParticleBufferREAD", _particleBufferREAD);
        pbfSimulationCS.SetBuffer(kernel, "ParticleBufferWRITE", _particleBufferWRITE);
        // pbfSimulationCS.SetBuffer(kernel, "WallParticleBuffer", _wallParticleBuffer);
        pbfSimulationCS.Dispatch(kernel, particleNum / 32, 1, 1);
    }

    void SwapBuffer(ref ComputeBuffer bufferREAD, ref ComputeBuffer bufferWRITE)
    {
        ComputeBuffer temp = bufferREAD;
        bufferREAD = bufferWRITE;
        bufferWRITE = temp;
    }

    private void DrawParticle()
    {
        particleMat.SetBuffer("ParticleBuffer", _particleBufferREAD);
        Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMat, instancingBounds,
            _instancingArgsBuffer);
        // particleMat.SetBuffer("ParticleBuffer", _wallParticleBuffer);
        // Graphics.DrawMeshInstancedIndirect(particleMesh, 0, particleMat, instancingBounds,
        //     _instancingArgsBuffer);
    }

    private void SpawnFluidParticles()
    {
        Vector3 boundarySpace = (boundaryMax - boundaryMin) * 0.9f;
        _particleData = new ParticleData[particleNum];
        instancingBounds = new Bounds(transform.position, Vector3.one * 200.0f);
        for (int i = 0; i < particleNum; i++)
        {
            Vector3 pos =  (Vector3) boundaryMin + 
                           MathHelper.Vec3Mul(MathHelper.Vec3Random(), boundarySpace);
            NewParticle(ref _particleData[i], pos);
        }
        
    }

    private void SpawnWalls()
    {
        Vector3 boundarySpace = boundaryMax - boundaryMin;
        float wallSpawnPadding = interactRadius / 4.0f;
        Vector3 boundaryNum = boundarySpace / wallSpawnPadding + Vector3.one;
        wallParticleNum = (int) (boundaryNum.x * boundaryNum.y * 2 + boundaryNum.x * boundaryNum.z * 2 +
                                 boundaryNum.y * boundaryNum.z * 2);
        
        _wallParticleData = new ParticleData[wallParticleNum];
        
        int _spawnedNum = 0;
        for (int i = 0; i < boundaryNum.x; i++)
        {
            for (int j = 0; j < boundaryNum.y; j++)
            {
                Vector3 posBut = new Vector3(i * wallSpawnPadding, j * wallSpawnPadding, 0.0f) + (Vector3) boundaryMin;
                NewParticle(ref _wallParticleData[_spawnedNum++], posBut);
                Vector3 posTop = new Vector3(i * wallSpawnPadding, j * wallSpawnPadding, boundarySpace.z) + (Vector3) boundaryMin;
                NewParticle(ref _wallParticleData[_spawnedNum++], posTop);
            }
        }

        for (int i = 0; i < boundaryNum.x; i++)
        {
            for (int j = 0; j < boundaryNum.z; j++)
            {
                Vector3 posBut = new Vector3(i * wallSpawnPadding, 0.0f, j * wallSpawnPadding) + (Vector3) boundaryMin;
                NewParticle(ref _wallParticleData[_spawnedNum++], posBut);
                Vector3 posTop = new Vector3(i * wallSpawnPadding, boundarySpace.y, j * wallSpawnPadding) + (Vector3) boundaryMin;
                NewParticle(ref _wallParticleData[_spawnedNum++], posTop);
            }
        }

        for (int i = 0; i < boundaryNum.y; i++)
        {
            for (int j = 0; j < boundaryNum.z; j++)
            {
                Vector3 posBut = new Vector3(0.0f, i * wallSpawnPadding, j * wallSpawnPadding) + (Vector3) boundaryMin;
                NewParticle(ref _wallParticleData[_spawnedNum++], posBut);
                Vector3 posTop = new Vector3(boundarySpace.x, i * wallSpawnPadding, j * wallSpawnPadding) + (Vector3) boundaryMin;
                NewParticle(ref _wallParticleData[_spawnedNum++], posTop);
            }
        }
    }

    private void NewParticle(ref ParticleData particle, Vector3 pos)
    {
            particle.curPosition = pos;
            particle.nextPosition = pos;
            particle.deltaPosition = Vector3.zero;
            particle.extForce = Vector3.zero;
            particle.velocity = Vector3.zero;
            particle.lambda = 0.0f;
    }

    private void OnDisable()
    {
        _instancingArgsBuffer.Release();
        _particleBufferREAD.Release();
        _particleBufferWRITE.Release();
        _wallParticleBuffer.Release();
    }

    private void OnDestroy()
    {
        _instancingArgsBuffer.Release();
        _particleBufferREAD.Release();
        _particleBufferWRITE.Release();
        _wallParticleBuffer.Release();
    }
}
