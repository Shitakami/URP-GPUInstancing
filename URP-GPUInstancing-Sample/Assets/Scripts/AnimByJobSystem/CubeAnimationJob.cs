using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

[BurstCompile]
public struct CubeAnimationJob : IJobParallelFor
{
    [ReadOnly] private readonly float _instancedHeight;
    [ReadOnly] private readonly float _destroyHeight;
    [ReadOnly] private readonly float _moveVelocity;
    [ReadOnly] private readonly float _rotationVelocity;
    [ReadOnly] private readonly float _deltaTime;
    private NativeArray<Matrix4x4> _transformMatrixArray;
    
    public CubeAnimationJob(
        float instancedHeight,
        float destroyHeight,
        float moveVelocity,
        float rotationVelocity,
        float deltaTime,
        NativeArray<Matrix4x4> transformMatrixArray
    )
    {
        _instancedHeight = instancedHeight;
        _destroyHeight = destroyHeight;
        _moveVelocity = moveVelocity;
        _rotationVelocity = rotationVelocity;
        _deltaTime = deltaTime;
        _transformMatrixArray = transformMatrixArray;
    }

    public void Execute(int index)
    {
        var position = _transformMatrixArray[index].GetPosition();
        var rotation = _transformMatrixArray[index].rotation;
        var scale = _transformMatrixArray[index].lossyScale;
        
        var random = new Unity.Mathematics.Random((uint)index + 1);
        var rotateAxis = random.NextFloat3Direction();
        
        position.y -= _moveVelocity * _deltaTime;
        rotation *= quaternion.AxisAngle(rotateAxis, _rotationVelocity * _deltaTime);
        
        if (position.y < _destroyHeight)
        {
            position.y = _instancedHeight;
        }

        _transformMatrixArray[index] = Matrix4x4.TRS(position, rotation, scale);
    }
}