using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class NoiseGenJobHandler {
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
    public NoiseGenJobHandler(int size, PlanetNoiseSettings noiseSettings) {
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

        NoiseGenJob job = new NoiseGenJob {
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
    public bool IsJobComplete() {
        if (!jobScheduled)
            return false;
        return jobHandle.IsCompleted;
    }
    public void Dispose() {
        if (noiseDatas.IsCreated) {
            noiseDatas.Dispose();
        }
        jobScheduled = false;
    }
    public void OnDestroy() {
        if (jobScheduled) {
            if (!jobHandle.IsCompleted) jobHandle.Complete();
        }
        Dispose();
    }
}
