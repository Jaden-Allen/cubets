using UnityEngine;

[System.Serializable]
public class PlanetNoiseSettings {
    public NoiseParameter height = new();
    public NoiseParameter temperature = new();
    public NoiseParameter humidity = new();

    [System.Serializable]
    public class NoiseParameter {
        public float scale;
        public int octaves;
        public float lacunarity;
        public float persistance;
    }
}
