using BlockGeometry;
using System;
using System.Collections.Generic;
using UnityEngine;

public class Chunk {
    public Vector3Int coord;
    public Vector3Int origin;
    public Planet planet;

    public GameObject chunkObject;
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;

    private int vertexIndex = 0;
    private List<Vector3> vertices = new();
    private List<Vector3> normals = new();
    private List<Vector2> uvs = new();
    private List<Color> colors = new();

    private List<int> opaqueIndices = new();
    private List<int> transparentIndices = new();
    private List<int> vegetationIndices = new();
    private List<int> waterIndices = new();

    private Bounds bounds;

    private uint[,,] voxelMap = new uint[Planet.ChunkSize, Planet.ChunkSize, Planet.ChunkSize];

    private Mesh mesh;

    public bool isDirty = true;

    public Chunk(Vector3Int coord, Planet planet) {
        this.coord = coord;
        this.planet = planet;

        origin = coord * Planet.ChunkSize;

        chunkObject = new GameObject($"Chunk {coord}");
        chunkObject.layer = LayerMask.NameToLayer("Terrain");
        chunkObject.transform.SetParent(planet.transform);
        chunkObject.transform.position = origin;

        meshFilter = chunkObject.AddComponent<MeshFilter>();
        meshRenderer = chunkObject.AddComponent<MeshRenderer>();

        meshRenderer.materials = new Material[] { planet.opaqueBlockMaterial, planet.transparentBlockMaterial, planet.vegetationBlockMaterial, planet.waterMaterial };

        bounds = new Bounds(Vector3.one * (Planet.ChunkSize / 2f), Vector3.one * Planet.ChunkSize);

        mesh = new Mesh();
        mesh.MarkDynamic();
        meshFilter.mesh = mesh;

        PopulateVoxelMap();
        PopulateMeshData();
        ApplyMeshData();
    }

    private void PopulateVoxelMap() {
        for (int x = 0; x < Planet.ChunkSize; x++) {
            for (int y = 0; y < Planet.ChunkSize; y++) {
                for ( int z = 0; z < Planet.ChunkSize; z++) {
                    voxelMap[x,y,z] = planet.GetVoxel(origin + new Vector3Int(x,y,z));
                }
            }
        }
    }
    public void PopulateMeshData() {
        for (int x = 0; x < Planet.ChunkSize; x++) {
            for (int y = 0; y < Planet.ChunkSize; y++) {
                for (int z = 0; z < Planet.ChunkSize; z++) {
                    if (voxelMap[x, y, z] == 0) continue;
                    AddVoxelDataToChunk(new Vector3Int(x, y, z));
                }
            }
        }
    }
    public void AddVoxelDataToChunk(Vector3Int localPos) {
        uint blockId = voxelMap[localPos.x, localPos.y, localPos.z];
        BlockType blockType = BlockRegistry.indexToBlock[blockId];
        if (blockType) {
            if (blockType.MeshDataOverride(new Block(planet, localPos + origin), out MeshData meshData)) {
                AddMeshDataOverride(meshData, localPos, blockType);
            }
            else {
                BlockGeometryAsset blockGeometry = blockType.geometry;
                if (blockGeometry == null) {
                    AddCubeDataToChunk(localPos, blockType);
                }
                else {
                    AddCustomGeoDataToChunk(localPos, blockType);
                }
            }
        }
        else {
            throw new Exception($"Block not found: {blockType}");
        }
    }
    private void AddMeshDataOverride(MeshData meshData, Vector3Int localPos, BlockType block) {
        var indicesToAddTo = block.materialInstances.renderType switch {
            BlockRenderType.Opaque => opaqueIndices,
            BlockRenderType.Transparent => transparentIndices,
            BlockRenderType.Vegetation => vegetationIndices,
            BlockRenderType.Water => waterIndices,
            _ => opaqueIndices
        };
    }

    private void AddCustomGeoDataToChunk(Vector3Int localPos, BlockType block) {
        if (BlockGeometryRegistry.GetBlockGeometry(block.geometry.id, out BlockGeometryFile geometry)) {
            geometry.vertices.ForEach(v => { vertices.Add(v + localPos); colors.Add(block.VertexColorPaint(v, localPos + origin, planet)); });

            var indicesToAddTo = block.materialInstances.renderType switch {
                BlockRenderType.Opaque => opaqueIndices,
                BlockRenderType.Transparent => transparentIndices,
                BlockRenderType.Vegetation => vegetationIndices,
                BlockRenderType.Water => waterIndices,
                _ => opaqueIndices
            };
            geometry.indices.ForEach(i => indicesToAddTo.Add(i + vertexIndex));
            geometry.normals.ForEach(n => normals.Add(n));

            foreach (var uvItem in geometry.uvs) {
                var uv = uvItem.Item1;
                var textureKey = uvItem.Item2;
                Rect textureRect = block.materialInstances.GetRect(textureKey);
                uvs.Add(uv * textureRect.size + textureRect.min);
            }
            vertexIndex += geometry.vertices.Count;
        }
        else {
            throw new Exception($"Block geometry not found: {block.geometry.id}");
        }
    }
    private void AddCubeDataToChunk(Vector3Int localPos, BlockType block) {
        for (int p = 0; p < 6; p++) {
            BlockType neighbor = GetNeighbor(localPos, CubeMesh.Normals[p]);
            if (neighbor.materialInstances.renderType == BlockRenderType.Opaque) continue;
            if (block.materialInstances.renderType != BlockRenderType.Opaque && neighbor.id == block.id) continue;

            for (int i = 0; i < 4; i++) {
                vertices.Add(CubeMesh.Vertices[CubeMesh.Indices[p, i]] + localPos);
                normals.Add(CubeMesh.Normals[p]);

                Rect uvRect = block.materialInstances.GetRect(GeometryBuilder.GetKey(p));
                uvs.Add(CubeMesh.Uvs[i] * uvRect.size + uvRect.min);
                colors.Add(block.VertexColorPaint(CubeMesh.Vertices[CubeMesh.Indices[p, i]], localPos + origin, planet));
            }

            int[] order = new int[6] {
                0, 1, 2, 2, 1, 3
            };

            var indicesToAddTo = block.materialInstances.renderType switch {
                BlockRenderType.Opaque => opaqueIndices,
                BlockRenderType.Transparent => transparentIndices,
                BlockRenderType.Vegetation => vegetationIndices,
                BlockRenderType.Water => waterIndices,
                _ => opaqueIndices
            };

            for (int i = 0; i < order.Length; i++) {
                var t = order[i];
                indicesToAddTo.Add(vertexIndex + t);
            }

            vertexIndex += 4;
        }
    }
    private BlockType GetNeighbor(Vector3Int localVoxelCoord, Vector3Int offset) {
        Vector3Int voxel = localVoxelCoord + offset;
        if (IsVoxelCoordInChunkBounds(voxel)) {
            return BlockRegistry.indexToBlock[voxelMap[voxel.x, voxel.y, voxel.z]];
        }
        else {
            return planet.GetBlockType(voxel + origin);
        }
    }
    private uint GetVoxel(Vector3Int localVoxelCoord) {
        if (IsVoxelCoordInChunkBounds(localVoxelCoord)) {
            return voxelMap[localVoxelCoord.x, localVoxelCoord.y, localVoxelCoord.z];
        }
        return planet.GetVoxel(localVoxelCoord + origin);
    }
    private bool IsVoxelCoordInChunkBounds(Vector3Int voxelCoord) {
        return voxelCoord.x >= 0 && voxelCoord.y >= 0 && voxelCoord.z >= 0 && voxelCoord.x < Planet.ChunkSize && voxelCoord.y < Planet.ChunkSize && voxelCoord.z < Planet.ChunkSize;
    }
    private void ApplyMeshData() {
        mesh.Clear();
        mesh.subMeshCount = 4;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(opaqueIndices, 0, true, 0);
        mesh.SetTriangles(transparentIndices, 1, true, 0);
        mesh.SetTriangles(vegetationIndices, 2, true, 0);
        mesh.SetTriangles(waterIndices, 3, true, 0);
        mesh.SetUVs(0, uvs);
        mesh.SetNormals(normals);
        mesh.SetColors(colors);
    }
    public void RebuildChunk() {
        vertexIndex = 0;
        vertices.Clear();
        opaqueIndices.Clear();
        transparentIndices.Clear();
        vegetationIndices.Clear();
        waterIndices.Clear();
        normals.Clear();
        uvs.Clear();
        colors.Clear();
        mesh.Clear();

        PopulateMeshData();
        ApplyMeshData();

        isDirty = false;
    }
    public void SetBlockType(Vector3Int localPos, uint voxelType) {
        voxelMap[localPos.x, localPos.y, localPos.z] = voxelType;
        isDirty = true;
    }
    public uint GetBlockIndex(Vector3Int localPos) {
        return voxelMap[localPos.x, localPos.y, localPos.z];
    }
    public void OnDestroy() {
        GameObject.Destroy(chunkObject);
        GameObject.Destroy(mesh);
        vertices = null;
        opaqueIndices = null;
        transparentIndices = null;
        vegetationIndices = null;
        waterIndices = null;   
        normals = null;
        uvs = null;
        colors = null;
    }
}
public struct MeshData {
    public List<Vector3> vertices;
    public List<Vector3> normals;
    public List<int> indices;
    public List<Vector2> uvs;
}