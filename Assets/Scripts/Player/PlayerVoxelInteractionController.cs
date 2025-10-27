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

    public uint heldBlock = 1;

    public override void OnEntityUpdate(bool ignoreLoops) {
        if (ignoreLoops) return;
        if (!planet.hasGeneratedWorld) return;

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll < 0f) {
            heldBlock--;
        }
        else if (scroll > 0f) {
            heldBlock++;
        }
        if (heldBlock < 1) {
            heldBlock = (uint)(BlockRegistry.Blocks.Count - 1);
        }
        if (heldBlock > BlockRegistry.Blocks.Count - 1) {
            heldBlock = 1;
        }

        blockToPlace = BlockRegistry.Blocks[(int)heldBlock];

        Player player = entity as Player;
        if (Input.GetMouseButtonDown(2) && player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out _, out _)) {
            heldBlock = BlockRegistry.typeIdToIndex[block.typeId];
            blockToPlace = BlockRegistry.Blocks[(int)heldBlock];
        }

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
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out _, out Vector3Int normal)) {
            AudioClip toPlay = block.blockData.destroyAudioClip == null ? defaultBlockInteractionSound : block.blockData.destroyAudioClip;
            SoundManager.Instance.PlaySound(toPlay, 1f, 1f, block.position + new Vector3(0.5f, 0.5f, 0.5f), 5f, 20f);
            block.SetType(BlockTypes.Air.type, 0);
        }
    }
    public void PlaceVoxel() {
        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out Vector3 hit, out Vector3Int normal)) {
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
            byte rotation = GetRotation(blockToPlace, hit, normal, playerCam.forward);
            Debug.Log(rotation);
            offset.SetType(blockToPlace, rotation);
        }
    }
    public void RenderHighlights() {
        Player player = entity as Player;
        if (player.RaycastBlock(playerCam.position, playerCam.forward, range, out Block block, out _, out Vector3Int normal)) {
            WireframeRenderer.Instance.AddSelection(new SelectionBox(block.blockData.selection.colliders, block.position + new Vector3(0.5f, 0f, 0.5f)));
            WireframeRenderer.Instance.AddDebugSelection(new SelectionBox(BlockTypes.Stone.type.collision.colliders, block.position + new Vector3(0.5f, 0f, 0.5f) + normal));
        }
    }
    public byte GetRotation(BlockType blockType, Vector3 hitPosition, Vector3Int placementNormal, Vector3 playerForward) {
        if (blockType.placementType == BlockRotationPlacement.None)
            return Chunk.EncodeRotationIndex(Quaternion.identity);

        // --- CARDINAL ---
        if (blockType.placementType == BlockRotationPlacement.Cardinal) {
            Vector3 fwd = playerForward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;

            fwd.Normalize();
            float angle = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;

            Quaternion rot;
            if (angle >= 45f && angle < 135f) rot = Quaternion.Euler(0f, 90f, 0f);
            else if (angle >= 135f && angle < 225f) rot = Quaternion.Euler(0f, 180f, 0f);
            else if (angle >= 225f && angle < 315f) rot = Quaternion.Euler(0f, 270f, 0f);
            else rot = Quaternion.identity;

            return Chunk.EncodeRotationIndex(rot);
        }
        // --- PILLAR ---
        else if (blockType.placementType == BlockRotationPlacement.Pillar) {
            Quaternion rot;
            if (placementNormal == Vector3Int.forward || placementNormal == Vector3Int.back) rot = Quaternion.Euler(90f, 0f, 0f);
            else if (placementNormal == Vector3Int.up || placementNormal == Vector3Int.down) rot = Quaternion.identity;
            else if (placementNormal == Vector3Int.right || placementNormal == Vector3Int.left) rot = Quaternion.Euler(90f, 90f, 0f);
            else rot = Quaternion.identity;

            return Chunk.EncodeRotationIndex(rot);
        }
        // --- FULL ---
        else {
            Vector3 fwd = playerForward;
            fwd.y = 0f;
            if (fwd.sqrMagnitude < 0.001f) fwd = Vector3.forward;

            fwd.Normalize();
            float angle = Mathf.Atan2(fwd.x, fwd.z) * Mathf.Rad2Deg;
            angle = (angle + 360f) % 360f;

            bool topHalf = (hitPosition.y - Mathf.FloorToInt(hitPosition.y)) > 0.5f;
            float additive = topHalf ? 180f : 0f;

            Quaternion rot;
            if (angle >= 45f && angle < 135f) rot = Quaternion.Euler(additive, 90f + additive, 0f);
            else if (angle >= 135f && angle < 225f) rot = Quaternion.Euler(additive, 180f + additive, 0f);
            else if (angle >= 225f && angle < 315f) rot = Quaternion.Euler(additive, 270f + additive, 0f);
            else rot = Quaternion.Euler(additive, 0f + additive, 0f);

            return Chunk.EncodeRotationIndex(rot);
        }
    }

}
