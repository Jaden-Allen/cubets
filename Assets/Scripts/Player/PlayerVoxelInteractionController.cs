using System.Collections.Generic;
using UnityEngine;

public class PlayerVoxelInteractionController : EntityComponent
{
    public Planet planet;
    public Transform playerCam;
    public EntityCollider playerCollider;
    public float range = 6f;

    public BlockType blockToPlace;

    public AudioClip defaultBlockInteractionSound;

    const float TickInterval = 0.05f;
    float timer = 0f;

    bool destroyPressed = false;
    bool placePressed = false;

    public override void OnEntityUpdate(bool ignoreLoops) {
        if (ignoreLoops) return;
        if (!planet.hasGeneratedWorld) return;

        destroyPressed |= Input.GetMouseButtonDown(0);
        placePressed |= Input.GetMouseButtonDown(1);

        timer += Time.deltaTime;
        if (timer >= TickInterval) {
            timer = 0f;

            RenderHighlights();

            if (destroyPressed) {
                DestroyVoxel();
            }
            else if (placePressed) {
                PlaceVoxel();
            }

            destroyPressed = false;
            placePressed = false;
        }
    }
    public void DestroyVoxel() {
        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out Vector3Int normal)) {
            AudioClip toPlay = block.blockData.destroyAudioClip == null ? defaultBlockInteractionSound : block.blockData.destroyAudioClip;
            SoundManager.Instance.PlaySound(toPlay, 1f, 1f, block.position + new Vector3(0.5f, 0.5f, 0.5f), 5f, 20f);
            block.SetType(BlockTypes.Air.type);
        }
    }
    public void PlaceVoxel() {
        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out Vector3Int normal)) {
            Block offset = block.Offset(normal);

            if (offset == null) return;

            if (blockToPlace.collision.enabled) {
                foreach(var c in blockToPlace.collision.colliders) {
                    Vector3 colliderPos = offset.position + new Vector3(0.5f, 0f, 0.5f);
                    if (playerCollider.IsColliding(c, colliderPos, Vector3.zero)) {
                        return;
                    }
                }
            }
            AudioClip toPlay = blockToPlace.placeAudioClip == null ? defaultBlockInteractionSound : blockToPlace.placeAudioClip;
            SoundManager.Instance.PlaySound(toPlay, 1f, 1f, offset.position + new Vector3(0.5f, 0.5f, 0.5f), 5f, 20f);
            offset.SetType(blockToPlace);
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
