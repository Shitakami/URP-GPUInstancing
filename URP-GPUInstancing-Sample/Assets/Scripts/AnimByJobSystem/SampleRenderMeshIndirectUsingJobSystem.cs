using System.Runtime.InteropServices;
using AnimationSettings;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Rendering;

public class SampleRenderMeshIndirectUsingJobSystem : MonoBehaviour
{
    [SerializeField] private int _count;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField] private ShadowCastingMode _shadowCastingMode;
    [SerializeField] private bool _receiveShadows;
    
    [SerializeField] private CubeAnimationSettings _settings;
    
    private NativeArray<Matrix4x4> _transformMatrixArray;
    
    private GraphicsBuffer _drawArgsBuffer;
    private GraphicsBuffer _dataBuffer;

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
    }

    private void Update()
    {
        var job = new CubeAnimationJob(
            (transform.position + transform.localScale / 2).y,
            (transform.position - transform.localScale / 2).y,
            _settings.MoveVelocity,
            _settings.RotationVelocity,
            Time.deltaTime,
            _transformMatrixArray
        );

        var jobHandle = job.Schedule(_count, 64);
        jobHandle.Complete();
        
        _dataBuffer.SetData(_transformMatrixArray);

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