using UnityEngine;

public class Player : Entity
{
    public Planet planet;
    public Camera playerCam;

    public bool RaycastBlock(Vector3 position, Vector3 direction, float distance, out Block block, out Vector3Int normal) {
        if (!planet.hasGeneratedWorld) {
            block = null;
            normal = Vector3Int.zero;
            return false;
        }

        block = null;
        normal = Vector3Int.zero;

        direction.Normalize();

        Vector3 currentPos = position;
        float step = 0.01f;
        float dst = 0f;

        Vector3Int lastVoxel = Vector3Int.FloorToInt(currentPos);

        while (dst < distance) {
            dst += step;
            currentPos += direction * step;

            Vector3Int voxel = Vector3Int.FloorToInt(currentPos);
            if (voxel != lastVoxel) {
                Block testBlock = planet.GetBlock(voxel);
                if (testBlock != null && testBlock.typeId != "air") {
                    block = testBlock;

                    Vector3Int delta = voxel - lastVoxel;
                    normal = -delta;
                    normal.Clamp(Vector3Int.one * -1, Vector3Int.one);

                    return true;
                }

                lastVoxel = voxel;
            }
        }

        return false;
    }
    public void Teleport(Vector3 position) {
        transform.position = position;
    }
}
