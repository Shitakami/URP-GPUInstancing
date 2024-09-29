using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public static class CreateMatrix4x4
{
    public static NativeArray<Matrix4x4> CreateMatrix4x4Array(
        int count,
        float3 maxPosition,
        float3 minPosition 
    )
    {
        var matrix4x4Array = new NativeArray<Matrix4x4>(count, Allocator.Persistent);
        var job = new InitializeMatrixJob
        {
            matrix4x4Array = matrix4x4Array,
            maxPosition = maxPosition,
            minPosition = minPosition
        };

        JobHandle jobHandle = job.Schedule(count, 64);
        jobHandle.Complete();

        return matrix4x4Array;
    }

    private struct InitializeMatrixJob : IJobParallelFor
    {
        public NativeArray<Matrix4x4> matrix4x4Array;
        public float3 maxPosition;
        public float3 minPosition;

        public void Execute(int index)
        {
            var random = new Unity.Mathematics.Random((uint)index + 1);
            var x = random.NextFloat(minPosition.x, maxPosition.x);
            var y = random.NextFloat(minPosition.y, maxPosition.y);
            var z = random.NextFloat(minPosition.z, maxPosition.z);

            matrix4x4Array[index] = Matrix4x4.TRS(new Vector3(x, y, z), random.NextQuaternionRotation(), Vector3.one);
        }
    }
    
}
