using UnityEngine;

public class BoxVolumeCollider : MonoBehaviour {
    public float width = 1f;
    public float height = 1.8f;
    public Vector3 offset;

    public bool CheckCollisions(Planet planet, Vector3 sampleOffset, out Vector3 clipping) {
        clipping = Vector3.zero;
        return true;

        float halfWidth = width * 0.5f;

        bool foundCollision = false;
        Vector3 correction = Vector3.zero;
        int samples = 0;

        // Apply the test offset to the collider position
        Vector3 newPos = transform.position + sampleOffset;

        Vector3 min = newPos + offset + new Vector3(-halfWidth, 0f, -halfWidth);
        Vector3 max = newPos + offset + new Vector3(halfWidth, height, halfWidth);

        for (int x = Mathf.FloorToInt(min.x); x <= Mathf.FloorToInt(max.x); x++) {
            for (int y = Mathf.FloorToInt(min.y); y <= Mathf.FloorToInt(max.y); y++) {
                for (int z = Mathf.FloorToInt(min.z); z <= Mathf.FloorToInt(max.z); z++) {
                    uint voxelIndex = planet.GetVoxel(new Vector3Int(x, y, z));

                    if (voxelIndex == 0 || voxelIndex == BlockTypes.Water.registryIndex)
                        continue;

                    Vector3 voxelMin = new Vector3(x, y, z);
                    Vector3 voxelMax = voxelMin + Vector3.one;

                    float overlapX = Mathf.Min(max.x, voxelMax.x) - Mathf.Max(min.x, voxelMin.x);
                    float overlapY = Mathf.Min(max.y, voxelMax.y) - Mathf.Max(min.y, voxelMin.y);
                    float overlapZ = Mathf.Min(max.z, voxelMax.z) - Mathf.Max(min.z, voxelMin.z);

                    if (overlapX > 0 && overlapY > 0 && overlapZ > 0) {
                        foundCollision = true;
                        samples++;

                        // find smallest overlap axis
                        float minOverlap = overlapX;
                        Vector3 pushDir = Vector3.right * Mathf.Sign((min.x + max.x) * 0.5f - (x + 0.5f)) * -1f;

                        if (overlapY < minOverlap) {
                            minOverlap = overlapY;
                            pushDir = Vector3.up * Mathf.Sign((min.y + max.y) * 0.5f - (y + 0.5f)) * -1f;
                        }
                        if (overlapZ < minOverlap) {
                            minOverlap = overlapZ;
                            pushDir = Vector3.forward * Mathf.Sign((min.z + max.z) * 0.5f - (z + 0.5f)) * -1f;
                        }

                        correction += pushDir * minOverlap;
                    }
                }
            }
        }

        clipping = samples > 0 ? correction / samples : Vector3.zero;
        return foundCollision;
    }
    public bool IsGrounded(Planet planet, float checkDistance = 0.1f) {
        return true;

        float halfWidth = width * 0.5f;

        // Bottom face world position range
        Vector3 basePos = transform.position + offset;
        float bottomY = basePos.y;

        // Four corners at the bottom of the box
        Vector3[] corners = new Vector3[]
        {
        new Vector3(basePos.x - halfWidth, bottomY, basePos.z - halfWidth),
        new Vector3(basePos.x + halfWidth, bottomY, basePos.z - halfWidth),
        new Vector3(basePos.x - halfWidth, bottomY, basePos.z + halfWidth),
        new Vector3(basePos.x + halfWidth, bottomY, basePos.z + halfWidth)
        };

        // Check each corner downward
        foreach (var corner in corners) {
            // Step down from the corner up to checkDistance
            Vector3Int voxelPos = Vector3Int.FloorToInt(corner + Vector3.down * checkDistance);
            
            uint voxel = planet.GetVoxel(voxelPos);

            // Anything solid counts as ground (skip air and water)
            if (voxel != 0 && voxel != BlockTypes.Water.registryIndex)
                return true;
        }

        return false;
    }

    private void OnDrawGizmos() {
        Gizmos.color = new Color(0.1f, 1f, 0.1f);
        Gizmos.DrawWireCube(transform.position + offset, new Vector3(width, height, width));
    }
}
