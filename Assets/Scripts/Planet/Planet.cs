using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

public class Planet : MonoBehaviour
{
    public static int ChunkSize = 16;
    public int planetRadius = 8;

    public List<Player> players = new List<Player>();

    public Material opaqueBlockMaterial;
    public Material transparentBlockMaterial;
    public Material vegetationBlockMaterial;
    public Material waterMaterial;

    public BlockRegistry blockManager;
    public TextureAtlasRegistry textureAtlasManager;
    public BlockGeometryRegistry blockGeometryManager;

    public Dictionary<Vector3Int, Chunk> chunks = new Dictionary<Vector3Int, Chunk>();
    public HashSet<Vector3Int> markedDirtySet = new HashSet<Vector3Int>();
    public Queue<Chunk> markedDirtyQueue = new Queue<Chunk>();

    private NoiseData[] noiseDatas;

    [Header("Noise Settings")]
    public PlanetNoiseSettings noiseSettings = new();

    private NoiseGenJobHandler noiseGenHandler;

    public bool hasGeneratedWorld = false;

    private void Awake() {
        blockManager.Init();
        textureAtlasManager.Init();
        blockGeometryManager.Init();

        opaqueBlockMaterial.mainTexture = TextureAtlasRegistry.BlockAtlas;
        transparentBlockMaterial.mainTexture = TextureAtlasRegistry.BlockAtlas;
        vegetationBlockMaterial.mainTexture = TextureAtlasRegistry.BlockAtlas;
        waterMaterial.mainTexture = TextureAtlasRegistry.BlockAtlas;
    }
    private void Start() {
        StartCoroutine(GenerateNoiseData());
    }
    private void GenerateWorld() {
        for (int x = 0; x < planetRadius; x++) {
            for (int y = 0; y <= 16; y++) {
                for (int z = 0; z < planetRadius; z++) {
                    Chunk chunk = new Chunk(new Vector3Int(x, y, z), this);
                    chunks.Add(chunk.coord, chunk);
                }
            }
        }

        SpawnPlayers();

        hasGeneratedWorld = true;
    }
    private int GetTerrainSurface(Vector3 position) {
        Vector3Int voxelCoord = Vector3Int.FloorToInt(position);
        int index = GetVoxelIndex(voxelCoord);
        return Mathf.RoundToInt(Mathf.Lerp(40f, 120f, noiseDatas[index].height));
    }
    private void SpawnPlayers(float radius = 10f) {
        int playerCount = players.Count;

        Vector3 planetCenter = new Vector3(planetRadius * ChunkSize / 2f, 0f, planetRadius * ChunkSize / 2f);

        for (int i = 0; i < playerCount; i++) {
            float angleDeg = 360f * i / playerCount;
            float angleRad = math.radians(angleDeg);

            Vector3 spawnPosition = new Vector3(
                Mathf.Cos(angleRad) * radius,
                0f,                     
                Mathf.Sin(angleRad) * radius
            );

            int surface = GetTerrainSurface(spawnPosition);
            spawnPosition.y = surface + 1f;

            Player player = players[i];
            player.Teleport(spawnPosition + planetCenter);
        }
    }

    private IEnumerator GenerateNoiseData() {
        noiseGenHandler = new NoiseGenJobHandler(planetRadius * ChunkSize, noiseSettings);
        noiseGenHandler.StartJob();

        while (!noiseGenHandler.IsJobComplete()) {
            yield return null;
        }

        NativeArray<NoiseData> resultArray = noiseGenHandler.CompleteJob();
        noiseDatas = resultArray.ToArray();

        noiseGenHandler.Dispose();
        noiseGenHandler = null;

        GenerateWorld();
    }
    public uint GetVoxel(Vector3Int globalVoxelCoord) {
        Vector3Int chunkCoord = GlobalToChunkCoord(globalVoxelCoord);

        if (chunks.TryGetValue(chunkCoord, out Chunk chunk)) {
            // Local position within the chunk
            Vector3Int localPos = new Vector3Int(
                ToLocalIndex(globalVoxelCoord.x),
                ToLocalIndex(globalVoxelCoord.y),
                ToLocalIndex(globalVoxelCoord.z)
            );

            return chunk.GetBlockIndex(localPos);
        }

        if (globalVoxelCoord.y < 0 || globalVoxelCoord.y > 255)
            return BlockTypes.Air.registryIndex;

        int noiseSize = planetRadius * ChunkSize;

        if (globalVoxelCoord.x < 0 || globalVoxelCoord.z < 0 ||
            globalVoxelCoord.x >= noiseSize || globalVoxelCoord.z >= noiseSize)
            return BlockTypes.Stone.registryIndex;


        int index = globalVoxelCoord.z * noiseSize + globalVoxelCoord.x;
        NoiseData n = noiseDatas[index];

        int terrainHeight = Mathf.RoundToInt(Mathf.Lerp(40f, 120f, n.height)); 

        int waterLevel = 64;

        // --- Terrain composition ---
        if (globalVoxelCoord.y < terrainHeight - 4)
            return BlockTypes.Stone.registryIndex;

        if (globalVoxelCoord.y < terrainHeight)
            return BlockTypes.Dirt.registryIndex;

        if (globalVoxelCoord.y == terrainHeight)
            return terrainHeight < waterLevel
                ? BlockTypes.Sand.registryIndex
                : BlockTypes.GrassBlock.registryIndex;

        if (globalVoxelCoord.y <= waterLevel && globalVoxelCoord.y > terrainHeight)
            return BlockTypes.Water.registryIndex;

        return BlockTypes.Air.registryIndex;
    }


    private void Update() {
        foreach(var player in players) {
            Vector3 playerPos = player.transform.position;
            player.transform.position = playerPos;
        }

        int builtCount = 0;
        const int maxBuildsPerFrame = 100;

        while (markedDirtyQueue.Count > 0 && builtCount < maxBuildsPerFrame) {
            var chunk = markedDirtyQueue.Dequeue();

            chunk.RebuildChunk();
            markedDirtySet.Remove(chunk.coord);
            builtCount++;
            
        }
    }
    public Block GetBlock(Vector3Int globalVoxelCoord) {
        return new Block(this, globalVoxelCoord);
    }
    public BlockType GetBlockType(Vector3Int globalVoxelCoord) {
        return BlockRegistry.indexToBlock[GetVoxel(globalVoxelCoord)];
    }
    public void SetBlockType(Vector3Int globalVoxelCoord, uint voxelType) {
        Vector3Int chunkCoord = GlobalToChunkCoord(globalVoxelCoord);
        if (chunks.TryGetValue(chunkCoord, out Chunk chunk)) {
            Vector3Int localPos = new Vector3Int(
                ToLocalIndex(globalVoxelCoord.x),
                ToLocalIndex(globalVoxelCoord.y),
                ToLocalIndex(globalVoxelCoord.z)
            );

            chunk.SetBlockType(localPos, voxelType);

            if (markedDirtySet.Add(chunkCoord)) {
                markedDirtyQueue.Enqueue(chunk);
            }

            //MarkNeighborsDirty(chunkCoord);
        }
    }
    private static int ToLocalIndex(int globalCoord) {
        int mod = globalCoord % ChunkSize;
        if (mod < 0) mod += ChunkSize;
        return mod;
    }
    private Vector3Int GlobalToChunkCoord(Vector3Int globalPos) {
        return new Vector3Int(
            Mathf.FloorToInt((float)globalPos.x / ChunkSize),
            Mathf.FloorToInt((float)globalPos.y / ChunkSize),
            Mathf.FloorToInt((float)globalPos.z / ChunkSize)
        );
    }
    private static readonly Vector3Int[] NeighborOffsets = {
        new Vector3Int( 1, 0, 0),
        new Vector3Int(-1, 0, 0),
        new Vector3Int( 0, 0, 1),
        new Vector3Int( 0, 0,-1),
        new Vector3Int( 0, 1, 0),
        new Vector3Int( 0, -1, 0),
    };
    public int GetVoxelIndex(Vector3Int voxelCoord) {
        WrapCoord(ref voxelCoord);
        int noiseSize = planetRadius * ChunkSize;
        return voxelCoord.z * noiseSize + voxelCoord.x;
    }
    public Chunk GetChunk(Vector3Int coord) {
        Vector3Int chunkCoord = new();
        return chunks[chunkCoord];
    }
    private void MarkNeighborsDirty(Vector3Int coord) {
        HashSet<Vector3Int> coords = new HashSet<Vector3Int>();
        foreach (var neighbor in NeighborOffsets) {
            Vector3Int c = neighbor + coord;
            if (markedDirtySet.Add(c)) {
                markedDirtyQueue.Enqueue(GetChunk(c));
            }
        }

    }
    public void WrapCoord(ref Vector3Int pos) {
        int worldSize = planetRadius * ChunkSize;

        pos.x = ((pos.x % worldSize) + worldSize) % worldSize;
        pos.z = ((pos.z % worldSize) + worldSize) % worldSize;
    }
    public void WrapCoord(ref Vector3 pos) {
        float worldSize = planetRadius * ChunkSize;

        pos.x = Mathf.Repeat(pos.x, worldSize);
        pos.z = Mathf.Repeat(pos.z, worldSize);
    }
    public void WrapChunkCoord(ref Vector3Int chunkPos) {
        int worldChunkSize = planetRadius;

        chunkPos.x = ((chunkPos.x % worldChunkSize) + worldChunkSize) % worldChunkSize;
        chunkPos.z = ((chunkPos.z % worldChunkSize) + worldChunkSize) % worldChunkSize;
    }
    public void WrapChunkCoord(ref Vector3 chunkPos) {
        float worldChunkSize = planetRadius;

        chunkPos.x = Mathf.Repeat(chunkPos.x, worldChunkSize);
        chunkPos.z = Mathf.Repeat(chunkPos.z, worldChunkSize);
    }
    public Vector3Int GetWrappedChunkCoord(Vector3 worldPos) {
        int worldSize = planetRadius * ChunkSize;
        int chunkSize = ChunkSize;

        float wrappedX = Mathf.Repeat(worldPos.x, worldSize);
        float wrappedZ = Mathf.Repeat(worldPos.z, worldSize);

        int chunkX = Mathf.FloorToInt(wrappedX / chunkSize);
        int chunkZ = Mathf.FloorToInt(wrappedZ / chunkSize);
        int chunkY = Mathf.FloorToInt(worldPos.y / chunkSize);

        return new Vector3Int(chunkX, chunkY, chunkZ);
    }
    private void OnDestroy() {
        foreach (var chunk in chunks.Values) {
            chunk.OnDestroy();
        }
        chunks.Clear();

        if (noiseGenHandler != null) {
            noiseGenHandler.OnDestroy();
            noiseGenHandler = null;
        }
    }
}
