using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Terrain Feature", menuName = "Data/TerrainFeature")]
public class TerrainFeature : ScriptableObject
{
    [Range(0f, 1f)] public float threshold;
    public List<VoxelPlacement> blockPlacements = new List<VoxelPlacement>();
}
[System.Serializable]
public class VoxelPlacement {
    [Range(0f, 1f)] public float threshold;
    public BlockType type;
    public Vector3Int offset;
}