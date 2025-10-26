using TMPro;
using UnityEngine;

public class DebugText : MonoBehaviour
{
    public Planet planet;
    public Player player;

    public TMP_Text coordsText;
    public TMP_Text targettedBlockText;
    private void Update() {
        if (!planet.hasGeneratedWorld) return;

        Vector3 coords = player.transform.position;
        coords.x = Mathf.Round(coords.x * 10f) / 10f;
        coords.y = Mathf.Round(coords.y * 10f) / 10f;
        coords.z = Mathf.Round(coords.z * 10f) / 10f;
        coordsText.text = $"Coords: {coords.x}, {coords.y}, {coords.z}";

        if (player.RaycastBlock(player.playerCam.transform.position, player.playerCam.transform.forward, 6f, out Block block, out Vector3Int normal)) {
            targettedBlockText.text = $"Targetted Block: {block.blockData.name} at {block.position.x}, {block.position.y}, {block.position.z}";
        }
        else {
            targettedBlockText.text = $"Targetted Block: None";
        }
    }
}
