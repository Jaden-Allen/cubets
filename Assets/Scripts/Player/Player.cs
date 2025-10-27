using UnityEngine;

public class Player : Entity
{
    public Planet planet;
    public Camera playerCam;
    public EntityCollider playerCollider;

    public bool RaycastBlock(Vector3 position, Vector3 direction, float distance, out Block block, out Vector3Int normal) {
        block = null;
        normal = Vector3Int.zero;

        if (!planet.hasGeneratedWorld)
            return false;

        direction.Normalize();

        Vector3 currentPos = position;
        float step = 0.01f;
        float dst = 0f;

        Vector3Int lastVoxel = Vector3Int.FloorToInt(currentPos);

        while (dst < distance) {
            dst += step;
            currentPos += direction * step;

            Vector3Int voxel = Vector3Int.FloorToInt(currentPos);

            Block testBlock = planet.GetBlock(voxel);
            if (testBlock != null && !testBlock.isAir && testBlock.typeId != "water") {
                foreach (var c in testBlock.blockData.selection.colliders) {
                    if (c.Overlaps(currentPos, voxel + new Vector3(0.5f, 0f, 0.5f), direction, out normal)) {
                        block = testBlock;

                        return true;
                    }
                }
            }

            if (lastVoxel != voxel)
                lastVoxel = voxel;
        }

        return false;
    }

    public void Teleport(Vector3 position) {
        transform.position = position;
    }
}
