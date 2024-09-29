using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class TransformMatrixArrayFactory
{
    public static NativeArray<Matrix4x4> Create(
        int count,
        float3 maxPosition,
        float3 minPosition 
    )
    {
        var transformMatrixArray = new NativeArray<Matrix4x4>(count, Allocator.Persistent);
        var job = new InitializeMatrixJob
        {
            _transformMatrixArray = transformMatrixArray,
            _maxPosition = maxPosition,
            _minPosition = minPosition
        };

        var jobHandle = job.Schedule(count, 64);
        jobHandle.Complete();

        return transformMatrixArray;
    }

    [BurstCompile]
    private struct InitializeMatrixJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<Matrix4x4> _transformMatrixArray;
        [ReadOnly] public float3 _maxPosition;
        [ReadOnly] public float3 _minPosition;

        public void Execute(int index)
        {
            var random = new Unity.Mathematics.Random((uint)index + 1);
            var x = random.NextFloat(_minPosition.x, _maxPosition.x);
            var y = random.NextFloat(_minPosition.y, _maxPosition.y);
            var z = random.NextFloat(_minPosition.z, _maxPosition.z);

            _transformMatrixArray[index] = Matrix4x4.TRS(new Vector3(x, y, z), random.NextQuaternionRotation(), Vector3.one);
        }
    }
}
