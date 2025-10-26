using UnityEngine;

public class PlayerVoxelInteractionController : EntityComponent
{
    public Planet planet;
    public Transform playerCam;
    public float range = 6f;

    public override void OnEntityUpdate(bool ignoreLoops) {
        if (Input.GetMouseButtonDown(0)) {
            DestroyVoxel();
        }
        else if (Input.GetMouseButtonDown(1)) {
            PlaceVoxel();
        }
    }
    public void DestroyVoxel() {
        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out Vector3Int normal)) {
            Debug.Log("Found block to destroy");
            block.SetType(BlockTypes.Air.type);
        }
    }
    public void PlaceVoxel() {
        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out Vector3Int normal)) {
            block.Offset(normal).SetType(BlockTypes.Stone.type);
        }
    }
    private void OnDrawGizmos() {
        return;
        if (!Application.isPlaying) return;

        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out Vector3Int normal)) {
            Gizmos.color = Color.red;
            Gizmos.DrawCube(block.position + Vector3.one * 0.5f, Vector3.one * 1.001f);
            Gizmos.color = Color.green;
            Gizmos.DrawCube(block.position + Vector3.one * 0.5f + normal, Vector3.one);
        }
    }
}
