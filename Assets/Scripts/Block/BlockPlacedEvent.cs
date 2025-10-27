using UnityEngine;

public struct BlockPlacedEvent
{
    public Block block;
}
public struct BlockDestroyedEvent {
    public Block blockBeforeDestroyed;
    public Block block;
}
public struct BlockPlacedAdjacentEvent {
    public Block other;
    public Block block;
}
public struct BlockTickEvent {
    public Block block;
}
public struct BlockRandomTickEvent {
    public Block block;
}
public class Block {
    public readonly Planet planet;
    public readonly Vector3Int position;

    public BlockType blockData => planet.GetBlockType(position);
    public string typeId => blockData.id;

    public bool isAir => blockData.id == "air";
    public Block(Planet planet, Vector3Int position) {
        this.planet = planet;
        this.position = position;
    }

    public Block Above(int steps = 1) => planet.GetBlock(position + Vector3Int.up * steps);
    public Block Below(int steps = 1) => planet.GetBlock(position + Vector3Int.down * steps);
    public Block North(int steps = 1) => planet.GetBlock(position + Vector3Int.forward * steps);
    public Block South(int steps = 1) => planet.GetBlock(position + Vector3Int.back * steps);
    public Block East(int steps = 1) => planet.GetBlock(position + Vector3Int.right * steps);
    public Block West(int steps = 1) => planet.GetBlock(position + Vector3Int.left * steps);
    public Block Offset(Vector3Int offset) => planet.GetBlock(position + offset);

    public Vector3 Center() => new Vector3(position.x + 0.5f, position.y + 0.5f, position.z + 0.5f);

    public void SetType(BlockType type, byte rotation) => planet.SetBlockType(position, BlockRegistry.typeIdToIndex[type.id], rotation);
    //public void SetPermutation(BlockPermutation permutation) => world.SetPermutation(position, permutation);
}
