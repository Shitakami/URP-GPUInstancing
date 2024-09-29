using System.Runtime.InteropServices;
using AnimationSettings;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;

public class SampleRenderMeshIndirectUsingComputeShader : MonoBehaviour
{
    [SerializeField] private int _count;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField] private ShadowCastingMode _shadowCastingMode;
    [SerializeField] private bool _receiveShadows;
    
    [SerializeField] private CubeAnimationSettings _settings;
    [SerializeField] private ComputeShader _computeShader;
    
    private NativeArray<Matrix4x4> _transformMatrixArray;
    
    private GraphicsBuffer _drawArgsBuffer;
    private GraphicsBuffer _dataBuffer;
    
    private int _kernelIndex;
    private static readonly int TransformMatrixArray = Shader.PropertyToID("TransformMatrixArray");
    private static readonly int TransformMatrixArrayLength = Shader.PropertyToID("TransformMatrixArrayLength");
    private static readonly int InstancedHeight = Shader.PropertyToID("InstancedHeight");
    private static readonly int DestroyHeight = Shader.PropertyToID("DestroyHeight");
    private static readonly int MoveVelocity = Shader.PropertyToID("MoveVelocity");
    private static readonly int RotationVelocity = Shader.PropertyToID("RotationVelocity");
    private static readonly int DeltaTime = Shader.PropertyToID("DeltaTime");

    private void Start()
    {
        var maxPosition = transform.position + transform.localScale / 2;
        var minPosition = transform.position - transform.localScale / 2; 
        
        _drawArgsBuffer = CreateDrawArgsBufferForRenderMeshIndirect(_mesh, _count);
        _dataBuffer = CreateDataBuffer<Matrix4x4>(_count);
        
        _transformMatrixArray = TransformMatrixArrayFactory.Create(_count, maxPosition, minPosition);
        _dataBuffer.SetData(_transformMatrixArray);
        
        _material.SetBuffer("_TransformMatrixArray", _dataBuffer);
        _material.SetVector("_BoundsOffset", transform.position);
        
        _kernelIndex = _computeShader.FindKernel("CubeAnimation");
        _computeShader.SetBuffer(_kernelIndex, TransformMatrixArray, _dataBuffer);
        _computeShader.SetInt(TransformMatrixArrayLength, _count);
        _computeShader.SetFloat(InstancedHeight, maxPosition.y);
        _computeShader.SetFloat(DestroyHeight, minPosition.y);
        _computeShader.SetFloat(MoveVelocity, _settings.MoveVelocity);
        _computeShader.SetFloat(RotationVelocity, _settings.RotationVelocity);
    }

    private void Update()
    {
        _computeShader.SetFloat(DeltaTime, Time.deltaTime);
        _computeShader.GetKernelThreadGroupSizes(_kernelIndex, out var threadGroupSizeX, out _, out _);
        var threadGroups = Mathf.CeilToInt(_count / (float)threadGroupSizeX);
        _computeShader.Dispatch(_kernelIndex, threadGroups, 1, 1);

        var renderParams = new RenderParams(_material)
        {
            receiveShadows = _receiveShadows,
            shadowCastingMode = _shadowCastingMode,
            worldBounds = new Bounds(transform.position, transform.localScale)
        };

        Graphics.RenderMeshIndirect(
            renderParams,
            _mesh,
            _drawArgsBuffer
        );
    }

    private void OnDestroy()
    {
        _drawArgsBuffer?.Dispose();
        _dataBuffer?.Dispose();
        _transformMatrixArray.Dispose();
    }

    private static GraphicsBuffer CreateDrawArgsBufferForRenderMeshIndirect(Mesh mesh, int instanceCount)
    {
        var commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        commandData[0] = new GraphicsBuffer.IndirectDrawIndexedArgs
        {
            indexCountPerInstance = mesh.GetIndexCount(0),
            instanceCount = (uint)instanceCount,
            startIndex = mesh.GetIndexStart(0),
            baseVertexIndex = mesh.GetBaseVertex(0),
        };

        var drawArgsBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.IndirectArguments,
            1,
            GraphicsBuffer.IndirectDrawIndexedArgs.size
        );
        drawArgsBuffer.SetData(commandData);

        return drawArgsBuffer;
    }
    
    private static GraphicsBuffer CreateDataBuffer<T>(int count) where T : struct
    {
        return new GraphicsBuffer(GraphicsBuffer.Target.Structured, count, Marshal.SizeOf<T>());
    }
}
