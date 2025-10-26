using System.Drawing;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
public class NoiseWrapJob {
    private NativeArray<NoiseData> input;
    private NativeArray<NoiseData> output;
    public int size;
    public JobHandle jobHandle;
    public bool jobScheduled = false;
    public NoiseWrapJob(int size, NativeArray<NoiseData> input) {
        this.size = size;
        this.input = input;
    }

    public void StartJob() {
        if (jobScheduled) {
            Debug.LogWarning("Terrain job already running!");
            return;
        }

        output = new NativeArray<NoiseData>(input.Length, Allocator.Persistent);

        NoiseWrapParallelJob job = new NoiseWrapParallelJob {
            input = input,
            output = output,
            size = size,
            blendWidth = 8
        };

        jobHandle = job.Schedule(output.Length, 64);
        jobScheduled = true;
    }
    public NativeArray<NoiseData> CompleteJob() {
        jobHandle.Complete();
        jobScheduled = false;
        return output;
    }

    public void Dispose() {
        output.Dispose();
        jobScheduled = false;
    }
    public bool IsJobComplete() {
        if (!jobScheduled)
            return false;
        return jobHandle.IsCompleted;
    }

    public void OnDestroy() {
        if (jobScheduled) {
            jobHandle.Complete();
            if (output.IsCreated)
                output.Dispose();
        }
    }
}
[BurstCompile]
public struct NoiseWrapParallelJob : IJobParallelFor {
    [ReadOnly] public NativeArray<NoiseData> input;
    public NativeArray<NoiseData> output;

    public int size;
    public int blendWidth;

    public void Execute(int index) {
        int x = index % size;
        int z = index / size;

        NoiseData n = input[index];

        float tX = 0f;
        if (x < blendWidth) tX = (float)(blendWidth - x) / blendWidth;
        else if (x >= size - blendWidth) tX = (float)(x - (size - blendWidth) + 1) / blendWidth;

        float tZ = 0f;
        if (z < blendWidth) tZ = (float)(blendWidth - z) / blendWidth;
        else if (z >= size - blendWidth) tZ = (float)(z - (size - blendWidth) + 1) / blendWidth;

        if (tX > 0f || tZ > 0f) {
            // Determine opposite edge pixels
            int oppX = (x < blendWidth) ? size - blendWidth + x :
                       (x >= size - blendWidth) ? x - (size - blendWidth) : x;

            int oppZ = (z < blendWidth) ? size - blendWidth + z :
                       (z >= size - blendWidth) ? z - (size - blendWidth) : z;

            NoiseData opposite = input[oppX + oppZ * size];

            // For corners: use bilinear blending
            float weight = math.saturate(tX * tZ); // multiplies X and Z blending

            n = LerpNoiseData(n, opposite, weight);
        }

        output[index] = n;
    }

    private static NoiseData LerpNoiseData(NoiseData a, NoiseData b, float t) {
        return new NoiseData {
            height = math.lerp(a.height, b.height, t),
            temperature = math.lerp(a.temperature, b.temperature, t),
            humidity = math.lerp(a.humidity, b.humidity, t)
        };
    }
}


