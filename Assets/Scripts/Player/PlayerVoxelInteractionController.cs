using System.Collections.Generic;
using UnityEngine;

public class PlayerVoxelInteractionController : EntityComponent
{
    public Planet planet;
    public Transform playerCam;
    public EntityCollider playerCollider;
    public float range = 6f;

    public override void OnEntityUpdate(bool ignoreLoops) {
        if (ignoreLoops) return;
        if (!planet.hasGeneratedWorld) return;

        RenderHighlights();

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
            Block offset = block.Offset(normal);

            if (offset == null) return;

            if (offset.blockData.collision.enabled) {
                foreach(var c in offset.blockData.collision.colliders) {
                    if(playerCollider.IsColliding(c, offset.position + new Vector3(0.5f, 0f, 0.5f), Vector3.zero)) {
                        return;
                    }
                }
            }
            

            offset.SetType(BlockTypes.StoneSlab.type); 
        }
    }
    public void RenderHighlights() {
        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out Vector3Int normal)) {
            WireframeRenderer.Instance.AddSelection(new SelectionBox(block.blockData.selection.colliders, block.position + new Vector3(0.5f, 0f, 0.5f)));
            WireframeRenderer.Instance.AddDebugSelection(new SelectionBox(BlockTypes.Stone.type.collision.colliders, block.position + new Vector3(0.5f, 0f, 0.5f) + normal));
        }

        
    }
}
