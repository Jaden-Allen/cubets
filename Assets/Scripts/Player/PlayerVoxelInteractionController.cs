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
            block.SetType(BlockTypes.Air.type);
        }
    }
    public void PlaceVoxel() {
        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out Vector3Int normal)) {
            block.Offset(normal).SetType(BlockTypes.Stone.type);
        }
    }
    
}
