using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.LightTransport;

public class NoiseGenJob {
    private NativeArray<NoiseData> noiseDatas;
    public JobHandle jobHandle;
    public bool jobScheduled = false;
    public int size;
    public float heightScale;
    public int heightOctaves;
    public float heightLacunarity;
    public float heightPersistance;
    public float tempScale;
    public int tempOctaves;
    public float tempLacunarity;
    public float tempPersistance;
    public float humidScale;
    public int humidOctaves;
    public float humidLacunarity;
    public float humidPersistance;
    public NoiseGenJob(int size, PlanetNoiseSettings noiseSettings) {
        this.size = size;
        heightScale = noiseSettings.height.scale;
        heightOctaves = noiseSettings.height.octaves;
        heightLacunarity = noiseSettings.height.lacunarity;
        heightPersistance = noiseSettings.height.persistance;
        tempScale = noiseSettings.temperature.scale;
        tempOctaves = noiseSettings.temperature.octaves;
        tempLacunarity = noiseSettings.temperature.lacunarity;
        tempPersistance = noiseSettings.temperature.persistance;
        humidScale = noiseSettings.humidity.scale;
        humidOctaves = noiseSettings.humidity.octaves;
        humidLacunarity = noiseSettings.humidity.lacunarity;
        humidPersistance = noiseSettings.humidity.persistance;
    }

    public void StartJob() {
        if (jobScheduled) {
            Debug.LogWarning("Terrain job already running!");
            return;
        }

        noiseDatas = new NativeArray<NoiseData>(size * size, Allocator.Persistent);

        NoiseParallelJob job = new NoiseParallelJob {
            width = size,
            height = size,
            noiseDatas = noiseDatas,
            heightScale = heightScale,
            heightOctaves = heightOctaves,
            heightLacunarity = heightLacunarity,
            heightPersistance = heightPersistance,
            tempScale = tempScale,
            tempOctaves = tempOctaves,
            tempLacunarity = tempLacunarity,
            tempPersistance = tempPersistance,
            humidScale = humidScale,
            humidOctaves = humidOctaves,
            humidLacunarity = humidLacunarity,
            humidPersistance = humidPersistance,
        };

        jobHandle = job.Schedule(noiseDatas.Length, 64);
        jobScheduled = true;
    }
    public NativeArray<NoiseData> CompleteJob() {
        jobHandle.Complete();
        jobScheduled = false;
        return noiseDatas;
    }

    public void Dispose() {
        noiseDatas.Dispose();
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
            if (noiseDatas.IsCreated)
                noiseDatas.Dispose();
        }
    }
}

public struct NoiseData
{
    public float height;
    public float temperature;
    public float humidity;
}

[BurstCompile]
public struct NoiseParallelJob : IJobParallelFor {
    public int width;
    public int height;

    public float heightScale;
    public int heightOctaves;
    public float heightLacunarity;
    public float heightPersistance;

    public float tempScale;
    public int tempOctaves;
    public float tempLacunarity;
    public float tempPersistance;

    public float humidScale;
    public int humidOctaves;
    public float humidLacunarity;
    public float humidPersistance;

    [WriteOnly]
    public NativeArray<NoiseData> noiseDatas;

    public void Execute(int index) {
        int localX = index % width;
        int localZ = index / width;

        // World-space coordinates (normalized)
        float worldX = (float)localX / width;
        float worldZ = (float)localZ / height;
        float2 samplePos = new float2(localX, localZ);

        // === Sample controlling noises ===
        float baseHeight = FBM(samplePos * heightScale, heightOctaves, heightLacunarity, heightPersistance);
        float temperature = FBM(samplePos * tempScale, tempOctaves, tempLacunarity, tempPersistance);
        float humidity = FBM(samplePos * humidScale, humidOctaves, humidLacunarity, humidPersistance);

        // === Simulate elevation affecting temperature ===
        temperature *= (1f - baseHeight * 0.5f);

        // === Store result ===
        noiseDatas[index] = new NoiseData {
            height = baseHeight,
            temperature = temperature,
            humidity = humidity
        };
    }

    float FBM(float2 pos, int octaves, float lacunarity, float gain) {
        float sum = 0f;
        float amp = 1f;
        float freq = 1f;
        float amplitudeSum = 0f;

        for (int i = 0; i < octaves; i++) {
            sum += noise.snoise(pos * freq) * amp;
            amplitudeSum += amp;

            freq *= lacunarity;
            amp *= gain;
        }

        float normalized = sum / amplitudeSum;
        return math.saturate(normalized * 0.5f + 0.5f);
    }


}