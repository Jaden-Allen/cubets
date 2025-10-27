using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UIElements;

public class EntityCollider : MonoBehaviour {
    public float width = 1f;
    public float height = 1.8f;
    public Vector3 offset;
    private Vector3 debugOffset => new Vector3(0f, height / 2f, 0f);

    public Vector3 size => new Vector3(width, height, width);
    public Vector3 center => size / 2f + offset;
    public Vector3 min => new Vector3(-(width / 2f), 0f, -(width / 2f)) + offset;
    public Vector3 max => min + size;

    public HashSet<Vector3Int> OccupiedBlocks(Planet planet) {
        HashSet<Vector3Int> occupied = new HashSet<Vector3Int>();
        if (!planet.hasGeneratedWorld) return occupied;
        
        for (int x = Mathf.FloorToInt(min.x); x <= Mathf.FloorToInt(max.x); x++)
            for (int y = Mathf.FloorToInt(min.y); y <= Mathf.FloorToInt(max.y); y++)
                for (int z = Mathf.FloorToInt(min.z); z <= Mathf.FloorToInt(max.z); z++)
                    occupied.Add(new Vector3Int(x, y, z));

        return occupied;
    }
    public bool IsColliding(CubeCollider cubeCollider, Vector3 position, Vector3 sampleOffset) {
        Vector3 _min = min + transform.position + sampleOffset;
        Vector3 _max = max + transform.position + sampleOffset;

        Vector3 cubeMin = cubeCollider.min + position;
        Vector3 cubeMax = cubeCollider.max + position;

        bool overlapX = _min.x <= cubeMax.x && _max.x >= cubeMin.x;
        bool overlapY = _min.y <= cubeMax.y && _max.y >= cubeMin.y;
        bool overlapZ = _min.z <= cubeMax.z && _max.z >= cubeMin.z;

        return overlapX && overlapY && overlapZ;
    }
    public bool CheckCollisions(Planet planet, Vector3 sampleOffset) {
        if (!planet.hasGeneratedWorld) return true;

        Vector3 newPos = transform.position + sampleOffset;

        Vector3 _min = newPos + min;
        Vector3 _max = newPos + max;

        for (int x = Mathf.FloorToInt(_min.x); x <= Mathf.FloorToInt(_max.x); x++) {
            for (int y = Mathf.FloorToInt(_min.y); y <= Mathf.FloorToInt(_max.y); y++) {
                for (int z = Mathf.FloorToInt(_min.z); z <= Mathf.FloorToInt(_max.z); z++) {
                    Vector3Int voxelPos = new Vector3Int(x, y, z);
                    Block block = planet.GetBlock(voxelPos);
                    if (block == null || block.isAir || !block.blockData.collision.enabled)
                        continue;

                    Vector3 voxelBase = voxelPos + new Vector3(0.5f, 0f, 0.5f);
                    foreach (var cube in block.blockData.collision.colliders) {
                        if (IsColliding(cube, voxelBase, sampleOffset)) return true;
                    }
                }
            }
        }

        return false;
    }
    public bool IsGrounded(Planet planet, float checkDistance = 0.1f) {
        if (!planet.hasGeneratedWorld) return true;

        Vector3 basePos = transform.position + offset;

        Vector3[] corners = new Vector3[] {
            new Vector3(min.x + basePos.x, basePos.y - checkDistance, min.z + basePos.z),
            new Vector3(max.x + basePos.x, basePos.y - checkDistance, min.z + basePos.z),
            new Vector3(min.x + basePos.x, basePos.y - checkDistance, max.z + basePos.z),
            new Vector3(max.x + basePos.x, basePos.y - checkDistance, max.z + basePos.z)
        };

        foreach (var corner in corners) {
            Vector3Int voxelPos = Vector3Int.FloorToInt(new Vector3(corner.x, corner.y, corner.z));
            Block block = planet.GetBlock(voxelPos);
            if (block == null || !block.blockData.collision.enabled) continue;

            Vector3 voxelBase = voxelPos + new Vector3(0.5f, 0f, 0.5f);
            foreach (var cube in block.blockData.collision.colliders) {
                if (IsColliding(cube, voxelBase, Vector3.down * checkDistance)) return true;
            }
        }

        return false;
    }
    public bool IsHeadHitting(Planet planet, float checkDistance = 0.1f) {
        if (!planet.hasGeneratedWorld) return true;

        Vector3 basePos = transform.position + offset;

        Vector3[] corners = new Vector3[] {
            new Vector3(min.x + basePos.x, basePos.y + height + checkDistance, min.z + basePos.z),
            new Vector3(max.x + basePos.x, basePos.y + height + checkDistance, min.z + basePos.z),
            new Vector3(min.x + basePos.x, basePos.y + height + checkDistance, max.z + basePos.z),
            new Vector3(max.x + basePos.x, basePos.y + height + checkDistance, max.z + basePos.z)
        };

        foreach (var corner in corners) {
            Vector3Int voxelPos = Vector3Int.FloorToInt(new Vector3(corner.x, corner.y, corner.z));
            Block block = planet.GetBlock(voxelPos);
            if (block == null || !block.blockData.collision.enabled) continue;

            Vector3 voxelBase = voxelPos + new Vector3(0.5f, 0f, 0.5f);
            foreach (var cube in block.blockData.collision.colliders) {
                if (IsColliding(cube, voxelBase, Vector3.up * checkDistance)) return true;
            }
        }

        return false;
    }
    public Vector3[] GetHorizontalCorners(Vector3 position) {
        Vector3 worldMin = min + position;
        Vector3 worldMax = max + position;

        return new Vector3[] {
        new Vector3(worldMin.x, position.y, worldMin.z),
        new Vector3(worldMax.x, position.y, worldMin.z),
        new Vector3(worldMin.x, position.y, worldMax.z),
        new Vector3(worldMax.x, position.y, worldMax.z)
    };
    }


    private void OnDrawGizmos() {
        Gizmos.color = new Color(0.1f, 1f, 0.1f);
        Gizmos.DrawWireCube(transform.position + offset + debugOffset, new Vector3(width, height, width));
    }
}
