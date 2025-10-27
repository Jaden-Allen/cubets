using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "Data/BlockData", order = 1)]
public class BlockType : ScriptableObject {
    public new string name;
    public string id;
    public BlockMaterialInstancesComponent materialInstances;
    public BlockGeometryAsset geometry;
    public BlockCollisionComponent collision = new();
    public BlockSelectionComponent selection = new();
    public BlockRotationPlacement placementType = BlockRotationPlacement.None;
    public AudioClip placeAudioClip;
    public AudioClip destroyAudioClip;
    public virtual void Setup() {}
    public virtual void OnPlaced(BlockPlacedEvent e) {}
    public virtual void OnDestroyed(BlockDestroyedEvent e) {}
    public virtual void OnBlockPlacedAdjacent(BlockPlacedAdjacentEvent e) {}
    public virtual BlockTickComponent tickComponent { get; } = null;
    public virtual BlockRandomTickComponent randomTickComponent { get; } = null;
    public virtual Color VertexColorPaint(Vector3 vertex, Vector3Int globalPosition, Planet planet) {
        return Color.black;
    }
    public virtual bool MeshDataOverride(Block block, out MeshData meshData) {
        meshData = new MeshData();
        return false;
    }
    public virtual BlockPermutation ResolvePermutation() {
        return null;
    }
    public bool isAir => id == BlockTypes.Air.typeId;
    
}
[System.Serializable]
public class BlockMaterialInstancesComponent {
    public BlockRenderType renderType;
    public List<BlockMaterialPropertyDefinition> definitions = new List<BlockMaterialPropertyDefinition>();
    private Dictionary<string, BlockMaterialPropertyDefinition> def;
    public Rect GetRect(string faceProperty) {
        // Lazy init dictionary
        if (def == null)
            def = definitions.ToDictionary(d => d.id, d => d);

        // Try to get exact property, else fallback to "*"
        if (!def.TryGetValue(faceProperty, out var d) && faceProperty != "*")
            def.TryGetValue("*", out d);

        // Return rect if found, else empty rect
        return d != null && TextureAtlasRegistry.typeIdToRect.TryGetValue(d.asset.id, out var rect)
            ? rect
            : new Rect();
    }

}
[System.Serializable]
public abstract class BlockTickComponent {
    public abstract int tickInterval { get; }
    public virtual void OnTick(BlockTickEvent e) { }
}
[System.Serializable]
public abstract class BlockRandomTickComponent {
    public virtual void OnTick(BlockTickEvent e) { }
}
[System.Serializable]
public class BlockMaterialPropertyDefinition {
    public string id = "*";
    public BlockMaterialPropertyAsset asset;
}
[System.Serializable]
public class BlockCollisionComponent {
    public bool enabled = true;
    public List<CubeCollider> colliders = new List<CubeCollider>();
}
[System.Serializable]
public class BlockSelectionComponent {
    public List<CubeCollider> colliders = new List<CubeCollider>();
}
public enum BlockRenderType {
    Opaque,
    Transparent,
    Vegetation,
    Water,
    Air
}
public enum BlockRotationPlacement {
    None,       // Use the model’s default rotation
    Cardinal,   // Place facing N/S/E/W based on player orientation
    Pillar,     // Rotate the “top” of the model to align with the surface normal (like logs)
    Full        // Like Cardinal + upside-down option if placing on the top half of a block (stairs, slabs)
}
public enum Direction : byte {
    North = 0, East = 1, South = 2, West = 3, Up = 4, Down = 5
}
