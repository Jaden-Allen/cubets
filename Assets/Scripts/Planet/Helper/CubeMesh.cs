using UnityEngine;
public static class CubeMesh {
    public static readonly Vector3[] Vertices = new Vector3[8] {
        new Vector3(0f, 0f, 0f),
        new Vector3(0f, 0f, 1f),
        new Vector3(1f, 0f, 0f),
        new Vector3(1f, 0f, 1f),
        new Vector3(0f, 1f, 0f),
        new Vector3(0f, 1f, 1f),
        new Vector3(1f, 1f, 0f),
        new Vector3(1f, 1f, 1f)
    };
    public static readonly int[,] Indices = new int[6, 4] {
        {0, 4, 2, 6}, // South
        {3, 7, 1, 5}, // North
        {2, 6, 3, 7}, // East
        {1, 5, 0, 4}, // West
        {4, 5, 6, 7}, // Top
        {1, 0, 3, 2}, // Bottom
    };
    public static readonly Vector2[] Uvs = new Vector2[4] {
        new Vector2(0f, 0f),
        new Vector2(0f, 1f),
        new Vector2(1f, 0f),
        new Vector2(1f, 1f),
    };
    public static readonly Vector3Int[] Normals = new Vector3Int[6] {
        new Vector3Int(0, 0, -1), // South
        new Vector3Int(0, 0, 1),  // North
        new Vector3Int(1, 0, 0),  // East
        new Vector3Int(-1, 0, 0), // West
        new Vector3Int(0, 1, 0),  // Top
        new Vector3Int(0, -1, 0)  // Bottom
    };
}