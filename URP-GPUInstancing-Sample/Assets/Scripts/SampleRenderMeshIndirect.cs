using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

public class SampleRenderMeshIndirect : MonoBehaviour
{
    [SerializeField] private int _count;
    [SerializeField] private Mesh _mesh;
    [SerializeField] private Material _material;
    [SerializeField] private ShadowCastingMode _shadowCastingMode;
    [SerializeField] private bool _receiveShadows;
    
    private GraphicsBuffer _drawArgsBuffer;
    private GraphicsBuffer _dataBuffer;
    
    private void Start()
    {
        var maxPosition = transform.position + transform.localScale / 2;
        var minPosition = transform.position - transform.localScale / 2; 
        
        _drawArgsBuffer = CreateDrawArgsBufferForRenderMeshIndirect(_mesh, _count);
        _dataBuffer = CreateDataBuffer<Matrix4x4>(_count);

        var transformMatrixArray = TransformMatrixArrayFactory.Create(_count, maxPosition, minPosition);
        _dataBuffer.SetData(transformMatrixArray);
        
        _material.SetBuffer("_TransformMatrixArray", _dataBuffer);
        _material.SetVector("_BoundsOffset", transform.position);

        transformMatrixArray.Dispose();
    }

    private void Update()
    {
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
    
    private static GraphicsBuffer CreateDataBuffer<T>(int instanceCount) where T : struct
    {
        return new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, instanceCount,
            Marshal.SizeOf(typeof(T))
        );
    }
}
