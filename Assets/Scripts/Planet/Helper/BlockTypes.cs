public struct BlockTypeEnum {
    public uint registryIndex;
    public string typeId;
    public BlockType type;
}

public static class BlockTypes {
    public static BlockTypeEnum Air => new BlockTypeEnum() { typeId = "air", registryIndex = 0, type = BlockRegistry.indexToBlock[0] };
    public static BlockTypeEnum ArrowBlock => new BlockTypeEnum() { typeId = "arrow_block", registryIndex = 1, type = BlockRegistry.indexToBlock[1] };
    public static BlockTypeEnum Chest => new BlockTypeEnum() { typeId = "chest", registryIndex = 2, type = BlockRegistry.indexToBlock[2] };
    public static BlockTypeEnum Dirt => new BlockTypeEnum() { typeId = "dirt", registryIndex = 3, type = BlockRegistry.indexToBlock[3] };
    public static BlockTypeEnum Glass => new BlockTypeEnum() { typeId = "glass", registryIndex = 4, type = BlockRegistry.indexToBlock[4] };
    public static BlockTypeEnum Grass => new BlockTypeEnum() { typeId = "grass", registryIndex = 5, type = BlockRegistry.indexToBlock[5] };
    public static BlockTypeEnum GrassBlock => new BlockTypeEnum() { typeId = "grass_block", registryIndex = 6, type = BlockRegistry.indexToBlock[6] };
    public static BlockTypeEnum Gravel => new BlockTypeEnum() { typeId = "gravel", registryIndex = 7, type = BlockRegistry.indexToBlock[7] };
    public static BlockTypeEnum OakLeaves => new BlockTypeEnum() { typeId = "oak_leaves", registryIndex = 8, type = BlockRegistry.indexToBlock[8] };
    public static BlockTypeEnum OakLog => new BlockTypeEnum() { typeId = "oak_log", registryIndex = 9, type = BlockRegistry.indexToBlock[9] };
    public static BlockTypeEnum Sand => new BlockTypeEnum() { typeId = "sand", registryIndex = 10, type = BlockRegistry.indexToBlock[10] };
    public static BlockTypeEnum Stone => new BlockTypeEnum() { typeId = "stone", registryIndex = 11, type = BlockRegistry.indexToBlock[11] };
    public static BlockTypeEnum StoneSlab => new BlockTypeEnum() { typeId = "stone_slab", registryIndex = 12, type = BlockRegistry.indexToBlock[12] };
    public static BlockTypeEnum StoneStair => new BlockTypeEnum() { typeId = "stone_stair", registryIndex = 13, type = BlockRegistry.indexToBlock[13] };
    public static BlockTypeEnum Water => new BlockTypeEnum() { typeId = "water", registryIndex = 14, type = BlockRegistry.indexToBlock[14] };
}