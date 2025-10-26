using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

[BurstCompile]
public struct NoiseGenJob : IJobParallelFor {
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

        float u = (float)localX / width;
        float v = (float)localZ / height;

        float2 xCircle = new float2(math.cos(u * math.PI * 2f), math.sin(u * math.PI * 2f)) * width;
        float2 zCircle = new float2(math.cos(v * math.PI * 2f), math.sin(v * math.PI * 2f)) * height;

        float2 samplePos = xCircle + zCircle;

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