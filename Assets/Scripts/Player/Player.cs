using UnityEngine;

public class Player : Entity
{
    public Planet planet;
    public Camera playerCam;

    public bool RaycastBlock(Vector3 position, Vector3 direction, float distance, out Block block, out Vector3Int normal) {
        block = null;
        normal = Vector3Int.zero;

        // Normalize direction so steps are consistent
        direction.Normalize();

        // Start position
        Vector3 currentPos = position;
        float step = 0.01f;
        float dst = 0f;

        // Last voxel we were in (non-hit)
        Vector3Int lastVoxel = Vector3Int.FloorToInt(currentPos);

        while (dst < distance) {
            dst += step;
            currentPos += direction * step;

            Vector3Int voxel = Vector3Int.FloorToInt(currentPos);
            if (voxel != lastVoxel) {
                // We crossed a voxel boundary
                Block testBlock = planet.GetBlock(voxel); // replace with your block getter
                if (testBlock != null && testBlock.typeId != "air") {
                    // Hit!
                    block = testBlock;

                    // Determine which axis changed from last voxel to current voxel
                    Vector3Int delta = voxel - lastVoxel;
                    normal = -delta; // negative because ray entered the block from that side
                    normal.Clamp(Vector3Int.one * -1, Vector3Int.one); // make sure it’s one of 6 cardinal dirs

                    return true;
                }

                lastVoxel = voxel;
            }
        }

        return false;
    }
}
