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
    
    void Start()
    {
        var maxPosition = transform.position + transform.localScale / 2;
        var minPosition = transform.position - transform.localScale / 2; 
        
        _drawArgsBuffer = CreateDrawArgsBufferForRenderMeshIndirect(_mesh, _count);
        _dataBuffer = CreateDataBuffer(_count);
        
        var matrix4X4Array = CreateMatrix4x4.CreateMatrix4x4Array(_count, maxPosition, minPosition);
        _dataBuffer.SetData(matrix4X4Array);
        
        _material.SetBuffer("_MatricesArray", _dataBuffer);
        _material.SetVector("_BoundsOffset", transform.position);
        
        matrix4X4Array.Dispose();
    }

    // Update is called once per frame
    void Update()
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
        if (_drawArgsBuffer != null)
            _drawArgsBuffer.Dispose();
        
        if (_dataBuffer != null)
            _dataBuffer.Dispose();
    }
    
    private static GraphicsBuffer CreateDrawArgsBufferForRenderMeshIndirect(Mesh mesh, int instanceCount)
    {
        var drawArgsBuffer = new GraphicsBuffer(
            GraphicsBuffer.Target.IndirectArguments,
            1,
            GraphicsBuffer.IndirectDrawIndexedArgs.size
        );

        var commandData = new GraphicsBuffer.IndirectDrawIndexedArgs[1];
        commandData[0] = new GraphicsBuffer.IndirectDrawIndexedArgs
        {
            indexCountPerInstance = mesh.GetIndexCount(0),
            instanceCount = (uint)instanceCount,
            startIndex = mesh.GetIndexStart(0),
            baseVertexIndex = mesh.GetBaseVertex(0),
        };

        drawArgsBuffer.SetData(commandData);

        return drawArgsBuffer;
    }
    
    private static GraphicsBuffer CreateDataBuffer(int instanceCount)
    {
        return new GraphicsBuffer(
            GraphicsBuffer.Target.Structured, instanceCount,
            Marshal.SizeOf(typeof(Matrix4x4))
        );
    }
}
