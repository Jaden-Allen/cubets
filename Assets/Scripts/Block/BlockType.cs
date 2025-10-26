using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "BlockData", menuName = "Data/BlockData", order = 1)]
public class BlockType : ScriptableObject {
    public new string name;
    public string id;
    public BlockMaterialInstancesComponent materialInstances;
    public BlockGeometryAsset geometry;
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
        return d != null && TextureAtlasManager.typeIdToRect.TryGetValue(d.asset.id, out var rect)
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
public enum BlockRenderType {
    Opaque,
    Transparent,
    Vegetation,
    Water,
    Air
}